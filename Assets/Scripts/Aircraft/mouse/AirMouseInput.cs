using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

public enum ConnectionStatus { Disconnected, Connecting, Connected, Failed }
public enum SensorAxis { Pitch, Roll, Yaw }

public class AirMouseInput : MonoBehaviour
{
    [Header("1. Connection")]
    public string portName = "COM3";
    public int baudRate = 115200;

    [Header("2. Axis Orientation")]
    public SensorAxis horizontalAxis = SensorAxis.Yaw;
    public bool invertHorizontal = true;
    public SensorAxis verticalAxis = SensorAxis.Pitch;
    public bool invertVertical = false;
    public SensorAxis rollAxis = SensorAxis.Roll;
    public bool invertRoll = true;

    [Header("3. Flight Controls")]
    public float sensitivity = 1.0f;
    public float rollSensitivity = 1.0f;
    [Range(0f, 5f)]
    public float deadzone = 0.5f;
    [Range(0.01f, 1.0f)]
    public float smoothingFactor = 0.15f;
    
    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;
    public volatile string RawDataString;

    private readonly ConcurrentQueue<string> _commandQueue = new ConcurrentQueue<string>();
    private SerialPort _arduinoPort;
    private Thread _serialThread;
    private volatile bool _isRunning = false;
    private readonly object _dataLock = new object();
    private Vector2 _latestSensorInput = Vector2.zero;
    private volatile float _latestSensorRoll = 0f;
    private Vector2 _smoothInput;
    private float _smoothRoll;
    
    public void SendVibrationCommand(int intensity, int durationMs)
    {
        if (Status == ConnectionStatus.Connected && durationMs > 0)
        {
            intensity = Mathf.Clamp(intensity, 0, 255);
            _commandQueue.Enqueue($"V,{intensity},{durationMs}\n");
        }
    }
    
    void OnEnable() {
        Status = ConnectionStatus.Connecting;
        _isRunning = true;
        _serialThread = new Thread(SerialPortThread);
        _serialThread.Start();
    }
    void OnDisable() {
        _isRunning = false;
        if (_serialThread != null && _serialThread.IsAlive) _serialThread.Join(500);
        if (_arduinoPort != null && _arduinoPort.IsOpen) _arduinoPort.Close();
        Status = ConnectionStatus.Disconnected;
        Debug.Log("AirMouseInput Disabled and Cleaned Up.");
    }
    void Update() {
        if (Status != ConnectionStatus.Connected) return;
        Vector2 currentSensorInput;
        float currentSensorRoll;
        lock (_dataLock) {
            currentSensorInput = _latestSensorInput;
            currentSensorRoll = _latestSensorRoll;
        }
        _smoothInput.x = Mathf.Lerp(_smoothInput.x, currentSensorInput.x, smoothingFactor);
        _smoothInput.y = Mathf.Lerp(_smoothInput.y, currentSensorInput.y, smoothingFactor);
        _smoothRoll = Mathf.Lerp(_smoothRoll, currentSensorRoll, smoothingFactor);
    }
    public Vector2 GetInput() { return _smoothInput; }
    public float GetRollInput() { return _smoothRoll; }

    private void SerialPortThread()
    {
        // **FIX:** The incorrect, extra command check that was here has been REMOVED.

        try {
            _arduinoPort = new SerialPort(portName, baudRate) { ReadTimeout = 2000 };
            _arduinoPort.Open();
        } catch (Exception ex) {
            Debug.LogError($"Connection Failed: {ex.Message}");
            Status = ConnectionStatus.Failed;
            _isRunning = false;
            return;
        }

        if (!CalibrateGyro(out double gxBias, out double gyBias, out double gzBias)) {
            Status = ConnectionStatus.Failed;
            _isRunning = false;
            if (_arduinoPort.IsOpen) _arduinoPort.Close();
            return;
        }

        Status = ConnectionStatus.Connected;

        while (_isRunning)
        {
            // This is the single, CORRECT place to check for and send commands.
            if (_commandQueue.TryDequeue(out string command))
            {
                try { _arduinoPort.Write(command); } catch (Exception) { /* Ignore write errors */ }
            }
            
            try {
                 string line = _arduinoPort.ReadLine();
                 RawDataString = line;
                 string[] parts = line.Split(',');
                 if (parts.Length == 7) {
                     double correctedRoll = double.Parse(parts[3]) - gxBias;
                     double correctedPitch = double.Parse(parts[4]) - gyBias;
                     double correctedYaw = double.Parse(parts[5]) - gzBias;
                     double horizontalValue = GetAxisValue(horizontalAxis, correctedPitch, correctedRoll, correctedYaw);
                     double verticalValue = GetAxisValue(verticalAxis, correctedPitch, correctedRoll, correctedYaw);
                     double rollValue = GetAxisValue(rollAxis, correctedPitch, correctedRoll, correctedYaw);
                     float horizontalMultiplier = invertHorizontal ? -1f : 1f;
                     float verticalMultiplier = invertVertical ? -1f : 1f;
                     float rollMultiplier = invertRoll ? -1f : 1f;
                     float inputX = (Math.Abs(horizontalValue) > deadzone) ? (float)horizontalValue * horizontalMultiplier * sensitivity : 0;
                     float inputY = (Math.Abs(verticalValue) > deadzone) ? (float)verticalValue * verticalMultiplier * sensitivity : 0;
                     float rollInput = (Math.Abs(rollValue) > deadzone) ? (float)rollValue * rollMultiplier * rollSensitivity : 0;
                     lock (_dataLock) {
                         _latestSensorInput = new Vector2(inputX, inputY);
                         _latestSensorRoll = rollInput;
                     }
                 }
            } catch (TimeoutException) {}
            catch (Exception) {}
        }
        if (_arduinoPort != null && _arduinoPort.IsOpen) _arduinoPort.Close();
    }
    
    private double GetAxisValue(SensorAxis axis, double pitch, double roll, double yaw) {
        switch (axis) {
            case SensorAxis.Pitch: return pitch;
            case SensorAxis.Roll: return roll;
            case SensorAxis.Yaw: return yaw;
            default: return 0;
        }
    }
    private bool CalibrateGyro(out double gxBias, out double gyBias, out double gzBias) {
        Debug.Log("Calibrating...");
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
            Debug.LogError($"Calibration Failed: Received only {samples.Count}/{calibrationSamples} samples.");
            gxBias = gyBias = gzBias = 0;
            return false;
        }
        gxBias = samples.Average(s => s.gx);
        gyBias = samples.Average(s => s.gy);
        gzBias = samples.Average(s => s.gz);
        Debug.Log("Calibration Complete!");
        return true;
    }
}