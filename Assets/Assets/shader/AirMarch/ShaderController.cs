using UnityEngine;
using UnityEngine.UI;

public class ShaderController : MonoBehaviour
{
    [Header("Input Source")]
    [Tooltip("Drag the GameObject that has the AirMouseInput script here.")]
    public AirMouseInput airMouseInput;

    [Header("Control Sensitivity")]
    public float rotationSpeed = 20f;

    // --- Private Variables ---
    private Material _materialInstance;
    private Vector3 _currentRotation; // x=Pitch, y=Yaw, z=Roll

    void Awake()
    {
        // Try to find the source material from either a Renderer or a RawImage.
        Material sourceMaterial = null;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            sourceMaterial = rend.material;
        }
        else
        {
            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                sourceMaterial = rawImage.material;
            }
        }

        if (sourceMaterial == null)
        {
            Debug.LogError("ShaderController Error: Could not find a Renderer or RawImage with a Material on this object!", this);
            this.enabled = false;
            return;
        }
        
        // Create an instance of the material so we don't change the base asset.
        _materialInstance = new Material(sourceMaterial);

        // Re-assign the new material instance back to the component.
        if (rend != null)
        {
            rend.material = _materialInstance;
        }
        else
        {
            GetComponent<RawImage>().material = _materialInstance;
        }
    }

    void Update()
    {
        if (airMouseInput == null || _materialInstance == null) return;
        
        if (airMouseInput.Status == ConnectionStatus.Connected)
        {
            Vector2 moveInput = airMouseInput.GetInput();
            _currentRotation.x -= moveInput.y * rotationSpeed * Time.deltaTime; // Pitch
            _currentRotation.y += moveInput.x * rotationSpeed * Time.deltaTime; // Yaw
        }

        _materialInstance.SetVector("_AirMouseRotation", new Vector4(_currentRotation.x, _currentRotation.y, 0, 0));
    }
}