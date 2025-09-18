using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Required for Queue
using System.Text; // Required for StringBuilder

public class MainMenuController : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject mainMenuPanel;
    public GameObject nextPanel;

    [Header("Component References")]
    public AirMouseInput airMouseInput;
    public TargetController targetController;

    [Header("UI Element References")]
    public TMP_InputField portInputField;
    public TMP_InputField baudRateInputField;
    public Button connectButton;
    public Toggle useMouseToggle;
    public TextMeshProUGUI statusText;
    public Button continueButton;
    
    [Header("Raw Data Display")]
    public GameObject rawDataDisplayPanel;
    public TextMeshProUGUI rawDataText;
    
    [Tooltip("The maximum number of raw data lines to show in the log.")]
    [Range(1, 30)]
    public int maxDataLines = 7; // NEW: Control the number of lines from the Inspector

    private bool isReadyToContinue = false;

    // NEW: A queue to store the recent data lines and a variable to track the last update.
    private Queue<string> dataLines = new Queue<string>();
    private string lastDataString = "";


    void Start()
    {
        // ... (Start method remains the same)
        if (airMouseInput != null) airMouseInput.enabled = false;
        portInputField.text = "COM3";
        baudRateInputField.text = "115200";
        connectButton.onClick.AddListener(OnConnectButtonPressed);
        useMouseToggle.onValueChanged.AddListener(OnUseMouseToggleChanged);
        continueButton.onClick.AddListener(OnContinueButtonPressed);
        if (rawDataDisplayPanel != null) rawDataDisplayPanel.SetActive(false);
        UpdateUIState();
    }

    void Update()
    {
        // MODIFIED: The update logic is now smarter.
        if (airMouseInput != null && airMouseInput.Status == ConnectionStatus.Connected)
        {
            string currentData = airMouseInput.RawDataString;

            // Only update if the data is new and not empty, to prevent spamming the log.
            if (!string.IsNullOrEmpty(currentData) && currentData != lastDataString)
            {
                dataLines.Enqueue(currentData); // Add the new line to the end of the queue.
                lastDataString = currentData; // Remember this line as the last one we've seen.

                // If the queue has too many lines, remove the oldest one from the front.
                while (dataLines.Count > maxDataLines)
                {
                    dataLines.Dequeue();
                }

                UpdateRawDataDisplay(); // Update the text element.
            }
        }
    }
    
    // NEW: A helper method to build and display the text.
    void UpdateRawDataDisplay()
    {
        if (rawDataText == null) return;
        
        // Use a StringBuilder for efficient string building.
        var sb = new StringBuilder();
        foreach(string line in dataLines)
        {
            sb.AppendLine(line); // Append each line from our queue.
        }
        rawDataText.text = sb.ToString();
    }

    // Coroutine is slightly modified to clear the log on success/failure.
    private IEnumerator AttemptConnectionCoroutine(string port, int baudRate)
    {
        statusText.text = "<color=yellow>Connecting...</color>";
        isReadyToContinue = false;
        if (rawDataDisplayPanel != null) rawDataDisplayPanel.SetActive(false);
        
        // NEW: Clear any old data from the log.
        dataLines.Clear();
        UpdateRawDataDisplay();
        
        UpdateUIState();
        connectButton.interactable = false;

        airMouseInput.portName = port;
        airMouseInput.baudRate = baudRate;
        airMouseInput.enabled = true;

        float timeout = 5.0f;
        float elapsedTime = 0f;
        
        while (airMouseInput.Status == ConnectionStatus.Connecting && elapsedTime < timeout)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (airMouseInput.Status == ConnectionStatus.Connected)
        {
            statusText.text = "<color=green>Air Mouse Connected!</color>";
            targetController.currentControlMode = ControlMode.AirMouse;
            isReadyToContinue = true;
            if (rawDataDisplayPanel != null) rawDataDisplayPanel.SetActive(true);
        }
        else
        {
            statusText.text = "<color=red>Connection Failed. Check port & device.</color>";
            airMouseInput.enabled = false;
        }
        
        connectButton.interactable = true;
        UpdateUIState();
    }
    
    private void OnUseMouseToggleChanged(bool ison)
    {
        if (rawDataDisplayPanel != null) rawDataDisplayPanel.SetActive(false);
        
        // NEW: Clear the log when switching modes.
        dataLines.Clear();
        UpdateRawDataDisplay();

        if (ison)
        {
            statusText.text = "<color=green>Using Mouse Control</color>";
            targetController.currentControlMode = ControlMode.Mouse;
            isReadyToContinue = true;
            if(airMouseInput.enabled) airMouseInput.enabled = false;
        }
        else
        {
            isReadyToContinue = false;
            statusText.text = "Awaiting connection...";
        }
        UpdateUIState();
    }
    
    // The rest of the script remains the same.
    private void OnConnectButtonPressed() {
        if (useMouseToggle.isOn) useMouseToggle.isOn = false;
        string port = portInputField.text;
        if (string.IsNullOrEmpty(port)) {
            statusText.text = "<color=red>Port name cannot be empty.</color>";
            return;
        }
        if (!int.TryParse(baudRateInputField.text, out int baudRate)) {
            statusText.text = "<color=red>Invalid Baud Rate. Must be a number.</color>";
            return;
        }
        StartCoroutine(AttemptConnectionCoroutine(port, baudRate));
    }
    private void OnContinueButtonPressed() {
        mainMenuPanel.SetActive(false);
        nextPanel.SetActive(true);
    }
    void UpdateUIState() {
        continueButton.interactable = isReadyToContinue;
        bool airMouseSettingsActive = !useMouseToggle.isOn;
        portInputField.interactable = airMouseSettingsActive;
        baudRateInputField.interactable = airMouseSettingsActive;
        connectButton.interactable = airMouseSettingsActive && (airMouseInput == null || airMouseInput.Status != ConnectionStatus.Connected);
    }
}