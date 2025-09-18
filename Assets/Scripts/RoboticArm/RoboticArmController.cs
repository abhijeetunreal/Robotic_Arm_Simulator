using UnityEngine;

public class RoboticArmController : MonoBehaviour
{
    [Header("Arm Configuration")]
    public Transform baseJoint; // Stationary base joint
    public Transform[] armJoints; // Array of arm joints (shoulder, elbow, wrist, etc.)
    public Transform endEffector; // The cube at the end of the arm
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 90f;
    public float rollSpeed = 180f;
    
    [Header("Movement Limits")]
    public float maxReach = 3f; // Maximum reach distance from base
    public float minHeight = 0.5f; // Minimum height above ground
    public float maxHeight = 4f; // Maximum height above ground
    
    [Header("Input Settings")]
    public AirMouseInput airMouseInput; // Reference to air mouse input
    public bool useKeyboardInput = true; // Fallback to keyboard for testing
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private InverseKinematics ikSystem;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 basePosition;
    
    void Start()
    {
        InitializeArm();
    }
    
    void InitializeArm()
    {
        // Get or add IK component
        ikSystem = GetComponent<InverseKinematics>();
        if (ikSystem == null)
        {
            ikSystem = gameObject.AddComponent<InverseKinematics>();
        }
        
        // Set up IK system
        ikSystem.joints = armJoints;
        ikSystem.target = endEffector;
        
        // Initialize positions
        basePosition = baseJoint.position;
        targetPosition = endEffector.position;
        targetRotation = endEffector.rotation;
        
        // Find air mouse input if not assigned
        if (airMouseInput == null)
        {
            airMouseInput = FindObjectOfType<AirMouseInput>();
        }
        
        Debug.Log("Robotic Arm Controller initialized");
    }
    
    void Update()
    {
        HandleInput();
        UpdateArmMovement();
    }
    
    void HandleInput()
    {
        Vector2 input = Vector2.zero;
        float rollInput = 0f;
        
        // Get input from air mouse or keyboard
        if (airMouseInput != null)
        {
            input = airMouseInput.GetInput();
            rollInput = airMouseInput.GetRollInput();
        }
        else if (useKeyboardInput)
        {
            // Keyboard fallback for testing
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
            rollInput = Input.GetAxis("Mouse ScrollWheel") * 10f;
        }
        
        // Apply input to target position
        Vector3 movement = new Vector3(input.x, input.y, 0) * moveSpeed * Time.deltaTime;
        targetPosition += movement;
        
        // Apply roll input to target rotation
        Vector3 eulerRotation = targetRotation.eulerAngles;
        eulerRotation.z += rollInput * rollSpeed * Time.deltaTime;
        targetRotation = Quaternion.Euler(eulerRotation);
        
        // Clamp target position within limits
        ClampTargetPosition();
    }
    
    void ClampTargetPosition()
    {
        // Clamp height
        targetPosition.y = Mathf.Clamp(targetPosition.y, basePosition.y + minHeight, basePosition.y + maxHeight);
        
        // Clamp horizontal distance from base
        Vector3 horizontalOffset = targetPosition - basePosition;
        horizontalOffset.y = 0; // Only consider horizontal distance
        float horizontalDistance = horizontalOffset.magnitude;
        
        if (horizontalDistance > maxReach)
        {
            horizontalOffset = horizontalOffset.normalized * maxReach;
            targetPosition = basePosition + horizontalOffset + Vector3.up * (targetPosition.y - basePosition.y);
        }
    }
    
    void UpdateArmMovement()
    {
        // Update end effector position and rotation
        endEffector.position = Vector3.Lerp(endEffector.position, targetPosition, moveSpeed * Time.deltaTime);
        endEffector.rotation = Quaternion.Lerp(endEffector.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Update IK target
        ikSystem.target = endEffector;
    }
    
    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        ClampTargetPosition();
    }
    
    public void SetTargetRotation(Quaternion rotation)
    {
        targetRotation = rotation;
    }
    
    public void ResetToDefaultPosition()
    {
        targetPosition = basePosition + Vector3.up * 2f + Vector3.forward * 1f;
        targetRotation = Quaternion.identity;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Draw movement limits
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(basePosition, maxReach);
        
        // Draw height limits
        Gizmos.color = Color.cyan;
        Vector3 minPos = basePosition + Vector3.up * minHeight;
        Vector3 maxPos = basePosition + Vector3.up * maxHeight;
        Gizmos.DrawLine(minPos, maxPos);
        
        // Draw target position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.1f);
        
        // Draw end effector
        if (endEffector != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(endEffector.position, Vector3.one * 0.2f);
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Robotic Arm Controller");
        GUILayout.Label($"Target Position: {targetPosition}");
        GUILayout.Label($"End Effector Position: {endEffector.position}");
        GUILayout.Label($"Distance from Base: {Vector3.Distance(basePosition, endEffector.position):F2}");
        
        if (GUILayout.Button("Reset to Default"))
        {
            ResetToDefaultPosition();
        }
        
        GUILayout.EndArea();
    }
}
