using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.AirMouse
{
    /// <summary>
    /// Manages the UI interface for AirMouse input settings.
    /// This script provides a clean interface to modify AirMouseInput settings through UI elements.
    /// </summary>
    public class AirMouseUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public AirMouseUIPanel uiPanel;
        [SerializeField] public Button togglePanelButton;
        [SerializeField] public Button applySettingsButton;
        [SerializeField] public Button resetToDefaultsButton;
        
        [Header("AirMouse Input Reference")]
        [SerializeField] public AirMouseInput airMouseInput;
        
        [Header("UI State")]
        [SerializeField] private bool isPanelVisible = false;
        
        // Events
        public System.Action OnSettingsApplied;
        public System.Action OnSettingsReset;
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }
        
        private void InitializeUI()
        {
            if (uiPanel != null)
            {
                uiPanel.gameObject.SetActive(isPanelVisible);
                uiPanel.Initialize(airMouseInput);
            }
            
            if (togglePanelButton != null)
            {
                UpdateToggleButtonText();
            }
        }
        
        private void SetupEventListeners()
        {
            if (togglePanelButton != null)
            {
                togglePanelButton.onClick.AddListener(TogglePanel);
            }
            
            if (applySettingsButton != null)
            {
                applySettingsButton.onClick.AddListener(ApplySettings);
            }
            
            if (resetToDefaultsButton != null)
            {
                resetToDefaultsButton.onClick.AddListener(ResetToDefaults);
            }
        }
        
        public void TogglePanel()
        {
            isPanelVisible = !isPanelVisible;
            
            if (uiPanel != null)
            {
                uiPanel.gameObject.SetActive(isPanelVisible);
            }
            
            UpdateToggleButtonText();
        }
        
        public void ShowPanel()
        {
            isPanelVisible = true;
            if (uiPanel != null)
            {
                uiPanel.gameObject.SetActive(true);
            }
            UpdateToggleButtonText();
        }
        
        public void HidePanel()
        {
            isPanelVisible = false;
            if (uiPanel != null)
            {
                uiPanel.gameObject.SetActive(false);
            }
            UpdateToggleButtonText();
        }
        
        public void ApplySettings()
        {
            if (uiPanel != null && airMouseInput != null)
            {
                uiPanel.ApplySettingsToAirMouse();
                OnSettingsApplied?.Invoke();
                Debug.Log("AirMouse settings applied successfully!");
            }
        }
        
        public void ResetToDefaults()
        {
            if (uiPanel != null)
            {
                uiPanel.ResetToDefaults();
                OnSettingsReset?.Invoke();
                Debug.Log("AirMouse settings reset to defaults!");
            }
        }
        
        private void UpdateToggleButtonText()
        {
            if (togglePanelButton != null)
            {
                var buttonText = togglePanelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isPanelVisible ? "Hide AirMouse Settings" : "Show AirMouse Settings";
                }
            }
        }
        
        // Public getters for external access
        public bool IsPanelVisible => isPanelVisible;
        public AirMouseInput AirMouseInput => airMouseInput;
        
        // Method to refresh UI with current AirMouse settings
        public void RefreshUI()
        {
            if (uiPanel != null && airMouseInput != null)
            {
                uiPanel.RefreshFromAirMouse();
            }
        }
        
        // Method to force refresh the visual indicator
        public void RefreshVisualIndicator()
        {
            if (uiPanel != null && uiPanel.visualIndicator != null)
            {
                uiPanel.visualIndicator.ForceRefresh();
            }
        }
        
        // Context menu methods for easy testing
        [ContextMenu("Refresh Visual Indicator")]
        private void ContextRefreshVisualIndicator()
        {
            RefreshVisualIndicator();
        }
        
        [ContextMenu("Debug Visual Indicator")]
        private void ContextDebugVisualIndicator()
        {
            if (uiPanel != null && uiPanel.visualIndicator != null)
            {
                Debug.Log($"Visual Indicator Debug:");
                Debug.Log($"- AirMouseInput: {(uiPanel.visualIndicator.airMouseInput != null ? "OK" : "NULL")}");
                Debug.Log($"- MainIndicator: {(uiPanel.visualIndicator.mainIndicator != null ? "OK" : "NULL")}");
                Debug.Log($"- IndicatorImage: {(uiPanel.visualIndicator.indicatorImage != null ? "OK" : "NULL")}");
                Debug.Log($"- InputValueText: {(uiPanel.visualIndicator.inputValueText != null ? "OK" : "NULL")}");
                
                if (uiPanel.visualIndicator.airMouseInput != null)
                {
                    Vector2 input = uiPanel.visualIndicator.airMouseInput.GetInput();
                    float roll = uiPanel.visualIndicator.airMouseInput.GetRollInput();
                    Debug.Log($"- Current Input: X={input.x:F3}, Y={input.y:F3}, Roll={roll:F3}");
                }
            }
        }
        
        [ContextMenu("Force Visual Connected")]
        private void ContextForceVisualConnected()
        {
            if (uiPanel != null && uiPanel.visualIndicator != null)
            {
                uiPanel.visualIndicator.ForceConnected();
                Debug.Log("Visual indicator forced to connected status");
            }
        }
        
        [ContextMenu("Test Visual Movement")]
        private void ContextTestVisualMovement()
        {
            if (uiPanel != null && uiPanel.visualIndicator != null)
            {
                // Test movement by setting a test position
                if (uiPanel.visualIndicator.mainIndicator != null)
                {
                    uiPanel.visualIndicator.mainIndicator.anchoredPosition = new Vector2(50, 50);
                    uiPanel.visualIndicator.mainIndicator.rotation = Quaternion.Euler(0, 0, 45);
                    Debug.Log("Test movement applied to visual indicator");
                }
            }
        }
        
        [ContextMenu("Test Visual with Input")]
        private void ContextTestVisualWithInput()
        {
            if (uiPanel != null && uiPanel.visualIndicator != null)
            {
                // Test with sample input values
                Vector2 testInput = new Vector2(0.5f, -0.3f);
                float testRoll = 0.8f;
                uiPanel.visualIndicator.TestVisualIndicator(testInput, testRoll);
            }
        }
        
        [ContextMenu("Test Visual with Random Input")]
        private void ContextTestVisualWithRandomInput()
        {
            if (uiPanel != null && uiPanel.visualIndicator != null)
            {
                // Test with random input values
                Vector2 testInput = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                float testRoll = Random.Range(-1f, 1f);
                uiPanel.visualIndicator.TestVisualIndicator(testInput, testRoll);
            }
        }
        
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (togglePanelButton != null)
            {
                togglePanelButton.onClick.RemoveListener(TogglePanel);
            }
            
            if (applySettingsButton != null)
            {
                applySettingsButton.onClick.RemoveListener(ApplySettings);
            }
            
            if (resetToDefaultsButton != null)
            {
                resetToDefaultsButton.onClick.RemoveListener(ResetToDefaults);
            }
        }
    }
}
