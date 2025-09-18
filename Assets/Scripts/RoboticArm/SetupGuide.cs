using UnityEngine;

public class SetupGuide : MonoBehaviour
{
    [Header("Step-by-Step Setup Guide")]
    [Space(10)]
    [TextArea(10, 20)]
    public string setupInstructions = @"
ROBOTIC ARM SETUP GUIDE
=======================

METHOD 1: Simple Setup (Recommended)
1. Add 'SimpleRoboticArmSetup' component to any GameObject
2. Check 'Create Complete Scene' in the inspector
3. The system will create everything automatically!

METHOD 2: Manual Setup
1. Create a new scene
2. Create empty GameObject
3. Add 'SimpleRoboticArmSetup' component
4. Check 'Create Robotic Arm' in the inspector
5. Add 'AirMouseInput' component to any GameObject

METHOD 3: Prefab Creation
1. Add 'RoboticArmPrefabCreator' component
2. Check 'Create Prefab' in the inspector
3. Drag the created object to Project window to save as prefab
4. Check 'Instantiate Prefab' to add to scene

TROUBLESHOOTING:
- If context menus don't appear, use the checkboxes in inspector
- If arm doesn't move, check AirMouseInput connection
- If IK doesn't work, verify all joints are assigned
- Check Console for error messages

CONTROLS:
- Air Mouse X/Y: Move end effector
- Air Mouse Roll: Rotate end effector
- Keyboard WASD: Move (fallback)
- Mouse Scroll: Roll (fallback)
";

    [Header("Quick Setup Buttons")]
    [Space(10)]
    [Tooltip("Click this to create a complete scene with robotic arm")]
    public bool createCompleteScene = false;
    
    [Space(10)]
    [Tooltip("Click this to create just the robotic arm")]
    public bool createRoboticArm = false;
    
    [Space(10)]
    [Tooltip("Click this to create a prefab")]
    public bool createPrefab = false;
    
    void Update()
    {
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
        
        if (createPrefab)
        {
            createPrefab = false;
            CreatePrefab();
        }
    }
    
    void CreateCompleteScene()
    {
        // Create ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 0.1f, 10f);
        ground.GetComponent<Renderer>().material.color = Color.gray;
        
        // Create lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Color.gray;
        RenderSettings.ambientIntensity = 0.3f;
        
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        // Create camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }
        mainCamera.transform.position = new Vector3(0, 5, -10);
        mainCamera.transform.rotation = Quaternion.Euler(15, 0, 0);
        
        // Create robotic arm
        CreateRoboticArm();
        
        // Create air mouse
        GameObject airMouse = new GameObject("Air Mouse");
        AirMouseInput airMouseInput = airMouse.AddComponent<AirMouseInput>();
        airMouseInput.sensitivity = 2f;
        airMouseInput.rollSensitivity = 1f;
        airMouseInput.deadzone = 0.1f;
        airMouseInput.smoothingFactor = 0.1f;
        
        Debug.Log("Complete scene created successfully!");
    }
    
    void CreateRoboticArm()
    {
        // Create base joint
        GameObject baseJoint = new GameObject("Base Joint");
        baseJoint.transform.position = Vector3.zero;
        
        // Create shoulder joint
        GameObject shoulder = new GameObject("Shoulder");
        shoulder.transform.position = baseJoint.transform.position + Vector3.up * 0.5f;
        shoulder.transform.parent = baseJoint.transform;
        
        // Create elbow joint
        GameObject elbow = new GameObject("Elbow");
        elbow.transform.position = shoulder.transform.position + Vector3.forward * 1f;
        elbow.transform.parent = shoulder.transform;
        
        // Create wrist joint
        GameObject wrist = new GameObject("Wrist");
        wrist.transform.position = elbow.transform.position + Vector3.forward * 1f;
        wrist.transform.parent = elbow.transform;
        
        // Create end effector (cube)
        GameObject endEffector = GameObject.CreatePrimitive(PrimitiveType.Cube);
        endEffector.name = "End Effector";
        endEffector.transform.position = wrist.transform.position + Vector3.forward * 0.5f;
        endEffector.transform.localScale = Vector3.one * 0.3f;
        endEffector.transform.parent = wrist.transform;
        endEffector.GetComponent<Renderer>().material.color = Color.red;
        
        // Add components
        RoboticArmSetup armSetup = baseJoint.AddComponent<RoboticArmSetup>();
        armSetup.baseJoint = baseJoint.transform;
        armSetup.endEffector = endEffector.transform;
        armSetup.autoSetup = true;
        
        // Setup the arm
        armSetup.SetupArm();
        
        Debug.Log("Robotic arm created successfully!");
    }
    
    void CreatePrefab()
    {
        // Create the arm structure
        GameObject armRoot = new GameObject("RoboticArmPrefab");
        armRoot.transform.position = Vector3.zero;
        
        // Create base joint
        GameObject baseJoint = new GameObject("Base Joint");
        baseJoint.transform.position = armRoot.transform.position;
        baseJoint.transform.parent = armRoot.transform;
        
        // Create arm structure
        Transform currentParent = baseJoint.transform;
        Vector3 currentPosition = baseJoint.transform.position;
        
        for (int i = 0; i < 3; i++)
        {
            GameObject joint = new GameObject($"Joint_{i + 1}");
            joint.transform.position = currentPosition + Vector3.forward * 1f;
            joint.transform.parent = currentParent;
            
            currentParent = joint.transform;
            currentPosition = joint.transform.position;
        }
        
        // Create end effector
        GameObject endEffector = GameObject.CreatePrimitive(PrimitiveType.Cube);
        endEffector.name = "End Effector";
        endEffector.transform.position = currentPosition + Vector3.forward * 0.5f;
        endEffector.transform.localScale = Vector3.one * 0.3f;
        endEffector.transform.parent = currentParent;
        endEffector.GetComponent<Renderer>().material.color = Color.red;
        
        // Add components
        RoboticArmSetup armSetup = armRoot.AddComponent<RoboticArmSetup>();
        armSetup.baseJoint = baseJoint.transform;
        armSetup.endEffector = endEffector.transform;
        armSetup.autoSetup = true;
        armSetup.SetupArm();
        
        Debug.Log("Robotic arm prefab created! Drag to Project window to save.");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("ROBOTIC ARM SETUP GUIDE", GUI.skin.box);
        GUILayout.Space(10);
        
        GUILayout.Label("Quick Setup Buttons:");
        GUILayout.Space(5);
        
        if (GUILayout.Button("Create Complete Scene"))
        {
            CreateCompleteScene();
        }
        
        if (GUILayout.Button("Create Robotic Arm Only"))
        {
            CreateRoboticArm();
        }
        
        if (GUILayout.Button("Create Prefab"))
        {
            CreatePrefab();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Or use the checkboxes in the inspector above!");
        
        GUILayout.EndArea();
    }
}
