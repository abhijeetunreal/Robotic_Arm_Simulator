using UnityEngine;

public class RoboticArmSceneSetup : MonoBehaviour
{
    [Header("Scene Setup")]
    public bool createGround = true;
    public bool createLighting = true;
    public bool createCamera = true;
    public bool createAirMouse = true;
    
    [Header("Ground Settings")]
    public Material groundMaterial;
    public Vector3 groundSize = new Vector3(10f, 0.1f, 10f);
    
    [Header("Lighting Settings")]
    public Color ambientColor = Color.gray;
    public float ambientIntensity = 0.3f;
    
    [Header("Camera Settings")]
    public Vector3 cameraPosition = new Vector3(0, 5, -10);
    public Vector3 cameraRotation = new Vector3(15, 0, 0);
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        if (createGround)
        {
            CreateGround();
        }
        
        if (createLighting)
        {
            SetupLighting();
        }
        
        if (createCamera)
        {
            SetupCamera();
        }
        
        if (createAirMouse)
        {
            CreateAirMouse();
        }
        
        Debug.Log("Scene setup complete!");
    }
    
    void CreateGround()
    {
        // Create ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = groundSize;
        
        // Apply material if provided
        if (groundMaterial != null)
        {
            ground.GetComponent<Renderer>().material = groundMaterial;
        }
        else
        {
            // Create a simple material
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.gray;
            ground.GetComponent<Renderer>().material = mat;
        }
        
        Debug.Log("Ground created");
    }
    
    void SetupLighting()
    {
        // Set ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        
        // Create directional light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        Debug.Log("Lighting setup complete");
    }
    
    void SetupCamera()
    {
        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // Create new camera
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }
        
        // Position and rotate camera
        mainCamera.transform.position = cameraPosition;
        mainCamera.transform.rotation = Quaternion.Euler(cameraRotation);
        
        // Add camera controller for better viewing
        CameraController cameraController = mainCamera.GetComponent<CameraController>();
        if (cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<CameraController>();
        }
        
        Debug.Log("Camera setup complete");
    }
    
    void CreateAirMouse()
    {
        // Create air mouse object
        GameObject airMouseObj = new GameObject("Air Mouse");
        AirMouseInput airMouse = airMouseObj.AddComponent<AirMouseInput>();
        
        // Configure air mouse settings for robotic arm
        airMouse.sensitivity = 2f;
        airMouse.rollSensitivity = 1f;
        airMouse.deadzone = 0.1f;
        airMouse.smoothingFactor = 0.1f;
        
        Debug.Log("Air Mouse created");
    }
    
    [ContextMenu("Create Robotic Arm")]
    public void CreateRoboticArm()
    {
        // Create base joint
        GameObject baseJoint = new GameObject("Base Joint");
        baseJoint.transform.position = Vector3.zero;
        
        // Create arm joints
        GameObject shoulder = new GameObject("Shoulder");
        shoulder.transform.position = baseJoint.transform.position + Vector3.up * 0.5f;
        shoulder.transform.parent = baseJoint.transform;
        
        GameObject elbow = new GameObject("Elbow");
        elbow.transform.position = shoulder.transform.position + Vector3.forward * 1f;
        elbow.transform.parent = shoulder.transform;
        
        GameObject wrist = new GameObject("Wrist");
        wrist.transform.position = elbow.transform.position + Vector3.forward * 1f;
        wrist.transform.parent = elbow.transform;
        
        // Create end effector (cube)
        GameObject endEffector = GameObject.CreatePrimitive(PrimitiveType.Cube);
        endEffector.name = "End Effector";
        endEffector.transform.position = wrist.transform.position + Vector3.forward * 0.5f;
        endEffector.transform.localScale = Vector3.one * 0.3f;
        endEffector.transform.parent = wrist.transform;
        
        // Add robotic arm setup component
        RoboticArmSetup armSetup = baseJoint.AddComponent<RoboticArmSetup>();
        armSetup.baseJoint = baseJoint.transform;
        armSetup.endEffector = endEffector.transform;
        armSetup.autoSetup = true;
        
        // Setup the arm
        armSetup.SetupArm();
        
        Debug.Log("Robotic arm created and setup complete!");
    }
    
    [ContextMenu("Create Complete Scene")]
    public void CreateCompleteScene()
    {
        SetupScene();
        CreateRoboticArm();
        
        Debug.Log("Complete robotic arm scene created!");
    }
}

// Simple camera controller for better viewing
public class CameraController : MonoBehaviour
{
    [Header("Camera Controls")]
    public float mouseSensitivity = 2f;
    public float scrollSensitivity = 2f;
    public float moveSpeed = 5f;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleScroll();
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
    
    void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * scrollSensitivity;
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label("Camera Controls:");
        GUILayout.Label("Mouse: Look around");
        GUILayout.Label("WASD: Move");
        GUILayout.Label("Scroll: Zoom");
        GUILayout.EndArea();
    }
}
