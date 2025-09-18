using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AirMouseUIController : MonoBehaviour
{
    [Header("Target Script References")]
    [Tooltip("Drag the GameObject that has the AirMouseInput script here.")]
    public AirMouseInput airMouseInput;
    [Tooltip("Drag the GameObject with the AssemblyGameManager script here.")]
    public AssemblyGameManager gameManager; // NEW: Reference to the GameManager

    [Header("Air Mouse UI References")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider rollSensitivitySlider;
    public TextMeshProUGUI rollSensitivityValueText;
    public Slider deadzoneSlider;
    public TextMeshProUGUI deadzoneValueText;
    public Slider smoothingSlider;
    public TextMeshProUGUI smoothingValueText;
    public TMP_Dropdown horizontalAxisDropdown;
    public TMP_Dropdown verticalAxisDropdown;
    public TMP_Dropdown rollAxisDropdown;
    public Toggle invertHorizontalToggle;
    public Toggle invertVerticalToggle;
    public Toggle invertRollToggle;

    [Header("NEW: Haptic Feedback UI References")]
    public Slider pickupIntensitySlider;
    public TextMeshProUGUI pickupIntensityValueText;
    public TMP_InputField pickupDurationInput;
    public Slider assemblyIntensitySlider;
    public TextMeshProUGUI assemblyIntensityValueText;
    public TMP_InputField assemblyDurationInput;

    void Start()
    {
        if (airMouseInput == null || gameManager == null)
        {
            Debug.LogError("A required script reference (AirMouseInput or GameManager) is not set on the UI Controller!", this);
            this.enabled = false;
            return;
        }

        InitializeUI();
        SetupListeners();
    }

    void InitializeUI()
    {
        // --- Air Mouse Settings ---
        sensitivitySlider.value = airMouseInput.sensitivity;
        sensitivityValueText.text = airMouseInput.sensitivity.ToString("F2");
        rollSensitivitySlider.value = airMouseInput.rollSensitivity;
        rollSensitivityValueText.text = airMouseInput.rollSensitivity.ToString("F2");
        deadzoneSlider.value = airMouseInput.deadzone;
        deadzoneValueText.text = airMouseInput.deadzone.ToString("F2");
        smoothingSlider.value = airMouseInput.smoothingFactor;
        smoothingValueText.text = airMouseInput.smoothingFactor.ToString("F2");
        invertHorizontalToggle.isOn = airMouseInput.invertHorizontal;
        invertVerticalToggle.isOn = airMouseInput.invertVertical;
        invertRollToggle.isOn = airMouseInput.invertRoll;
        horizontalAxisDropdown.ClearOptions();
        verticalAxisDropdown.ClearOptions();
        rollAxisDropdown.ClearOptions();
        var axisOptions = Enum.GetNames(typeof(SensorAxis));
        var optionsList = new System.Collections.Generic.List<string>(axisOptions);
        horizontalAxisDropdown.AddOptions(optionsList);
        verticalAxisDropdown.AddOptions(optionsList);
        rollAxisDropdown.AddOptions(optionsList);
        horizontalAxisDropdown.value = (int)airMouseInput.horizontalAxis;
        verticalAxisDropdown.value = (int)airMouseInput.verticalAxis;
        rollAxisDropdown.value = (int)airMouseInput.rollAxis;
        horizontalAxisDropdown.RefreshShownValue();
        verticalAxisDropdown.RefreshShownValue();
        rollAxisDropdown.RefreshShownValue();

        // --- NEW: Haptic Feedback Settings ---
        pickupIntensitySlider.value = gameManager.pickupVibrationIntensity;
        pickupIntensityValueText.text = gameManager.pickupVibrationIntensity.ToString();
        pickupDurationInput.text = gameManager.pickupVibrationDuration.ToString();

        assemblyIntensitySlider.value = gameManager.assemblyVibrationIntensity;
        assemblyIntensityValueText.text = gameManager.assemblyVibrationIntensity.ToString();
        assemblyDurationInput.text = gameManager.assemblyVibrationDuration.ToString();
    }

    void SetupListeners()
    {
        // Air Mouse Listeners
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        rollSensitivitySlider.onValueChanged.AddListener(OnRollSensitivityChanged);
        deadzoneSlider.onValueChanged.AddListener(OnDeadzoneChanged);
        smoothingSlider.onValueChanged.AddListener(OnSmoothingChanged);
        invertHorizontalToggle.onValueChanged.AddListener(OnInvertHorizontalChanged);
        invertVerticalToggle.onValueChanged.AddListener(OnInvertVerticalChanged);
        invertRollToggle.onValueChanged.AddListener(OnInvertRollChanged);
        horizontalAxisDropdown.onValueChanged.AddListener(OnHorizontalAxisChanged);
        verticalAxisDropdown.onValueChanged.AddListener(OnVerticalAxisChanged);
        rollAxisDropdown.onValueChanged.AddListener(OnRollAxisChanged);

        // --- NEW: Haptic Feedback Listeners ---
        pickupIntensitySlider.onValueChanged.AddListener(OnPickupIntensityChanged);
        pickupDurationInput.onValueChanged.AddListener(OnPickupDurationChanged);
        assemblyIntensitySlider.onValueChanged.AddListener(OnAssemblyIntensityChanged);
        assemblyDurationInput.onValueChanged.AddListener(OnAssemblyDurationChanged);
    }
    
    // --- Air Mouse Control Functions ---
    public void OnSensitivityChanged(float value) { airMouseInput.sensitivity = value; sensitivityValueText.text = value.ToString("F2"); }
    public void OnRollSensitivityChanged(float value) { airMouseInput.rollSensitivity = value; rollSensitivityValueText.text = value.ToString("F2"); }
    public void OnDeadzoneChanged(float value) { airMouseInput.deadzone = value; deadzoneValueText.text = value.ToString("F2"); }
    public void OnSmoothingChanged(float value) { airMouseInput.smoothingFactor = value; smoothingValueText.text = value.ToString("F2"); }
    public void OnInvertHorizontalChanged(bool value) { airMouseInput.invertHorizontal = value; }
    public void OnInvertVerticalChanged(bool value) { airMouseInput.invertVertical = value; }
    public void OnInvertRollChanged(bool value) { airMouseInput.invertRoll = value; }
    public void OnHorizontalAxisChanged(int index) { airMouseInput.horizontalAxis = (SensorAxis)index; }
    public void OnVerticalAxisChanged(int index) { airMouseInput.verticalAxis = (SensorAxis)index; }
    public void OnRollAxisChanged(int index) { airMouseInput.rollAxis = (SensorAxis)index; }

    // --- NEW: Haptic Feedback Control Functions ---
    public void OnPickupIntensityChanged(float value) {
        int intValue = Mathf.RoundToInt(value);
        gameManager.pickupVibrationIntensity = intValue;
        pickupIntensityValueText.text = intValue.ToString();
    }
    public void OnPickupDurationChanged(string value) {
        if (int.TryParse(value, out int duration)) gameManager.pickupVibrationDuration = duration;
    }
    public void OnAssemblyIntensityChanged(float value) {
        int intValue = Mathf.RoundToInt(value);
        gameManager.assemblyVibrationIntensity = intValue;
        assemblyIntensityValueText.text = intValue.ToString();
    }
    public void OnAssemblyDurationChanged(string value) {
        if (int.TryParse(value, out int duration)) gameManager.assemblyVibrationDuration = duration;
    }

    void OnDestroy() {
        // ... (Clean up listeners if necessary, though AddListener handles this well)
    }
}