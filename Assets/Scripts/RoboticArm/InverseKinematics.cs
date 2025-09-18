using UnityEngine;

public class InverseKinematics : MonoBehaviour
{
    [Header("IK Configuration")]
    public Transform[] joints; // Array of joint transforms (shoulder, elbow, wrist, etc.)
    public Transform target; // Target position for end effector
    public Transform pole; // Pole vector for elbow direction
    
    [Header("IK Settings")]
    public float tolerance = 0.01f; // How close we need to get to target
    public int maxIterations = 20; // Maximum iterations to prevent infinite loops
    public float damping = 0.5f; // Damping factor for stability
    
    [Header("Joint Constraints")]
    public float[] minAngles; // Minimum rotation angles for each joint
    public float[] maxAngles; // Maximum rotation angles for each joint
    
    [Header("Debug")]
    public bool showDebugLines = true;
    public Color debugLineColor = Color.red;
    
    private float[] jointLengths; // Length of each bone segment
    private Vector3[] jointPositions; // Current positions of joints
    
    void Start()
    {
        InitializeIK();
    }
    
    void Update()
    {
        if (target != null)
        {
            SolveIK();
        }
    }
    
    void InitializeIK()
    {
        if (joints == null || joints.Length < 2)
        {
            Debug.LogError("InverseKinematics: Need at least 2 joints!");
            return;
        }
        
        jointLengths = new float[joints.Length - 1];
        jointPositions = new Vector3[joints.Length];
        
        // Calculate bone lengths
        for (int i = 0; i < joints.Length - 1; i++)
        {
            if (joints[i] != null && joints[i + 1] != null)
            {
                jointLengths[i] = Vector3.Distance(joints[i].position, joints[i + 1].position);
            }
        }
        
        // Initialize constraint arrays if not set
        if (minAngles == null || minAngles.Length != joints.Length)
        {
            minAngles = new float[joints.Length];
            for (int i = 0; i < minAngles.Length; i++)
            {
                minAngles[i] = -180f;
            }
        }
        
        if (maxAngles == null || maxAngles.Length != joints.Length)
        {
            maxAngles = new float[joints.Length];
            for (int i = 0; i < maxAngles.Length; i++)
            {
                maxAngles[i] = 180f;
            }
        }
    }
    
    void SolveIK()
    {
        // Update joint positions
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] != null)
            {
                jointPositions[i] = joints[i].position;
            }
        }
        
        // Check if target is reachable
        float totalLength = 0f;
        for (int i = 0; i < jointLengths.Length; i++)
        {
            totalLength += jointLengths[i];
        }
        
        float distanceToTarget = Vector3.Distance(jointPositions[0], target.position);
        
        if (distanceToTarget > totalLength)
        {
            // Target is too far, extend arm towards target
            ExtendTowardsTarget();
            return;
        }
        
        // Perform CCD (Cyclic Coordinate Descent) IK
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Forward pass - work from end effector towards base
            for (int i = joints.Length - 2; i >= 0; i--)
            {
                if (joints[i] == null) continue;
                
                Vector3 toEnd = jointPositions[joints.Length - 1] - jointPositions[i];
                Vector3 toTarget = target.position - jointPositions[i];
                
                // Calculate rotation needed
                Quaternion rotation = Quaternion.FromToRotation(toEnd, toTarget);
                
                // Apply rotation with damping
                Quaternion newRotation = Quaternion.Lerp(Quaternion.identity, rotation, damping) * joints[i].rotation;
                
                // Apply constraints
                newRotation = ApplyConstraints(i, newRotation);
                
                joints[i].rotation = newRotation;
                
                // Update joint positions
                UpdateJointPositions();
                
                // Check if we're close enough
                if (Vector3.Distance(jointPositions[joints.Length - 1], target.position) < tolerance)
                {
                    return;
                }
            }
        }
    }
    
    void ExtendTowardsTarget()
    {
        Vector3 direction = (target.position - jointPositions[0]).normalized;
        
        for (int i = 0; i < joints.Length - 1; i++)
        {
            if (joints[i] == null) continue;
            
            Vector3 newPosition = jointPositions[i] + direction * jointLengths[i];
            joints[i + 1].position = newPosition;
            jointPositions[i + 1] = newPosition;
        }
    }
    
    void UpdateJointPositions()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] != null)
            {
                jointPositions[i] = joints[i].position;
            }
        }
    }
    
    Quaternion ApplyConstraints(int jointIndex, Quaternion rotation)
    {
        if (jointIndex >= minAngles.Length || jointIndex >= maxAngles.Length)
            return rotation;
        
        Vector3 eulerAngles = rotation.eulerAngles;
        
        // Normalize angles to -180 to 180 range
        for (int i = 0; i < 3; i++)
        {
            if (eulerAngles[i] > 180f)
                eulerAngles[i] -= 360f;
        }
        
        // Apply constraints (simplified - you might want more sophisticated constraint handling)
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, minAngles[jointIndex], maxAngles[jointIndex]);
        
        return Quaternion.Euler(eulerAngles);
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugLines || joints == null) return;
        
        Gizmos.color = debugLineColor;
        
        // Draw bone segments
        for (int i = 0; i < joints.Length - 1; i++)
        {
            if (joints[i] != null && joints[i + 1] != null)
            {
                Gizmos.DrawLine(joints[i].position, joints[i + 1].position);
            }
        }
        
        // Draw target
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, 0.1f);
        }
        
        // Draw pole vector
        if (pole != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pole.position, 0.05f);
        }
    }
}
