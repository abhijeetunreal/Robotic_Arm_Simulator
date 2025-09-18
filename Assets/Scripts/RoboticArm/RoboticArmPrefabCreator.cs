using UnityEngine;

public class RoboticArmPrefabCreator : MonoBehaviour
{
    [Header("Prefab Creation")]
    [Space(10)]
    [Tooltip("Click this to create a robotic arm prefab")]
    public bool createPrefab = false;
    
    [Space(10)]
    [Tooltip("Click this to instantiate the prefab in the scene")]
    public bool instantiatePrefab = false;
    
    [Header("Prefab Settings")]
    public string prefabName = "RoboticArmPrefab";
    public Vector3 prefabPosition = Vector3.zero;
    
    [Header("Arm Configuration")]
    public float armLength = 3f;
    public int numberOfJoints = 3;
    public float jointSize = 0.1f;
    
    private GameObject prefabInstance;
    
    void Update()
    {
        if (createPrefab)
        {
            createPrefab = false;
            CreateRoboticArmPrefab();
        }
        
        if (instantiatePrefab)
        {
            instantiatePrefab = false;
            InstantiatePrefab();
        }
    }
    
    public void CreateRoboticArmPrefab()
    {
        Debug.Log("Creating robotic arm prefab...");
        
        // Create the arm structure
        GameObject armRoot = CreateArmStructure();
        
        // Add all necessary components
        AddComponentsToArm(armRoot);
        
        // Save as prefab (this would typically be done in the editor)
        Debug.Log($"Robotic arm prefab '{prefabName}' created!");
        Debug.Log("To save as prefab: Drag the created object to the Project window");
        
        // Store reference for instantiation
        prefabInstance = armRoot;
    }
    
    GameObject CreateArmStructure()
    {
        // Create root object
        GameObject armRoot = new GameObject("RoboticArm");
        armRoot.transform.position = prefabPosition;
        
        // Create base joint
        GameObject baseJoint = new GameObject("Base Joint");
        baseJoint.transform.position = armRoot.transform.position;
        baseJoint.transform.parent = armRoot.transform;
        
        // Create arm structure
        Transform currentParent = baseJoint.transform;
        Vector3 currentPosition = baseJoint.transform.position;
        
        for (int i = 0; i < numberOfJoints; i++)
        {
            // Create joint
            GameObject joint = new GameObject($"Joint_{i + 1}");
            joint.transform.position = currentPosition + Vector3.forward * (armLength / numberOfJoints);
            joint.transform.parent = currentParent;
            
            // Add visual representation
            GameObject jointVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jointVisual.name = $"Joint_{i + 1}_Visual";
            jointVisual.transform.parent = joint.transform;
            jointVisual.transform.localPosition = Vector3.zero;
            jointVisual.transform.localScale = Vector3.one * jointSize;
            jointVisual.GetComponent<Renderer>().material.color = Color.blue;
            
            // Remove collider from visual
            DestroyImmediate(jointVisual.GetComponent<Collider>());
            
            currentParent = joint.transform;
            currentPosition = joint.transform.position;
        }
        
        // Create end effector (cube)
        GameObject endEffector = GameObject.CreatePrimitive(PrimitiveType.Cube);
        endEffector.name = "End Effector";
        endEffector.transform.position = currentPosition + Vector3.forward * 0.5f;
        endEffector.transform.localScale = Vector3.one * 0.3f;
        endEffector.transform.parent = currentParent;
        endEffector.GetComponent<Renderer>().material.color = Color.red;
        
        return armRoot;
    }
    
    void AddComponentsToArm(GameObject armRoot)
    {
        // Get base joint and end effector
        Transform baseJoint = armRoot.transform.Find("Base Joint");
        Transform endEffector = armRoot.transform.Find("Joint_1");
        
        // Find the last joint (end effector parent)
        Transform currentJoint = baseJoint;
        while (currentJoint.childCount > 0)
        {
            currentJoint = currentJoint.GetChild(0);
        }
        endEffector = currentJoint;
        
        // Add RoboticArmSetup component
        RoboticArmSetup armSetup = armRoot.AddComponent<RoboticArmSetup>();
        armSetup.baseJoint = baseJoint;
        armSetup.endEffector = endEffector;
        armSetup.autoSetup = true;
        
        // Add input handler
        RoboticArmInputHandler inputHandler = armRoot.AddComponent<RoboticArmInputHandler>();
        
        // Add air mouse
        GameObject airMouse = new GameObject("Air Mouse");
        airMouse.transform.parent = armRoot.transform;
        AirMouseInput airMouseInput = airMouse.AddComponent<AirMouseInput>();
        airMouseInput.sensitivity = 2f;
        airMouseInput.rollSensitivity = 1f;
        airMouseInput.deadzone = 0.1f;
        airMouseInput.smoothingFactor = 0.1f;
        
        Debug.Log("Components added to robotic arm");
    }
    
    public void InstantiatePrefab()
    {
        if (prefabInstance == null)
        {
            Debug.LogWarning("No prefab instance available. Create prefab first.");
            return;
        }
        
        // Instantiate the prefab
        GameObject instance = Instantiate(prefabInstance, prefabPosition, Quaternion.identity);
        instance.name = "RoboticArm_Instance";
        
        Debug.Log("Robotic arm prefab instantiated in scene");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 220, 300, 150));
        GUILayout.Label("Robotic Arm Prefab Creator");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Prefab"))
        {
            CreateRoboticArmPrefab();
        }
        
        if (GUILayout.Button("Instantiate Prefab"))
        {
            InstantiatePrefab();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Or use the checkboxes in the inspector:");
        GUILayout.Label("✓ Create Prefab");
        GUILayout.Label("✓ Instantiate Prefab");
        
        GUILayout.EndArea();
    }
}
