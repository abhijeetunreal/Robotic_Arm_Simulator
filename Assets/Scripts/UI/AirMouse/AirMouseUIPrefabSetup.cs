using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.AirMouse
{
    /// <summary>
    /// Helper script to create and setup the AirMouse UI prefab programmatically.
    /// This script can be used to generate the complete UI structure for AirMouse settings.
    /// </summary>
    public class AirMouseUIPrefabSetup : MonoBehaviour
    {
        [Header("Prefab Creation")]
        [SerializeField] public bool createUIPrefab = false;
        [SerializeField] public Transform parentCanvas;
        
        private void Start()
        {
            if (createUIPrefab)
            {
                CreateAirMouseUIPrefab();
            }
        }
        
        [ContextMenu("Create AirMouse UI Prefab")]
        public void CreateAirMouseUIPrefab()
        {
            if (parentCanvas == null)
            {
                Debug.LogError("Parent Canvas is not assigned!");
                return;
            }
            
            // Create main UI manager object
            GameObject uiManagerObject = new GameObject("AirMouseUIManager");
            uiManagerObject.transform.SetParent(parentCanvas);
            
            // Add AirMouseUIManager component
            AirMouseUIManager uiManager = uiManagerObject.AddComponent<AirMouseUIManager>();
            
            // Create toggle button
            GameObject toggleButton = CreateButton("TogglePanelButton", "Show AirMouse Settings", uiManagerObject.transform);
            uiManager.togglePanelButton = toggleButton.GetComponent<Button>();
            
            // Create apply button
            GameObject applyButton = CreateButton("ApplyButton", "Apply Settings", uiManagerObject.transform);
            uiManager.applySettingsButton = applyButton.GetComponent<Button>();
            
            // Create reset button
            GameObject resetButton = CreateButton("ResetButton", "Reset to Defaults", uiManagerObject.transform);
            uiManager.resetToDefaultsButton = resetButton.GetComponent<Button>();
            
            // Create UI panel
            GameObject panelObject = CreatePanel("AirMouseUIPanel", uiManagerObject.transform);
            AirMouseUIPanel uiPanel = panelObject.AddComponent<AirMouseUIPanel>();
            uiManager.uiPanel = uiPanel;
            
            // Setup panel content
            SetupPanelContent(panelObject, uiPanel);
            
            // Create visual indicator
            CreateVisualIndicator(panelObject, uiPanel);
            
            // Try to find AirMouseInput in scene
            AirMouseInput airMouseInput = FindFirstObjectByType<AirMouseInput>();
            if (airMouseInput != null)
            {
                uiManager.airMouseInput = airMouseInput;
            }
            else
            {
                Debug.LogWarning("No AirMouseInput found in scene. Please assign it manually in the inspector.");
            }
            
            Debug.Log("AirMouse UI Prefab created successfully!");
        }
        
        private GameObject CreateButton(string name, string text, Transform parent)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent);
            
            // Add Image component for button background
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add Button component
            Button button = buttonObject.AddComponent<Button>();
            
            // Create text child
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform);
            
            TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            // Setup RectTransform for text
            RectTransform textRect = textComponent.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Setup RectTransform for button
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150, 30);
            buttonRect.anchoredPosition = Vector2.zero;
            
            return buttonObject;
        }
        
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent);
            
            // Add Image component for panel background
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Setup RectTransform
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(400, 600);
            panelRect.anchoredPosition = new Vector2(0, 0);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            
            return panelObject;
        }
        
        private void SetupPanelContent(GameObject panelObject, AirMouseUIPanel uiPanel)
        {
            // Create scrollable content area
            GameObject scrollView = CreateScrollView("ScrollView", panelObject.transform);
            
            // Create content area
            GameObject contentArea = new GameObject("Content");
            contentArea.transform.SetParent(scrollView.transform);
            
            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Add VerticalLayoutGroup
            VerticalLayoutGroup layoutGroup = contentArea.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create sections
            CreateConnectionSection(contentArea.transform, uiPanel);
            CreateAxisOrientationSection(contentArea.transform, uiPanel);
            CreateFlightControlSection(contentArea.transform, uiPanel);
            CreateStatusSection(contentArea.transform, uiPanel);
        }
        
        private GameObject CreateScrollView(string name, Transform parent)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent);
            
            // Add ScrollRect
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            
            // Add Image for background
            Image scrollImage = scrollView.AddComponent<Image>();
            scrollImage.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            
            // Setup RectTransform
            RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;
            
            return scrollView;
        }
        
        private void CreateConnectionSection(Transform parent, AirMouseUIPanel uiPanel)
        {
            GameObject section = CreateSection("Connection Settings", parent);
            
            // Port Name
            CreateInputField("Port Name", "COM4", section.transform, out TMP_InputField portInput);
            uiPanel.portNameInput = portInput;
            
            // Baud Rate
            CreateInputField("Baud Rate", "115200", section.transform, out TMP_InputField baudInput);
            uiPanel.baudRateInput = baudInput;
        }
        
        private void CreateAxisOrientationSection(Transform parent, AirMouseUIPanel uiPanel)
        {
            GameObject section = CreateSection("Axis Orientation", parent);
            
            // Horizontal Axis
            CreateDropdown("Horizontal Axis", new string[] { "Pitch", "Roll", "Yaw" }, section.transform, out TMP_Dropdown horizontalDropdown);
            uiPanel.horizontalAxisDropdown = horizontalDropdown;
            
            CreateToggle("Invert Horizontal", section.transform, out Toggle invertHorizontal);
            uiPanel.invertHorizontalToggle = invertHorizontal;
            
            // Vertical Axis
            CreateDropdown("Vertical Axis", new string[] { "Pitch", "Roll", "Yaw" }, section.transform, out TMP_Dropdown verticalDropdown);
            uiPanel.verticalAxisDropdown = verticalDropdown;
            
            CreateToggle("Invert Vertical", section.transform, out Toggle invertVertical);
            uiPanel.invertVerticalToggle = invertVertical;
            
            // Roll Axis
            CreateDropdown("Roll Axis", new string[] { "Pitch", "Roll", "Yaw" }, section.transform, out TMP_Dropdown rollDropdown);
            uiPanel.rollAxisDropdown = rollDropdown;
            
            CreateToggle("Invert Roll", section.transform, out Toggle invertRoll);
            uiPanel.invertRollToggle = invertRoll;
        }
        
        private void CreateFlightControlSection(Transform parent, AirMouseUIPanel uiPanel)
        {
            GameObject section = CreateSection("Flight Controls", parent);
            
            // Sensitivity
            CreateSlider("Sensitivity", 0.1f, 5.0f, 1.0f, section.transform, out Slider sensitivitySlider, out TextMeshProUGUI sensitivityText);
            uiPanel.sensitivitySlider = sensitivitySlider;
            uiPanel.sensitivityValueText = sensitivityText;
            
            // Roll Sensitivity
            CreateSlider("Roll Sensitivity", 0.1f, 5.0f, 1.0f, section.transform, out Slider rollSensitivitySlider, out TextMeshProUGUI rollSensitivityText);
            uiPanel.rollSensitivitySlider = rollSensitivitySlider;
            uiPanel.rollSensitivityValueText = rollSensitivityText;
            
            // Deadzone
            CreateSlider("Deadzone", 0f, 5f, 0.5f, section.transform, out Slider deadzoneSlider, out TextMeshProUGUI deadzoneText);
            uiPanel.deadzoneSlider = deadzoneSlider;
            uiPanel.deadzoneValueText = deadzoneText;
            
            // Smoothing Factor
            CreateSlider("Smoothing Factor", 0.01f, 1.0f, 0.15f, section.transform, out Slider smoothingSlider, out TextMeshProUGUI smoothingText);
            uiPanel.smoothingFactorSlider = smoothingSlider;
            uiPanel.smoothingFactorValueText = smoothingText;
        }
        
        private void CreateStatusSection(Transform parent, AirMouseUIPanel uiPanel)
        {
            GameObject section = CreateSection("Status", parent);
            
            // Connection Status
            CreateLabel("Connection Status: Not Connected", section.transform, out TextMeshProUGUI connectionStatus);
            uiPanel.connectionStatusText = connectionStatus;
            
            // Current Input
            CreateLabel("Input: X=0.00, Y=0.00, Roll=0.00", section.transform, out TextMeshProUGUI currentInput);
            uiPanel.currentInputText = currentInput;
        }
        
        private GameObject CreateSection(string title, Transform parent)
        {
            GameObject section = new GameObject(title);
            section.transform.SetParent(parent);
            
            // Add VerticalLayoutGroup
            VerticalLayoutGroup layoutGroup = section.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Add title
            CreateLabel(title, section.transform, out TextMeshProUGUI titleText);
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.yellow;
            
            return section;
        }
        
        private void CreateInputField(string label, string placeholder, Transform parent, out TMP_InputField inputField)
        {
            GameObject container = new GameObject(label + "Container");
            container.transform.SetParent(parent);
            
            // Add HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create label
            CreateLabel(label + ":", container.transform, out TextMeshProUGUI labelText);
            labelText.rectTransform.sizeDelta = new Vector2(100, 20);
            
            // Create input field
            GameObject inputObject = new GameObject(label + "Input");
            inputObject.transform.SetParent(container.transform);
            
            Image inputImage = inputObject.AddComponent<Image>();
            inputImage.color = Color.white;
            
            inputField = inputObject.AddComponent<TMP_InputField>();
            inputField.text = placeholder;
            
            // Create text component
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(inputObject.transform);
            
            TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = placeholder;
            textComponent.fontSize = 12;
            textComponent.color = Color.black;
            
            RectTransform textRect = textComponent.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
            
            inputField.textComponent = textComponent;
            
            // Setup RectTransform
            RectTransform inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(0, 25);
        }
        
        private void CreateDropdown(string label, string[] options, Transform parent, out TMP_Dropdown dropdown)
        {
            GameObject container = new GameObject(label + "Container");
            container.transform.SetParent(parent);
            
            // Add HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create label
            CreateLabel(label + ":", container.transform, out TextMeshProUGUI labelText);
            labelText.rectTransform.sizeDelta = new Vector2(100, 20);
            
            // Create dropdown
            GameObject dropdownObject = new GameObject(label + "Dropdown");
            dropdownObject.transform.SetParent(container.transform);
            
            Image dropdownImage = dropdownObject.AddComponent<Image>();
            dropdownImage.color = Color.white;
            
            dropdown = dropdownObject.AddComponent<TMP_Dropdown>();
            dropdown.options.Clear();
            foreach (string option in options)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(option));
            }
            dropdown.value = 0;
            
            // Create label component
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(dropdownObject.transform);
            
            TextMeshProUGUI labelComponent = labelObject.AddComponent<TextMeshProUGUI>();
            labelComponent.text = options[0];
            labelComponent.fontSize = 12;
            labelComponent.color = Color.black;
            
            RectTransform labelRect = labelComponent.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(5, 0);
            labelRect.offsetMax = new Vector2(-5, 0);
            
            dropdown.captionText = labelComponent;
            
            // Setup RectTransform
            RectTransform dropdownRect = dropdownObject.GetComponent<RectTransform>();
            dropdownRect.sizeDelta = new Vector2(0, 25);
        }
        
        private void CreateToggle(string label, Transform parent, out Toggle toggle)
        {
            GameObject container = new GameObject(label + "Container");
            container.transform.SetParent(parent);
            
            // Add HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create toggle
            GameObject toggleObject = new GameObject(label + "Toggle");
            toggleObject.transform.SetParent(container.transform);
            
            Image toggleImage = toggleObject.AddComponent<Image>();
            toggleImage.color = Color.white;
            
            toggle = toggleObject.AddComponent<Toggle>();
            toggle.isOn = false;
            
            // Create checkmark
            GameObject checkmarkObject = new GameObject("Checkmark");
            checkmarkObject.transform.SetParent(toggleObject.transform);
            
            Image checkmarkImage = checkmarkObject.AddComponent<Image>();
            checkmarkImage.color = Color.black;
            
            RectTransform checkmarkRect = checkmarkImage.rectTransform;
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;
            
            toggle.graphic = checkmarkImage;
            
            // Create label
            CreateLabel(label, container.transform, out TextMeshProUGUI labelText);
            labelText.rectTransform.sizeDelta = new Vector2(100, 20);
            
            // Setup RectTransform
            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(20, 20);
        }
        
        private void CreateSlider(string label, float minValue, float maxValue, float defaultValue, Transform parent, out Slider slider, out TextMeshProUGUI valueText)
        {
            GameObject container = new GameObject(label + "Container");
            container.transform.SetParent(parent);
            
            // Add HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create label
            CreateLabel(label + ":", container.transform, out TextMeshProUGUI labelText);
            labelText.rectTransform.sizeDelta = new Vector2(100, 20);
            
            // Create slider
            GameObject sliderObject = new GameObject(label + "Slider");
            sliderObject.transform.SetParent(container.transform);
            
            Image sliderImage = sliderObject.AddComponent<Image>();
            sliderImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            slider = sliderObject.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            
            // Create fill area
            GameObject fillAreaObject = new GameObject("Fill Area");
            fillAreaObject.transform.SetParent(sliderObject.transform);
            
            RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            // Create fill
            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(fillAreaObject.transform);
            
            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.color = Color.blue;
            
            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            slider.fillRect = fillRect;
            
            // Create handle
            GameObject handleAreaObject = new GameObject("Handle Slide Area");
            handleAreaObject.transform.SetParent(sliderObject.transform);
            
            RectTransform handleAreaRect = handleAreaObject.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;
            
            GameObject handleObject = new GameObject("Handle");
            handleObject.transform.SetParent(handleAreaObject.transform);
            
            Image handleImage = handleObject.AddComponent<Image>();
            handleImage.color = Color.white;
            
            RectTransform handleRect = handleImage.rectTransform;
            handleRect.sizeDelta = new Vector2(20, 20);
            handleRect.anchorMin = new Vector2(0, 0.5f);
            handleRect.anchorMax = new Vector2(0, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            
            slider.handleRect = handleRect;
            
            // Create value text
            CreateLabel(defaultValue.ToString("F2"), container.transform, out valueText);
            valueText.rectTransform.sizeDelta = new Vector2(50, 20);
            
            // Setup RectTransform
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(0, 20);
        }
        
        private void CreateLabel(string text, Transform parent, out TextMeshProUGUI label)
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent);
            
            label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 12;
            label.color = Color.white;
            
            RectTransform labelRect = label.rectTransform;
            labelRect.sizeDelta = new Vector2(0, 20);
        }
        
        private void CreateVisualIndicator(GameObject panelObject, AirMouseUIPanel uiPanel)
        {
            // Create visual indicator section
            GameObject visualSection = CreateSection("Input Visualization", panelObject.transform);
            
            // Create visual indicator container
            GameObject visualContainer = new GameObject("VisualIndicatorContainer");
            visualContainer.transform.SetParent(visualSection.transform);
            
            // Add VerticalLayoutGroup for stacked layout
            VerticalLayoutGroup layoutGroup = visualContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create main visualization area
            GameObject mainVisualArea = CreateMainVisualizationArea("MainVisualization", visualContainer.transform);
            
            // Create visual indicator component
            GameObject visualIndicatorObject = new GameObject("AirMouseVisualIndicator");
            visualIndicatorObject.transform.SetParent(visualContainer.transform);
            
            AirMouseVisualIndicator visualIndicator = visualIndicatorObject.AddComponent<AirMouseVisualIndicator>();
            uiPanel.visualIndicator = visualIndicator;
            
            // Link visual elements to the indicator
            LinkVisualElements(visualIndicator, mainVisualArea);
        }
        
        private GameObject CreateMainVisualizationArea(string name, Transform parent)
        {
            GameObject mainArea = new GameObject(name);
            mainArea.transform.SetParent(parent);
            
            // Add VerticalLayoutGroup
            VerticalLayoutGroup layoutGroup = mainArea.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            
            // Create background for main visualization
            GameObject mainBackground = new GameObject("MainBackground");
            mainBackground.transform.SetParent(mainArea.transform);
            
            Image backgroundImage = mainBackground.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            RectTransform backgroundRect = mainBackground.GetComponent<RectTransform>();
            backgroundRect.sizeDelta = new Vector2(200, 200);
            
            // Create main indicator
            GameObject mainIndicator = new GameObject("MainIndicator");
            mainIndicator.transform.SetParent(mainBackground.transform);
            
            Image indicatorImage = mainIndicator.AddComponent<Image>();
            indicatorImage.color = Color.white;
            
            RectTransform indicatorRect = mainIndicator.GetComponent<RectTransform>();
            indicatorRect.sizeDelta = new Vector2(20, 20);
            indicatorRect.anchorMin = new Vector2(0.5f, 0.5f);
            indicatorRect.anchorMax = new Vector2(0.5f, 0.5f);
            indicatorRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Create input value text
            CreateLabel("X: 0.00\nY: 0.00\nRoll: 0.00", mainArea.transform, out TextMeshProUGUI inputValueText);
            
            return mainArea;
        }
        
        
        private void LinkVisualElements(AirMouseVisualIndicator visualIndicator, GameObject mainVisualArea)
        {
            // Find and link main elements
            Transform mainBackground = mainVisualArea.transform.Find("MainBackground");
            Transform mainIndicator = mainBackground?.Find("MainIndicator");
            Transform inputValueText = mainVisualArea.transform.GetChild(1); // Assuming text is second child
            
            if (mainIndicator != null)
            {
                visualIndicator.mainIndicator = mainIndicator.GetComponent<RectTransform>();
            }
            
            if (mainIndicator != null)
            {
                visualIndicator.indicatorImage = mainIndicator.GetComponent<Image>();
            }
            
            if (inputValueText != null)
            {
                visualIndicator.inputValueText = inputValueText.GetComponent<TextMeshProUGUI>();
            }
        }
    }
}
