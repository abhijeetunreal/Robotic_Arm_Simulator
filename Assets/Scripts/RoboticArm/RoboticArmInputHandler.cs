using UnityEngine;

public class RoboticArmInputHandler : MonoBehaviour
{
    [Header("Input Configuration")]
    public AirMouseInput airMouseInput;
    public RoboticArmController armController;
    public EndEffectorController endEffectorController;
    
    [Header("Input Mapping")]
    [Tooltip("How much the air mouse input affects movement")]
    public float inputSensitivity = 1f;
    [Tooltip("How much the air mouse roll affects end effector rotation")]
    public float rollSensitivity = 1f;
    [Tooltip("Dead zone for input to prevent jitter")]
    public float deadZone = 0.1f;
    
    [Header("Movement Modes")]
    public bool enablePositionControl = true;
    public bool enableRotationControl = true;
    public bool enableRollControl = true;
    
    [Header("Smoothing")]
    [Tooltip("Smoothing factor for input")]
    public float smoothingFactor = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Vector2 smoothedInput;
    private float smoothedRoll;
    private Vector2 lastInput;
    private float lastRoll;
    
    void Start()
    {
        InitializeInputHandler();
    }
    
    void InitializeInputHandler()
    {
        // Find components if not assigned
        if (airMouseInput == null)
        {
            airMouseInput = FindObjectOfType<AirMouseInput>();
        }
        
        if (armController == null)
        {
            armController = FindObjectOfType<RoboticArmController>();
        }
        
        if (endEffectorController == null)
        {
            endEffectorController = FindObjectOfType<EndEffectorController>();
        }
        
        // Initialize smoothed values
        smoothedInput = Vector2.zero;
        smoothedRoll = 0f;
        lastInput = Vector2.zero;
        lastRoll = 0f;
        
        Debug.Log("Robotic Arm Input Handler initialized");
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        if (airMouseInput == null) return;
        
        // Get raw input from air mouse
        Vector2 rawInput = airMouseInput.GetInput();
        float rawRoll = airMouseInput.GetRollInput();
        
        // Apply dead zone
        if (rawInput.magnitude < deadZone)
        {
            rawInput = Vector2.zero;
        }
        
        if (Mathf.Abs(rawRoll) < deadZone)
        {
            rawRoll = 0f;
        }
        
        // Apply smoothing
        smoothedInput = Vector2.Lerp(smoothedInput, rawInput, smoothingFactor);
        smoothedRoll = Mathf.Lerp(smoothedRoll, rawRoll, smoothingFactor);
        
        // Apply input to arm controller
        ApplyInputToArm();
        
        // Update last values for next frame
        lastInput = smoothedInput;
        lastRoll = smoothedRoll;
    }
    
    void ApplyInputToArm()
    {
        if (armController == null) return;
        
        // Get current target position
        Vector3 currentTarget = armController.endEffector.position;
        
        // Calculate movement based on input
        Vector3 movement = Vector3.zero;
        
        if (enablePositionControl)
        {
            // Map air mouse input to 3D movement
            // X input controls left/right movement
            // Y input controls up/down movement
            movement.x = smoothedInput.x * inputSensitivity * Time.deltaTime;
            movement.y = smoothedInput.y * inputSensitivity * Time.deltaTime;
            
            // Apply movement to target position
            Vector3 newTarget = currentTarget + movement;
            armController.SetTargetPosition(newTarget);
        }
        
        if (enableRotationControl && endEffectorController != null)
        {
            // Apply roll input to end effector
            if (enableRollControl)
            {
                float rollAmount = smoothedRoll * rollSensitivity * Time.deltaTime;
                endEffectorController.Roll(rollAmount);
            }
        }
    }
    
    public void SetInputSensitivity(float sensitivity)
    {
        inputSensitivity = sensitivity;
    }
    
    public void SetRollSensitivity(float sensitivity)
    {
        rollSensitivity = sensitivity;
    }
    
    public void SetDeadZone(float deadZone)
    {
        this.deadZone = deadZone;
    }
    
    public void EnablePositionControl(bool enable)
    {
        enablePositionControl = enable;
    }
    
    public void EnableRotationControl(bool enable)
    {
        enableRotationControl = enable;
    }
    
    public void EnableRollControl(bool enable)
    {
        enableRollControl = enable;
    }
    
    public Vector2 GetCurrentInput()
    {
        return smoothedInput;
    }
    
    public float GetCurrentRoll()
    {
        return smoothedRoll;
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(320, 10, 300, 200));
        GUILayout.Label("Input Handler Status");
        GUILayout.Label($"Raw Input: {airMouseInput?.GetInput()}");
        GUILayout.Label($"Smoothed Input: {smoothedInput}");
        GUILayout.Label($"Raw Roll: {airMouseInput?.GetRollInput()}");
        GUILayout.Label($"Smoothed Roll: {smoothedRoll:F2}");
        GUILayout.Label($"Input Magnitude: {smoothedInput.magnitude:F2}");
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("Position Control: " + (enablePositionControl ? "ON" : "OFF"));
        GUILayout.Label("Rotation Control: " + (enableRotationControl ? "ON" : "OFF"));
        GUILayout.Label("Roll Control: " + (enableRollControl ? "ON" : "OFF"));
        
        GUILayout.EndArea();
    }
}
