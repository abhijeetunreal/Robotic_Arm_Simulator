using UnityEngine;
using UnityEngine.UI;

namespace UI.AirMouse
{
    /// <summary>
    /// Example script showing how to use the AirMouse UI system.
    /// This script demonstrates how to integrate the UI with your game.
    /// </summary>
    public class AirMouseUIExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private AirMouseUIManager uiManager;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoFindComponents = true;
        [SerializeField] private bool createUIOnStart = false;
        
        private void Start()
        {
            if (autoFindComponents)
            {
                FindComponents();
            }
            
            if (createUIOnStart)
            {
                CreateUI();
            }
            
            SetupEventListeners();
        }
        
        private void FindComponents()
        {
            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<AirMouseUIManager>();
            }
            
            if (uiManager == null)
            {
                Debug.LogWarning("No AirMouseUIManager found. Please assign one or create the UI.");
            }
        }
        
        private void CreateUI()
        {
            // Find or create a canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            
            // Create the UI prefab setup
            GameObject setupObject = new GameObject("AirMouseUISetup");
            AirMouseUIPrefabSetup setup = setupObject.AddComponent<AirMouseUIPrefabSetup>();
            setup.parentCanvas = canvas.transform;
            setup.createUIPrefab = true;
            
            // Find the created UI manager
            uiManager = FindFirstObjectByType<AirMouseUIManager>();
        }
        
        private void SetupEventListeners()
        {
            if (uiManager != null)
            {
                uiManager.OnSettingsApplied += OnSettingsApplied;
                uiManager.OnSettingsReset += OnSettingsReset;
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(toggleKey) && uiManager != null)
            {
                uiManager.TogglePanel();
            }
        }
        
        private void OnSettingsApplied()
        {
            Debug.Log("AirMouse settings have been applied!");
            // Add any custom logic here when settings are applied
        }
        
        private void OnSettingsReset()
        {
            Debug.Log("AirMouse settings have been reset to defaults!");
            // Add any custom logic here when settings are reset
        }
        
        // Public methods for external access
        public void ShowAirMouseSettings()
        {
            if (uiManager != null)
            {
                uiManager.ShowPanel();
            }
        }
        
        public void HideAirMouseSettings()
        {
            if (uiManager != null)
            {
                uiManager.HidePanel();
            }
        }
        
        public void RefreshAirMouseUI()
        {
            if (uiManager != null)
            {
                uiManager.RefreshUI();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (uiManager != null)
            {
                uiManager.OnSettingsApplied -= OnSettingsApplied;
                uiManager.OnSettingsReset -= OnSettingsReset;
            }
        }
        
        // Context menu methods for easy testing
        [ContextMenu("Show AirMouse Settings")]
        private void ContextShowSettings()
        {
            ShowAirMouseSettings();
        }
        
        [ContextMenu("Hide AirMouse Settings")]
        private void ContextHideSettings()
        {
            HideAirMouseSettings();
        }
        
        [ContextMenu("Refresh UI")]
        private void ContextRefreshUI()
        {
            RefreshAirMouseUI();
        }
    }
}
