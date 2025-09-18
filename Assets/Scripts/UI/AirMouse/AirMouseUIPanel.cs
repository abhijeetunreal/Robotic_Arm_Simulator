using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.AirMouse
{
    /// <summary>
    /// UI Panel that contains all the AirMouse input settings controls.
    /// This panel provides a user-friendly interface to modify all AirMouseInput parameters.
    /// </summary>
    public class AirMouseUIPanel : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] public TMP_InputField portNameInput;
        [SerializeField] public TMP_InputField baudRateInput;
        
        [Header("Axis Orientation Settings")]
        [SerializeField] public TMP_Dropdown horizontalAxisDropdown;
        [SerializeField] public Toggle invertHorizontalToggle;
        [SerializeField] public TMP_Dropdown verticalAxisDropdown;
        [SerializeField] public Toggle invertVerticalToggle;
        [SerializeField] public TMP_Dropdown rollAxisDropdown;
        [SerializeField] public Toggle invertRollToggle;
        
        [Header("Flight Control Settings")]
        [SerializeField] public Slider sensitivitySlider;
        [SerializeField] public TextMeshProUGUI sensitivityValueText;
        [SerializeField] public Slider rollSensitivitySlider;
        [SerializeField] public TextMeshProUGUI rollSensitivityValueText;
        [SerializeField] public Slider deadzoneSlider;
        [SerializeField] public TextMeshProUGUI deadzoneValueText;
        [SerializeField] public Slider smoothingFactorSlider;
        [SerializeField] public TextMeshProUGUI smoothingFactorValueText;
        
        [Header("Status Display")]
        [SerializeField] public TextMeshProUGUI connectionStatusText;
        [SerializeField] public TextMeshProUGUI currentInputText;
        
        [Header("Visual Indicator")]
        [SerializeField] public AirMouseVisualIndicator visualIndicator;
        
        private AirMouseInput airMouseInput;
        private bool isInitialized = false;
        
        // Default values for reset functionality
        private readonly string defaultPortName = "COM4";
        private readonly int defaultBaudRate = 115200;
        private readonly SensorAxis defaultHorizontalAxis = SensorAxis.Yaw;
        private readonly bool defaultInvertHorizontal = true;
        private readonly SensorAxis defaultVerticalAxis = SensorAxis.Pitch;
        private readonly bool defaultInvertVertical = false;
        private readonly SensorAxis defaultRollAxis = SensorAxis.Roll;
        private readonly bool defaultInvertRoll = true;
        private readonly float defaultSensitivity = 1.0f;
        private readonly float defaultRollSensitivity = 1.0f;
        private readonly float defaultDeadzone = 0.5f;
        private readonly float defaultSmoothingFactor = 0.15f;
        
        public void Initialize(AirMouseInput airMouse)
        {
            airMouseInput = airMouse;
            SetupDropdowns();
            SetupSliders();
            RefreshFromAirMouse();
            
            // Initialize visual indicator
            if (visualIndicator != null)
            {
                visualIndicator.Initialize(airMouseInput);
            }
            
            isInitialized = true;
        }
        
        private void SetupDropdowns()
        {
            // Setup axis dropdowns
            if (horizontalAxisDropdown != null)
            {
                horizontalAxisDropdown.ClearOptions();
                horizontalAxisDropdown.AddOptions(new System.Collections.Generic.List<string> { "Pitch", "Roll", "Yaw" });
            }
            
            if (verticalAxisDropdown != null)
            {
                verticalAxisDropdown.ClearOptions();
                verticalAxisDropdown.AddOptions(new System.Collections.Generic.List<string> { "Pitch", "Roll", "Yaw" });
            }
            
            if (rollAxisDropdown != null)
            {
                rollAxisDropdown.ClearOptions();
                rollAxisDropdown.AddOptions(new System.Collections.Generic.List<string> { "Pitch", "Roll", "Yaw" });
            }
        }
        
        private void SetupSliders()
        {
            // Setup sensitivity slider
            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = 0.1f;
                sensitivitySlider.maxValue = 5.0f;
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }
            
            // Setup roll sensitivity slider
            if (rollSensitivitySlider != null)
            {
                rollSensitivitySlider.minValue = 0.1f;
                rollSensitivitySlider.maxValue = 5.0f;
                rollSensitivitySlider.onValueChanged.AddListener(OnRollSensitivityChanged);
            }
            
            // Setup deadzone slider
            if (deadzoneSlider != null)
            {
                deadzoneSlider.minValue = 0f;
                deadzoneSlider.maxValue = 5f;
                deadzoneSlider.onValueChanged.AddListener(OnDeadzoneChanged);
            }
            
            // Setup smoothing factor slider
            if (smoothingFactorSlider != null)
            {
                smoothingFactorSlider.minValue = 0.01f;
                smoothingFactorSlider.maxValue = 1.0f;
                smoothingFactorSlider.onValueChanged.AddListener(OnSmoothingFactorChanged);
            }
        }
        
        public void RefreshFromAirMouse()
        {
            if (airMouseInput == null) return;
            
            // Connection settings
            if (portNameInput != null)
                portNameInput.text = airMouseInput.portName;
            
            if (baudRateInput != null)
                baudRateInput.text = airMouseInput.baudRate.ToString();
            
            // Axis orientation settings
            if (horizontalAxisDropdown != null)
                horizontalAxisDropdown.value = (int)airMouseInput.horizontalAxis;
            
            if (invertHorizontalToggle != null)
                invertHorizontalToggle.isOn = airMouseInput.invertHorizontal;
            
            if (verticalAxisDropdown != null)
                verticalAxisDropdown.value = (int)airMouseInput.verticalAxis;
            
            if (invertVerticalToggle != null)
                invertVerticalToggle.isOn = airMouseInput.invertVertical;
            
            if (rollAxisDropdown != null)
                rollAxisDropdown.value = (int)airMouseInput.rollAxis;
            
            if (invertRollToggle != null)
                invertRollToggle.isOn = airMouseInput.invertRoll;
            
            // Flight control settings
            if (sensitivitySlider != null)
            {
                sensitivitySlider.value = airMouseInput.sensitivity;
                UpdateSensitivityText(airMouseInput.sensitivity);
            }
            
            if (rollSensitivitySlider != null)
            {
                rollSensitivitySlider.value = airMouseInput.rollSensitivity;
                UpdateRollSensitivityText(airMouseInput.rollSensitivity);
            }
            
            if (deadzoneSlider != null)
            {
                deadzoneSlider.value = airMouseInput.deadzone;
                UpdateDeadzoneText(airMouseInput.deadzone);
            }
            
            if (smoothingFactorSlider != null)
            {
                smoothingFactorSlider.value = airMouseInput.smoothingFactor;
                UpdateSmoothingFactorText(airMouseInput.smoothingFactor);
            }
        }
        
        public void ApplySettingsToAirMouse()
        {
            if (airMouseInput == null) return;
            
            // Connection settings
            if (portNameInput != null)
                airMouseInput.portName = portNameInput.text;
            
            if (baudRateInput != null && int.TryParse(baudRateInput.text, out int baudRate))
                airMouseInput.baudRate = baudRate;
            
            // Axis orientation settings
            if (horizontalAxisDropdown != null)
                airMouseInput.horizontalAxis = (SensorAxis)horizontalAxisDropdown.value;
            
            if (invertHorizontalToggle != null)
                airMouseInput.invertHorizontal = invertHorizontalToggle.isOn;
            
            if (verticalAxisDropdown != null)
                airMouseInput.verticalAxis = (SensorAxis)verticalAxisDropdown.value;
            
            if (invertVerticalToggle != null)
                airMouseInput.invertVertical = invertVerticalToggle.isOn;
            
            if (rollAxisDropdown != null)
                airMouseInput.rollAxis = (SensorAxis)rollAxisDropdown.value;
            
            if (invertRollToggle != null)
                airMouseInput.invertRoll = invertRollToggle.isOn;
            
            // Flight control settings
            if (sensitivitySlider != null)
                airMouseInput.sensitivity = sensitivitySlider.value;
            
            if (rollSensitivitySlider != null)
                airMouseInput.rollSensitivity = rollSensitivitySlider.value;
            
            if (deadzoneSlider != null)
                airMouseInput.deadzone = deadzoneSlider.value;
            
            if (smoothingFactorSlider != null)
                airMouseInput.smoothingFactor = smoothingFactorSlider.value;
        }
        
        public void ResetToDefaults()
        {
            // Connection settings
            if (portNameInput != null)
                portNameInput.text = defaultPortName;
            
            if (baudRateInput != null)
                baudRateInput.text = defaultBaudRate.ToString();
            
            // Axis orientation settings
            if (horizontalAxisDropdown != null)
                horizontalAxisDropdown.value = (int)defaultHorizontalAxis;
            
            if (invertHorizontalToggle != null)
                invertHorizontalToggle.isOn = defaultInvertHorizontal;
            
            if (verticalAxisDropdown != null)
                verticalAxisDropdown.value = (int)defaultVerticalAxis;
            
            if (invertVerticalToggle != null)
                invertVerticalToggle.isOn = defaultInvertVertical;
            
            if (rollAxisDropdown != null)
                rollAxisDropdown.value = (int)defaultRollAxis;
            
            if (invertRollToggle != null)
                invertRollToggle.isOn = defaultInvertRoll;
            
            // Flight control settings
            if (sensitivitySlider != null)
            {
                sensitivitySlider.value = defaultSensitivity;
                UpdateSensitivityText(defaultSensitivity);
            }
            
            if (rollSensitivitySlider != null)
            {
                rollSensitivitySlider.value = defaultRollSensitivity;
                UpdateRollSensitivityText(defaultRollSensitivity);
            }
            
            if (deadzoneSlider != null)
            {
                deadzoneSlider.value = defaultDeadzone;
                UpdateDeadzoneText(defaultDeadzone);
            }
            
            if (smoothingFactorSlider != null)
            {
                smoothingFactorSlider.value = defaultSmoothingFactor;
                UpdateSmoothingFactorText(defaultSmoothingFactor);
            }
        }
        
        // Slider event handlers
        private void OnSensitivityChanged(float value)
        {
            UpdateSensitivityText(value);
        }
        
        private void OnRollSensitivityChanged(float value)
        {
            UpdateRollSensitivityText(value);
        }
        
        private void OnDeadzoneChanged(float value)
        {
            UpdateDeadzoneText(value);
        }
        
        private void OnSmoothingFactorChanged(float value)
        {
            UpdateSmoothingFactorText(value);
        }
        
        // Text update methods
        private void UpdateSensitivityText(float value)
        {
            if (sensitivityValueText != null)
                sensitivityValueText.text = value.ToString("F2");
        }
        
        private void UpdateRollSensitivityText(float value)
        {
            if (rollSensitivityValueText != null)
                rollSensitivityValueText.text = value.ToString("F2");
        }
        
        private void UpdateDeadzoneText(float value)
        {
            if (deadzoneValueText != null)
                deadzoneValueText.text = value.ToString("F2");
        }
        
        private void UpdateSmoothingFactorText(float value)
        {
            if (smoothingFactorValueText != null)
                smoothingFactorValueText.text = value.ToString("F2");
        }
        
        // Status update methods (can be called from external scripts)
        public void UpdateConnectionStatus(string status, Color color)
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = status;
                connectionStatusText.color = color;
            }
        }
        
        public void UpdateCurrentInput(Vector2 input, float roll)
        {
            if (currentInputText != null)
            {
                currentInputText.text = $"Input: X={input.x:F2}, Y={input.y:F2}, Roll={roll:F2}";
            }
        }
        
        private void Update()
        {
            // Update status display if initialized
            if (isInitialized && airMouseInput != null)
            {
                UpdateCurrentInput(airMouseInput.GetInput(), airMouseInput.GetRollInput());
            }
        }
        
        private void OnDestroy()
        {
            // Clean up slider event listeners
            if (sensitivitySlider != null)
                sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
            
            if (rollSensitivitySlider != null)
                rollSensitivitySlider.onValueChanged.RemoveListener(OnRollSensitivityChanged);
            
            if (deadzoneSlider != null)
                deadzoneSlider.onValueChanged.RemoveListener(OnDeadzoneChanged);
            
            if (smoothingFactorSlider != null)
                smoothingFactorSlider.onValueChanged.RemoveListener(OnSmoothingFactorChanged);
        }
    }
}
