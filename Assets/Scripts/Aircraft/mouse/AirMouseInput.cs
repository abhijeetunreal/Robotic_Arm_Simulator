using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

public enum SensorAxis { Pitch, Roll, Yaw }

public class AirMouseInput : MonoBehaviour
{
    [Header("1. Connection")]
    public string portName = "COM4";
    public int baudRate = 115200;

    [Header("2. Axis Orientation")]
    [Tooltip("Which sensor rotation controls horizontal (X-axis) movement.")]
    public SensorAxis horizontalAxis = SensorAxis.Yaw;
    public bool invertHorizontal = true;

    [Tooltip("Which sensor rotation controls vertical (Y-axis) movement.")]
    public SensorAxis verticalAxis = SensorAxis.Pitch;
    public bool invertVertical = false;

    [Tooltip("Which sensor rotation controls the aircraft's roll (Z-axis rotation).")]
    public SensorAxis rollAxis = SensorAxis.Roll; // NEW
    public bool invertRoll = true; // NEW

    [Header("3. Flight Controls")]
    public float sensitivity = 1.0f;
    public float rollSensitivity = 1.0f;
    [Range(0f, 5f)]
    public float deadzone = 0.5f;
    [Range(0.01f, 1.0f)]
    public float smoothingFactor = 0.15f;

    // --- Private Variables ---
    private SerialPort _arduinoPort;
    private Thread _serialThread;
    private volatile bool _isRunning = false;
    private readonly object _dataLock = new object();
    private Vector2 _latestSensorInput = Vector2.zero;
    private volatile float _latestSensorRoll = 0f;
    private Vector2 _smoothInput;
    private float _smoothRoll;
    private bool _isReady = false;

    public Vector2 GetInput()
    {
        return _smoothInput;
    }

    public float GetRollInput()
    {
        // The inversion is now handled in the thread, so we just return the clean value.
        return _smoothRoll;
    }

    void Start()
    {
        _isRunning = true;
        _serialThread = new Thread(SerialPortThread);
        _serialThread.Start();
    }

    void Update()
    {
        if (!_isReady) return;

        Vector2 currentSensorInput;
        float currentSensorRoll;
        lock (_dataLock)
        {
            currentSensorInput = _latestSensorInput;
            currentSensorRoll = _latestSensorRoll;
        }

        _smoothInput.x = Mathf.Lerp(_smoothInput.x, currentSensorInput.x, smoothingFactor);
        _smoothInput.y = Mathf.Lerp(_smoothInput.y, currentSensorInput.y, smoothingFactor);
        _smoothRoll = Mathf.Lerp(_smoothRoll, currentSensorRoll, smoothingFactor);
    }

    void OnApplicationQuit()
    {
        _isRunning = false;
        if (_serialThread != null && _serialThread.IsAlive) { _serialThread.Join(500); }
        if (_arduinoPort != null && _arduinoPort.IsOpen) { _arduinoPort.Close(); }
    }

    private void SerialPortThread()
    {
        try {
            _arduinoPort = new SerialPort(portName, baudRate) { ReadTimeout = 500 };
            _arduinoPort.Open();
            Debug.Log($"<color=green>Successfully connected to port {portName}.</color>");
            Thread.Sleep(2000); 
        } catch (Exception ex) {
            Debug.LogError($"<color=red>Connection Failed:</color> {ex.Message}");
            _isRunning = false; return;
        }

        double gxBias, gyBias, gzBias;
        if (!CalibrateGyro(out gxBias, out gyBias, out gzBias)) {
            _isRunning = false;
            if (_arduinoPort.IsOpen) _arduinoPort.Close();
            return;
        }
        _isReady = true;

        while (_isRunning) {
            try {
                string line = _arduinoPort.ReadLine();
                string[] parts = line.Split(',');

                if (parts.Length == 7) {
                    double correctedRoll = double.Parse(parts[3]) - gxBias;
                    double correctedPitch = double.Parse(parts[4]) - gyBias;
                    double correctedYaw = double.Parse(parts[5]) - gzBias;

                    // --- Updated mapping logic ---
                    double horizontalValue = GetAxisValue(horizontalAxis, correctedPitch, correctedRoll, correctedYaw);
                    double verticalValue = GetAxisValue(verticalAxis, correctedPitch, correctedRoll, correctedYaw);
                    double rollValue = GetAxisValue(rollAxis, correctedPitch, correctedRoll, correctedYaw); // NEW

                    float horizontalMultiplier = invertHorizontal ? -1f : 1f;
                    float verticalMultiplier = invertVertical ? -1f : 1f;
                    float rollMultiplier = invertRoll ? -1f : 1f; // NEW

                    float inputX = (Math.Abs(horizontalValue) > deadzone) ? (float)horizontalValue * horizontalMultiplier * sensitivity : 0;
                    float inputY = (Math.Abs(verticalValue) > deadzone) ? (float)verticalValue * verticalMultiplier * sensitivity : 0;
                    float rollInput = (Math.Abs(rollValue) > deadzone) ? (float)rollValue * rollMultiplier * rollSensitivity : 0; // UPDATED

                    lock (_dataLock) {
                        _latestSensorInput = new Vector2(inputX, inputY);
                        _latestSensorRoll = rollInput;
                    }
                }
            } catch (TimeoutException) { /* Expected */ }
            catch (Exception) { /* Ignore */ }
        }
        if (_arduinoPort.IsOpen) _arduinoPort.Close();
    }

    private double GetAxisValue(SensorAxis axis, double pitch, double roll, double yaw)
    {
        switch (axis)
        {
            case SensorAxis.Pitch: return pitch;
            case SensorAxis.Roll:  return roll;
            case SensorAxis.Yaw:   return yaw;
            default:               return 0;
        }
    }

    private bool CalibrateGyro(out double gxBias, out double gyBias, out double gzBias)
    {
        Debug.Log("<color=yellow>Calibrating... Keep sensor perfectly still!</color>");
        var samples = new List<(double gx, double gy, double gz)>();
        int calibrationSamples = 150;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (samples.Count < calibrationSamples && stopwatch.ElapsedMilliseconds < 8000) {
            try {
                string line = _arduinoPort.ReadLine();
                string[] parts = line.Split(',');
                if (parts.Length == 7) {
                    samples.Add((double.Parse(parts[3]), double.Parse(parts[4]), double.Parse(parts[5])));
                }
            } catch (TimeoutException) { }
        }

        if (samples.Count < calibrationSamples) {
            Debug.LogError($"<color=red>Calibration Failed:</color> Received only {samples.Count}/{calibrationSamples} samples.");
            gxBias = gyBias = gzBias = 0;
            return false;
        }

        gxBias = samples.Average(s => s.gx);
        gyBias = samples.Average(s => s.gy);
        gzBias = samples.Average(s => s.gz);

        Debug.Log($"<color=green>Calibration Complete!</color>");
        return true;
    }
}