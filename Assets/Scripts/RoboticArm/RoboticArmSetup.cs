using UnityEngine;

[System.Serializable]
public class ArmJoint
{
    public string name;
    public Transform joint;
    public float length;
    public Vector3 minAngles;
    public Vector3 maxAngles;
}

public class RoboticArmSetup : MonoBehaviour
{
    [Header("Arm Structure")]
    public Transform baseJoint;
    public ArmJoint[] armJoints;
    public Transform endEffector;
    
    [Header("Auto Setup")]
    public bool autoSetup = true;
    public float defaultJointLength = 1f;
    public Vector3 defaultMinAngles = new Vector3(-90f, -90f, -90f);
    public Vector3 defaultMaxAngles = new Vector3(90f, 90f, 90f);
    
    [Header("Prefab Creation")]
    public bool createPrefab = false;
    public string prefabName = "RoboticArmPrefab";
    
    void Start()
    {
        if (autoSetup)
        {
            SetupArm();
        }
    }
    
    [ContextMenu("Setup Arm")]
    public void SetupArm()
    {
        if (baseJoint == null)
        {
            Debug.LogError("Base joint not assigned!");
            return;
        }
        
        // Find all child transforms that could be joints
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        // Filter out the base joint and end effector
        System.Collections.Generic.List<Transform> jointCandidates = new System.Collections.Generic.List<Transform>();
        
        foreach (Transform child in allChildren)
        {
            if (child != baseJoint && child != endEffector && child != transform)
            {
                jointCandidates.Add(child);
            }
        }
        
        // Create arm joints array
        armJoints = new ArmJoint[jointCandidates.Count];
        
        for (int i = 0; i < jointCandidates.Count; i++)
        {
            armJoints[i] = new ArmJoint
            {
                name = jointCandidates[i].name,
                joint = jointCandidates[i],
                length = defaultJointLength,
                minAngles = defaultMinAngles,
                maxAngles = defaultMaxAngles
            };
        }
        
        // Calculate actual joint lengths
        CalculateJointLengths();
        
        // Setup components
        SetupComponents();
        
        Debug.Log($"Arm setup complete with {armJoints.Length} joints");
    }
    
    void CalculateJointLengths()
    {
        for (int i = 0; i < armJoints.Length - 1; i++)
        {
            if (armJoints[i].joint != null && armJoints[i + 1].joint != null)
            {
                armJoints[i].length = Vector3.Distance(
                    armJoints[i].joint.position, 
                    armJoints[i + 1].joint.position
                );
            }
        }
        
        // Calculate length for last joint to end effector
        if (armJoints.Length > 0 && endEffector != null)
        {
            int lastIndex = armJoints.Length - 1;
            if (armJoints[lastIndex].joint != null)
            {
                armJoints[lastIndex].length = Vector3.Distance(
                    armJoints[lastIndex].joint.position, 
                    endEffector.position
                );
            }
        }
    }
    
    void SetupComponents()
    {
        // Add RoboticArmController if not present
        RoboticArmController armController = GetComponent<RoboticArmController>();
        if (armController == null)
        {
            armController = gameObject.AddComponent<RoboticArmController>();
        }
        
        // Setup arm controller
        armController.baseJoint = baseJoint;
        armController.endEffector = endEffector;
        
        // Create arm joints array for controller
        Transform[] jointTransforms = new Transform[armJoints.Length];
        for (int i = 0; i < armJoints.Length; i++)
        {
            jointTransforms[i] = armJoints[i].joint;
        }
        armController.armJoints = jointTransforms;
        
        // Add InverseKinematics component
        InverseKinematics ik = GetComponent<InverseKinematics>();
        if (ik == null)
        {
            ik = gameObject.AddComponent<InverseKinematics>();
        }
        
        // Setup IK
        ik.joints = jointTransforms;
        ik.target = endEffector;
        
        // Add EndEffectorController to end effector
        if (endEffector != null)
        {
            EndEffectorController endController = endEffector.GetComponent<EndEffectorController>();
            if (endController == null)
            {
                endController = endEffector.gameObject.AddComponent<EndEffectorController>();
            }
        }
        
        // Add input handler
        RoboticArmInputHandler inputHandler = GetComponent<RoboticArmInputHandler>();
        if (inputHandler == null)
        {
            inputHandler = gameObject.AddComponent<RoboticArmInputHandler>();
        }
        
        // Setup input handler references
        inputHandler.armController = armController;
        inputHandler.endEffectorController = endEffector.GetComponent<EndEffectorController>();
        
        Debug.Log("All components setup complete");
    }
    
    [ContextMenu("Create Prefab")]
    public void CreatePrefab()
    {
        if (createPrefab)
        {
            // This would typically be done in the Unity Editor
            Debug.Log($"Prefab creation requested: {prefabName}");
            Debug.Log("To create prefab: Drag this object to the Project window");
        }
    }
    
    [ContextMenu("Reset Arm Position")]
    public void ResetArmPosition()
    {
        if (armJoints == null) return;
        
        // Reset all joints to default rotation
        foreach (ArmJoint armJoint in armJoints)
        {
            if (armJoint.joint != null)
            {
                armJoint.joint.rotation = Quaternion.identity;
            }
        }
        
        // Reset end effector
        if (endEffector != null)
        {
            endEffector.rotation = Quaternion.identity;
        }
        
        Debug.Log("Arm position reset");
    }
    
    void OnDrawGizmos()
    {
        if (armJoints == null) return;
        
        // Draw joint connections
        Gizmos.color = Color.blue;
        for (int i = 0; i < armJoints.Length - 1; i++)
        {
            if (armJoints[i].joint != null && armJoints[i + 1].joint != null)
            {
                Gizmos.DrawLine(armJoints[i].joint.position, armJoints[i + 1].joint.position);
            }
        }
        
        // Draw connection to end effector
        if (armJoints.Length > 0 && endEffector != null)
        {
            int lastIndex = armJoints.Length - 1;
            if (armJoints[lastIndex].joint != null)
            {
                Gizmos.DrawLine(armJoints[lastIndex].joint.position, endEffector.position);
            }
        }
        
        // Draw joint positions
        Gizmos.color = Color.red;
        foreach (ArmJoint armJoint in armJoints)
        {
            if (armJoint.joint != null)
            {
                Gizmos.DrawWireSphere(armJoint.joint.position, 0.05f);
            }
        }
        
        // Draw end effector
        if (endEffector != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(endEffector.position, Vector3.one * 0.1f);
        }
    }
}
