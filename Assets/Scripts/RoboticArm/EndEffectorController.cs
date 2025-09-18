using UnityEngine;

public class EndEffectorController : MonoBehaviour
{
    [Header("End Effector Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 90f;
    public float rollSpeed = 180f;
    
    [Header("Visual Feedback")]
    public Renderer cubeRenderer;
    public Color defaultColor = Color.blue;
    public Color activeColor = Color.red;
    public Color targetColor = Color.green;
    
    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip moveSound;
    public AudioClip rotateSound;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private bool isRotating = false;
    
    void Start()
    {
        InitializeEndEffector();
    }
    
    void InitializeEndEffector()
    {
        // Get renderer if not assigned
        if (cubeRenderer == null)
        {
            cubeRenderer = GetComponent<Renderer>();
        }
        
        // Get audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Initialize target position and rotation
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        
        // Set default color
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = defaultColor;
        }
        
        Debug.Log("End Effector Controller initialized");
    }
    
    void Update()
    {
        UpdateMovement();
        UpdateVisualFeedback();
    }
    
    void UpdateMovement()
    {
        // Smooth movement to target position
        Vector3 positionDifference = targetPosition - transform.position;
        if (positionDifference.magnitude > 0.01f)
        {
            isMoving = true;
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Play movement sound
            if (audioSource != null && moveSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = moveSound;
                audioSource.Play();
            }
        }
        else
        {
            isMoving = false;
        }
        
        // Smooth rotation to target rotation
        Quaternion rotationDifference = targetRotation * Quaternion.Inverse(transform.rotation);
        if (rotationDifference.eulerAngles.magnitude > 0.1f)
        {
            isRotating = true;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Play rotation sound
            if (audioSource != null && rotateSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = rotateSound;
                audioSource.Play();
            }
        }
        else
        {
            isRotating = false;
        }
    }
    
    void UpdateVisualFeedback()
    {
        if (cubeRenderer == null) return;
        
        // Change color based on state
        Color currentColor = defaultColor;
        
        if (isMoving || isRotating)
        {
            currentColor = activeColor;
        }
        
        cubeRenderer.material.color = Color.Lerp(cubeRenderer.material.color, currentColor, Time.deltaTime * 5f);
    }
    
    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }
    
    public void SetTargetRotation(Quaternion rotation)
    {
        targetRotation = rotation;
    }
    
    public void MoveToPosition(Vector3 position)
    {
        SetTargetPosition(position);
    }
    
    public void RotateTo(Quaternion rotation)
    {
        SetTargetRotation(rotation);
    }
    
    public void Roll(float rollAmount)
    {
        Vector3 eulerRotation = targetRotation.eulerAngles;
        eulerRotation.z += rollAmount;
        targetRotation = Quaternion.Euler(eulerRotation);
    }
    
    public void ResetToDefault()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }
    
    public bool IsMoving()
    {
        return isMoving || isRotating;
    }
    
    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }
    
    public Quaternion GetTargetRotation()
    {
        return targetRotation;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Draw target position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.1f);
        
        // Draw movement direction
        if (isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
        
        // Draw rotation axis
        if (isRotating)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 220, 300, 150));
        GUILayout.Label("End Effector Status");
        GUILayout.Label($"Position: {transform.position}");
        GUILayout.Label($"Target Position: {targetPosition}");
        GUILayout.Label($"Is Moving: {isMoving}");
        GUILayout.Label($"Is Rotating: {isRotating}");
        
        if (GUILayout.Button("Reset Position"))
        {
            ResetToDefault();
        }
        
        GUILayout.EndArea();
    }
}
