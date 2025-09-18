using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.AirMouse
{
    /// <summary>
    /// Visual indicator that shows real-time AirMouse input for testing and tweaking settings.
    /// Displays input values, movement visualization, and connection status.
    /// </summary>
    public class AirMouseVisualIndicator : MonoBehaviour
    {
        [Header("Visual Elements")]
        [SerializeField] public RectTransform mainIndicator;
        [SerializeField] public Image indicatorImage;
        [SerializeField] public TextMeshProUGUI inputValueText;
        [SerializeField] public TextMeshProUGUI statusText;
        
        [Header("Visual Settings")]
        [SerializeField] public float maxIndicatorRange = 100f;
        [SerializeField] public float moveSpeed = 200f; // Speed of movement (like PlayerController moveSpeed)
        [SerializeField] public float rollSpeed = 180f; // Speed of rotation (like PlayerController rollSpeed)
        
        [Header("Input Display")]
        [SerializeField] private bool showInputValues = true;
        
        public AirMouseInput airMouseInput;
        private Vector3 currentPosition; // X, Y, Z (roll rotation)
        
        // Status tracking
        private bool isConnected = false;
        private float lastInputTime = 0f;
        private float inputTimeout = 2f; // Consider disconnected if no input for 2 seconds
        
        public void Initialize(AirMouseInput airMouse)
        {
            airMouseInput = airMouse;
            SetupVisualElements();
            
            Debug.Log($"AirMouseVisualIndicator initialized with AirMouseInput: {(airMouseInput != null ? "SUCCESS" : "FAILED")}");
            Debug.Log($"Visual elements - MainIndicator: {(mainIndicator != null ? "OK" : "NULL")}, IndicatorImage: {(indicatorImage != null ? "OK" : "NULL")}");
            Debug.Log($"Text elements - InputText: {(inputValueText != null ? "OK" : "NULL")}");
        }
        
        
        private void SetupVisualElements()
        {
            // Setup main indicator
            if (mainIndicator != null)
            {
                mainIndicator.anchoredPosition = Vector2.zero;
                mainIndicator.rotation = Quaternion.identity;
            }
            
            // Setup initial text
            UpdateStatusText("Waiting for input...");
        }
        
        private void Update()
        {
            if (airMouseInput == null) 
            {
                Debug.LogWarning("AirMouseVisualIndicator: AirMouseInput is null in Update!");
                return;
            }
            
            UpdateInputValues();
            UpdateVisualIndicators();
            UpdateStatus();
        }
        
        private void UpdateInputValues()
        {
            if (airMouseInput == null)
            {
                Debug.LogWarning("AirMouseVisualIndicator: AirMouseInput is null!");
                UpdateStatusText("No AirMouseInput");
                return;
            }
            
            Vector2 input = airMouseInput.GetInput();
            float roll = airMouseInput.GetRollInput();
            
            // Debug: Always log the input values to see what we're getting
            Debug.Log($"AirMouseVisualIndicator Update - Input: X={input.x:F3}, Y={input.y:F3}, Roll={roll:F3}");
            
            // Always update text display to show current values
            if (showInputValues && inputValueText != null)
            {
                inputValueText.text = $"X: {input.x:F2}\nY: {input.y:F2}\nRoll: {roll:F2}";
                Debug.Log($"Updated text display: {inputValueText.text}");
            }
            else
            {
                Debug.LogWarning("InputValueText is null!");
            }
            
            // Update last input time for any non-zero input
            if (input.magnitude > 0.001f || Mathf.Abs(roll) > 0.001f)
            {
                lastInputTime = Time.time;
            }
            
            // Calculate movement exactly like PlayerController
            Vector3 moveDirection = new Vector3(input.x, input.y, 0);
            
            // Calculate new position exactly like PlayerController: transform.position + combinedMove * moveSpeed * Time.deltaTime
            Vector3 newPosition = currentPosition + moveDirection * moveSpeed * Time.deltaTime;
            
            // Clamp position exactly like PlayerController
            newPosition.x = Mathf.Clamp(newPosition.x, -maxIndicatorRange, maxIndicatorRange);
            newPosition.y = Mathf.Clamp(newPosition.y, -maxIndicatorRange, maxIndicatorRange);
            
            // Update position exactly like PlayerController: transform.position = newPosition
            currentPosition = newPosition;
            
            // Handle rotation exactly like PlayerController: transform.Rotate(0, 0, -totalRoll * rollSpeed * Time.deltaTime)
            float rollRotation = -roll * rollSpeed * Time.deltaTime;
            currentPosition.z += rollRotation;
            
            Debug.Log($"Current Position: X={currentPosition.x:F1}, Y={currentPosition.y:F1}, Z={currentPosition.z:F1}");
            
        }
        
        private void UpdateVisualIndicators()
        {
            // Update main indicator position and rotation
            if (mainIndicator != null)
            {
                // Set position (X, Y movement)
                Vector2 newPosition = new Vector2(currentPosition.x, currentPosition.y);
                mainIndicator.anchoredPosition = newPosition;
                
                // Set rotation (Z rotation for roll)
                Quaternion newRotation = Quaternion.Euler(0, 0, currentPosition.z);
                mainIndicator.rotation = newRotation;
                
                Debug.Log($"Updated Visual Indicator - Position: {newPosition}, Rotation: {newRotation.eulerAngles.z:F1}°");
            }
            else
            {
                Debug.LogWarning("MainIndicator is null! Visual indicator cannot update.");
            }
        }
        
        
        private void UpdateStatus()
        {
            bool wasConnected = isConnected;
            
            // Check if AirMouseInput is available and has been initialized
            if (airMouseInput == null)
            {
                isConnected = false;
                UpdateStatusText("No AirMouseInput");
            }
            else
            {
                // Consider connected if we've received input recently OR if AirMouseInput is ready
                bool hasRecentInput = (Time.time - lastInputTime) < inputTimeout;
                bool isAirMouseReady = airMouseInput.GetInput().magnitude >= 0 || airMouseInput.GetRollInput() >= 0; // Always true if AirMouseInput is working
                
                isConnected = hasRecentInput || isAirMouseReady;
                
                if (isConnected)
                {
                    UpdateStatusText("Connected");
                }
                else
                {
                    UpdateStatusText("Waiting for input...");
                }
            }
            
            if (isConnected != wasConnected)
            {
                Debug.Log($"AirMouseVisualIndicator Status Changed: {(isConnected ? "Connected" : "Disconnected")}");
            }
        }
        
        
        private void UpdateStatusText(string status)
        {
            if (statusText != null)
            {
                statusText.text = $"Status: {status}";
            }
        }
        
        // Public methods for external control
        public void SetMaxRange(float range)
        {
            maxIndicatorRange = range;
        }
        
        
        // Force connection status (useful for debugging)
        public void ForceConnected()
        {
            isConnected = true;
            lastInputTime = Time.time;
            UpdateStatusText("Connected (Forced)");
        }
        
        // Reset connection status
        public void ResetConnection()
        {
            isConnected = false;
            lastInputTime = 0f;
            UpdateStatusText("Disconnected");
        }
        
        // Test visual indicator with manual input
        public void TestVisualIndicator(Vector2 testInput, float testRoll)
        {
            Debug.Log($"Testing Visual Indicator with Input: X={testInput.x:F2}, Y={testInput.y:F2}, Roll={testRoll:F2}");
            
            // Update text display
            if (showInputValues && inputValueText != null)
            {
                inputValueText.text = $"X: {testInput.x:F2}\nY: {testInput.y:F2}\nRoll: {testRoll:F2}";
            }
            
            // Apply test input exactly like PlayerController
            Vector3 testMoveDirection = new Vector3(testInput.x, testInput.y, 0);
            Vector3 testNewPosition = currentPosition + testMoveDirection * moveSpeed * Time.deltaTime;
            
            // Clamp position
            testNewPosition.x = Mathf.Clamp(testNewPosition.x, -maxIndicatorRange, maxIndicatorRange);
            testNewPosition.y = Mathf.Clamp(testNewPosition.y, -maxIndicatorRange, maxIndicatorRange);
            
            // Update position
            currentPosition = testNewPosition;
            
            // Apply test rotation
            float testRollRotation = -testRoll * rollSpeed * Time.deltaTime;
            currentPosition.z += testRollRotation;
            
            // Update visual indicator
            if (mainIndicator != null)
            {
                mainIndicator.anchoredPosition = new Vector2(currentPosition.x, currentPosition.y);
                mainIndicator.rotation = Quaternion.Euler(0, 0, currentPosition.z);
                Debug.Log($"Test Visual Updated - Position: {mainIndicator.anchoredPosition}, Rotation: {mainIndicator.rotation.eulerAngles.z:F1}°");
            }
        }
        
        
        public void ResetIndicators()
        {
            currentPosition = Vector3.zero;
            
            if (mainIndicator != null)
            {
                mainIndicator.anchoredPosition = Vector2.zero;
                mainIndicator.rotation = Quaternion.identity;
            }
        }
        
        // Manual refresh method for troubleshooting
        public void ForceRefresh()
        {
            if (airMouseInput == null)
            {
                Debug.LogWarning("AirMouseVisualIndicator: No AirMouseInput reference!");
                return;
            }
            
            Vector2 input = airMouseInput.GetInput();
            float roll = airMouseInput.GetRollInput();
            
            Debug.Log($"Force Refresh - Input: X={input.x:F2}, Y={input.y:F2}, Roll={roll:F2}");
            
            // Force update text display
            if (inputValueText != null)
            {
                inputValueText.text = $"X: {input.x:F2}\nY: {input.y:F2}\nRoll: {roll:F2}";
            }
            
            // Force update indicator position and rotation
            if (mainIndicator != null)
            {
                Vector2 position = new Vector2(input.x * maxIndicatorRange, input.y * maxIndicatorRange);
                float rotation = -roll * rollSpeed * Time.deltaTime;
                
                mainIndicator.anchoredPosition = position;
                mainIndicator.rotation = Quaternion.Euler(0, 0, rotation);
            }
        }
        
        // Getters for external access
        public Vector3 CurrentPosition => currentPosition;
        public bool IsConnected => isConnected;
        public float InputMagnitude => new Vector2(currentPosition.x, currentPosition.y).magnitude;
        public float RollMagnitude => Mathf.Abs(currentPosition.z);
    }
}
