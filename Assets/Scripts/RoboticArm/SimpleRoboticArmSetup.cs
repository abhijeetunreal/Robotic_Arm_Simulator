using UnityEngine;

public class SimpleRoboticArmSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [Space(10)]
    [Tooltip("Click this button to create a complete robotic arm scene")]
    public bool createCompleteScene = false;
    
    [Space(10)]
    [Tooltip("Click this button to create just the robotic arm")]
    public bool createRoboticArm = false;
    
    [Header("Arm Settings")]
    public float armLength = 3f;
    public int numberOfJoints = 3;
    public float jointSize = 0.1f;
    
    [Header("Scene Settings")]
    public bool addGround = true;
    public bool addLighting = true;
    public bool addCamera = true;
    
    void Update()
    {
        // Check for button presses
        if (createCompleteScene)
        {
            createCompleteScene = false;
            CreateCompleteScene();
        }
        
        if (createRoboticArm)
        {
            createRoboticArm = false;
            CreateRoboticArm();
        }
    }
    
    public void CreateCompleteScene()
    {
        Debug.Log("Creating complete robotic arm scene...");
        
        // Create ground
        if (addGround)
        {
            CreateGround();
        }
        
        // Create lighting
        if (addLighting)
        {
            CreateLighting();
        }
        
        // Create camera
        if (addCamera)
        {
            CreateCamera();
        }
        
        // Create robotic arm
        CreateRoboticArm();
        
        // Create air mouse
        CreateAirMouse();
        
        Debug.Log("Complete scene created successfully!");
    }
    
    public void CreateRoboticArm()
    {
        Debug.Log("Creating robotic arm...");
        
        // Create base joint
        GameObject baseJoint = new GameObject("Base Joint");
        baseJoint.transform.position = Vector3.zero;
        
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
        
        // Add components
        AddRoboticArmComponents(baseJoint, endEffector);
        
        Debug.Log("Robotic arm created successfully!");
    }
    
    void AddRoboticArmComponents(GameObject baseJoint, GameObject endEffector)
    {
        // Add RoboticArmSetup component
        RoboticArmSetup armSetup = baseJoint.AddComponent<RoboticArmSetup>();
        armSetup.baseJoint = baseJoint.transform;
        armSetup.endEffector = endEffector.transform;
        armSetup.autoSetup = true;
        
        // Setup the arm
        armSetup.SetupArm();
        
        // Add input handler
        RoboticArmInputHandler inputHandler = baseJoint.AddComponent<RoboticArmInputHandler>();
        
        Debug.Log("Robotic arm components added!");
    }
    
    void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 0.1f, 10f);
        
        // Create material
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = Color.gray;
        ground.GetComponent<Renderer>().material = groundMat;
        
        Debug.Log("Ground created");
    }
    
    void CreateLighting()
    {
        // Set ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Color.gray;
        RenderSettings.ambientIntensity = 0.3f;
        
        // Create directional light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        Debug.Log("Lighting created");
    }
    
    void CreateCamera()
    {
        // Find or create main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }
        
        // Position camera
        mainCamera.transform.position = new Vector3(0, 5, -10);
        mainCamera.transform.rotation = Quaternion.Euler(15, 0, 0);
        
        // Add camera controller
        CameraController cameraController = mainCamera.GetComponent<CameraController>();
        if (cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<CameraController>();
        }
        
        Debug.Log("Camera created");
    }
    
    void CreateAirMouse()
    {
        // Create air mouse object
        GameObject airMouseObj = new GameObject("Air Mouse");
        AirMouseInput airMouse = airMouseObj.AddComponent<AirMouseInput>();
        
        // Configure for robotic arm
        airMouse.sensitivity = 2f;
        airMouse.rollSensitivity = 1f;
        airMouse.deadzone = 0.1f;
        airMouse.smoothingFactor = 0.1f;
        
        Debug.Log("Air Mouse created");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Simple Robotic Arm Setup");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Complete Scene"))
        {
            CreateCompleteScene();
        }
        
        if (GUILayout.Button("Create Robotic Arm Only"))
        {
            CreateRoboticArm();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Or use the checkboxes in the inspector:");
        GUILayout.Label("✓ Create Complete Scene");
        GUILayout.Label("✓ Create Robotic Arm");
        
        GUILayout.EndArea();
    }
}
