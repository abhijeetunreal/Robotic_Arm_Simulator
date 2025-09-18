using UnityEngine;

namespace UI.AirMouse
{
    /// <summary>
    /// Test script to demonstrate AirMouse visual feedback.
    /// This script can be used to test the visual indicator without actual hardware.
    /// </summary>
    public class AirMouseVisualTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableTestMode = false;
        [SerializeField] private float testInputSpeed = 2f;
        [SerializeField] private float testRollSpeed = 1f;
        [SerializeField] private bool useSineWave = true;
        
        [Header("Test Input Simulation")]
        [SerializeField] private Vector2 testInputRange = new Vector2(-2f, 2f);
        [SerializeField] private float testRollRange = 1f;
        
        private AirMouseVisualIndicator visualIndicator;
        private AirMouseUIManager uiManager;
        private float testTime = 0f;
        
        private void Start()
        {
            // Find the visual indicator
            visualIndicator = FindFirstObjectByType<AirMouseVisualIndicator>();
            uiManager = FindFirstObjectByType<AirMouseUIManager>();
            
            if (visualIndicator == null)
            {
                Debug.LogWarning("No AirMouseVisualIndicator found. Please create the UI first.");
            }
        }
        
        private void Update()
        {
            if (!enableTestMode || visualIndicator == null) return;
            
            testTime += Time.deltaTime;
            
            // Generate test input
            Vector2 testInput = GenerateTestInput();
            float testRoll = GenerateTestRoll();
            
            // Simulate the visual feedback
            SimulateVisualFeedback(testInput, testRoll);
        }
        
        private Vector2 GenerateTestInput()
        {
            if (useSineWave)
            {
                float x = Mathf.Sin(testTime * testInputSpeed) * testInputRange.x;
                float y = Mathf.Cos(testTime * testInputSpeed * 0.7f) * testInputRange.y;
                return new Vector2(x, y);
            }
            else
            {
                // Random input for more chaotic testing
                float x = Random.Range(testInputRange.x, testInputRange.y);
                float y = Random.Range(testInputRange.x, testInputRange.y);
                return new Vector2(x, y);
            }
        }
        
        private float GenerateTestRoll()
        {
            if (useSineWave)
            {
                return Mathf.Sin(testTime * testRollSpeed * 1.3f) * testRollRange;
            }
            else
            {
                return Random.Range(-testRollRange, testRollRange);
            }
        }
        
        private void SimulateVisualFeedback(Vector2 input, float roll)
        {
            // Use the new TestVisualIndicator method which works with the velocity-based system
            visualIndicator.TestVisualIndicator(input, roll);
        }
        
        
        // Public methods for external control
        public void ToggleTestMode()
        {
            enableTestMode = !enableTestMode;
            Debug.Log($"AirMouse Visual Test Mode: {(enableTestMode ? "ON" : "OFF")}");
        }
        
        public void SetTestSpeed(float speed)
        {
            testInputSpeed = speed;
        }
        
        public void SetTestRollSpeed(float speed)
        {
            testRollSpeed = speed;
        }
        
        public void ToggleSineWave()
        {
            useSineWave = !useSineWave;
        }
        
        // Context menu methods for easy testing
        [ContextMenu("Toggle Test Mode")]
        private void ContextToggleTestMode()
        {
            ToggleTestMode();
        }
        
        [ContextMenu("Toggle Sine Wave")]
        private void ContextToggleSineWave()
        {
            ToggleSineWave();
        }
    }
}
