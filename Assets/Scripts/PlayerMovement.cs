using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float slideSpeed = 15f;
    [SerializeField] private float airSpeed = 4f;
    [SerializeField] private float wallRunSpeed = 10f;
    
    [Header("Jump & Air Control")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float airControl = 0.3f;
    [SerializeField] private float airDrag = 0.95f;
    [SerializeField] private float maxAirSpeed = 20f;
    [SerializeField] private float gravity = -10f;
    
    [Header("Advanced Movement")]
    [SerializeField] private float slideTime = 1f;
    [SerializeField] private float wallRunTime = 2f;
    [SerializeField] private float wallRunUpwardForce = 2f;
    [SerializeField] private float wallRunSideForce = 5f;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private LayerMask wallLayerMask = 1;
    
    [Header("Physics")]
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    
    [Header("Input")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    // Components
    private CharacterController controller;
    private Camera playerCamera;
    private Transform cameraTransform;
    
    // Movement state
    private Vector3 velocity;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSliding;
    private bool isWallRunning;
    private bool isRunning;
    
    // Timers
    private float slideTimer;
    private float wallRunTimer;
    
    // Input values
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool crouchPressed;
    private bool runPressed;
    
    // Wall run data
    private Vector3 wallNormal;
    private bool wallLeft;
    private bool wallRight;
    
    // Camera
    private float xRotation = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        cameraTransform = playerCamera.transform;
        
        // Configure CharacterController for better physics
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.3f;
        controller.minMoveDistance = 0f;
        
        // Lock cursor for FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleInput();
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleGravity();
        HandleCrouch();
        HandleSlide();
        HandleWallRun();
        HandleCamera();
        ApplyMovement();
    }
    
    void HandleInput()
    {
        // Movement input (WASD)
        if (Keyboard.current != null)
        {
            moveInput.x = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
            moveInput.y = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
            
            // Jump
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            
            // Crouch
            crouchPressed = Keyboard.current.leftCtrlKey.wasPressedThisFrame;
            
            // Run
            runPressed = Keyboard.current.leftShiftKey.isPressed;
            isRunning = runPressed;
        }
        
        // Mouse look
        if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;
        }
    }
    
    void CheckGround()
    {
        // Use collision detection at the base of the player
        // Check multiple points around the player's base for more accurate detection
        
        bool groundHit = false;
        float closestDistance = float.MaxValue;
        
        // Increase raycast distance to ensure we hit the ground
        float raycastDistance = 3.0f;
        
        // Check center point
        RaycastHit centerHit;
        if (Physics.Raycast(transform.position, Vector3.down, out centerHit, raycastDistance))
        {
            groundHit = true;
            closestDistance = Vector3.Distance(transform.position, centerHit.point);
        }
        
        // Check front and back points
        Vector3 frontPoint = transform.position + transform.forward * 0.3f;
        Vector3 backPoint = transform.position - transform.forward * 0.3f;
        
        RaycastHit frontHit, backHit;
        if (Physics.Raycast(frontPoint, Vector3.down, out frontHit, raycastDistance))
        {
            groundHit = true;
            float distance = Vector3.Distance(transform.position, frontHit.point);
            if (distance < closestDistance) closestDistance = distance;
        }
        
        if (Physics.Raycast(backPoint, Vector3.down, out backHit, raycastDistance))
        {
            groundHit = true;
            float distance = Vector3.Distance(transform.position, backHit.point);
            if (distance < closestDistance) closestDistance = distance;
        }
        
        // Check left and right points
        Vector3 leftPoint = transform.position - transform.right * 0.3f;
        Vector3 rightPoint = transform.position + transform.right * 0.3f;
        
        RaycastHit leftHit, rightHit;
        if (Physics.Raycast(leftPoint, Vector3.down, out leftHit, raycastDistance))
        {
            groundHit = true;
            float distance = Vector3.Distance(transform.position, leftHit.point);
            if (distance < closestDistance) closestDistance = distance;
        }
        
        if (Physics.Raycast(rightPoint, Vector3.down, out rightHit, raycastDistance))
        {
            groundHit = true;
            float distance = Vector3.Distance(transform.position, rightHit.point);
            if (distance < closestDistance) closestDistance = distance;
        }
        
        // Determine if grounded based on closest ground distance
        if (groundHit)
        {
            // When on ground, distance should be around 1.0 (from center to ground)
            // When in air, distance should be larger
            isGrounded = closestDistance <= 1.5f; // Allow some tolerance
        }
        else
        {
            isGrounded = false;
        }
        
        // Additional check: if we're moving upward, we're definitely not grounded
        if (velocity.y > 0.1f)
        {
            isGrounded = false;
        }
        
        // Debug: Print ground state
        if (Debug.isDebugBuild)
        {
            Debug.Log($"Grounded: {isGrounded}, Velocity Y: {velocity.y:F2}, Distance to Ground: {(groundHit ? closestDistance.ToString("F2") : "No ground")}, Ground Hit: {groundHit}, Raycast Distance: {raycastDistance}");
        }
    }
    
    void HandleMovement()
    {
        // Calculate movement direction
        Vector3 forward = transform.forward * moveInput.y;
        Vector3 right = transform.right * moveInput.x;
        moveDirection = (forward + right).normalized;
        
        // Determine current speed
        float currentSpeed = GetCurrentSpeed();
        
        // Apply movement
        if (isGrounded)
        {
            // Ground movement with acceleration
            Vector3 targetVelocity = moveDirection * currentSpeed;
            velocity = Vector3.Lerp(velocity, targetVelocity, acceleration * Time.deltaTime);
            
            // Apply ground drag
            velocity.x *= (1f - groundDrag * Time.deltaTime);
            velocity.z *= (1f - groundDrag * Time.deltaTime);
        }
        else
        {
            // Air movement
            if (moveDirection.magnitude > 0.1f)
            {
                Vector3 airVelocity = moveDirection * airSpeed;
                velocity.x = Mathf.Lerp(velocity.x, airVelocity.x, airControl * Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, airVelocity.z, airControl * Time.deltaTime);
            }
            
            // Air drag
            velocity.x *= airDrag;
            velocity.z *= airDrag;
            
            // Limit air speed
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            if (horizontalVelocity.magnitude > maxAirSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxAirSpeed;
                velocity.x = horizontalVelocity.x;
                velocity.z = horizontalVelocity.z;
            }
        }
    }
    
    float GetCurrentSpeed()
    {
        if (isSliding) return slideSpeed;
        if (isWallRunning) return wallRunSpeed;
        if (isCrouching) return crouchSpeed;
        if (isRunning && !isCrouching) return runSpeed;
        return walkSpeed;
    }
    
    void HandleJump()
    {
        if (jumpPressed && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    
    void HandleGravity()
    {
        // Apply gravity only when not grounded
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            // When grounded, ensure we stay on the ground
            if (velocity.y < 0)
            {
                velocity.y = 0f; // Stop falling when grounded
            }
        }
        
        // Clamp fall speed to prevent excessive falling speed
        if (velocity.y < -50f)
        {
            velocity.y = -50f;
        }
    }
    
    void HandleCrouch()
    {
        if (crouchPressed && isGrounded && !isSliding)
        {
            if (!isCrouching)
            {
                // Start crouching
                isCrouching = true;
                controller.height = 1f;
                controller.center = new Vector3(0, -0.5f, 0);
                Debug.Log("Crouch activated");
            }
            else
            {
                // Force stand up when Ctrl is pressed again
                isCrouching = false;
                controller.height = 2f;
                controller.center = Vector3.zero;
                Debug.Log("Crouch deactivated by Ctrl press");
            }
        }
        
        // Auto-stand up when not grounded or when sliding
        if (isCrouching && (!isGrounded || isSliding))
        {
            isCrouching = false;
            controller.height = 2f;
            controller.center = Vector3.zero;
            Debug.Log("Auto-stand up due to not grounded or sliding");
        }
        
        // Debug info
        if (Debug.isDebugBuild)
        {
            Debug.Log($"Crouch Debug - crouchPressed: {crouchPressed}, isCrouching: {isCrouching}, isGrounded: {isGrounded}, isSliding: {isSliding}");
        }
    }
    
    void HandleSlide()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }
        else if (crouchPressed && isGrounded && !isCrouching && velocity.magnitude > 5f)
        {
            StartSlide();
        }
    }
    
    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideTime;
        controller.height = 1f;
        controller.center = new Vector3(0, -0.5f, 0);
        
        // Boost forward velocity
        Vector3 slideDirection = transform.forward;
        velocity = slideDirection * slideSpeed;
    }
    
    void EndSlide()
    {
        isSliding = false;
        if (!isCrouching)
        {
            controller.height = 2f;
            controller.center = Vector3.zero;
        }
    }
    
    void HandleWallRun()
    {
        CheckWalls();
        
        if (isWallRunning)
        {
            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0 || (!wallLeft && !wallRight))
            {
                EndWallRun();
            }
            else
            {
                // Apply wall run forces
                if (wallLeft)
                {
                    velocity += transform.right * wallRunSideForce * Time.deltaTime;
                    velocity.y += wallRunUpwardForce * Time.deltaTime;
                }
                else if (wallRight)
                {
                    velocity -= transform.right * wallRunSideForce * Time.deltaTime;
                    velocity.y += wallRunUpwardForce * Time.deltaTime;
                }
            }
        }
        else if (CanWallRun())
        {
            StartWallRun();
        }
    }
    
    void CheckWalls()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, wallCheckDistance, wallLayerMask);
        wallRight = Physics.Raycast(transform.position, transform.right, wallCheckDistance, wallLayerMask);
    }
    
    bool CanWallRun()
    {
        return !isGrounded && (wallLeft || wallRight) && velocity.magnitude > 5f && !isCrouching;
    }
    
    void StartWallRun()
    {
        isWallRunning = true;
        wallRunTimer = wallRunTime;
        
        // Determine wall normal
        if (wallLeft)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallLayerMask))
            {
                wallNormal = hit.normal;
            }
        }
        else if (wallRight)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallLayerMask))
            {
                wallNormal = hit.normal;
            }
        }
    }
    
    void EndWallRun()
    {
        isWallRunning = false;
        wallRunTimer = 0;
    }
    
    void HandleCamera()
    {
        // Horizontal rotation (Y-axis)
        transform.Rotate(Vector3.up * lookInput.x);
        
        // Vertical rotation (X-axis) with clamping
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void ApplyMovement()
    {
        // Apply velocity to character controller
        controller.Move(velocity * Time.deltaTime);
    }
    
    // Public methods for external access
    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
    public bool IsSliding() => isSliding;
    public bool IsWallRunning() => isWallRunning;
    public bool IsRunning() => isRunning;
    public Vector3 GetVelocity() => velocity;
    public float GetSpeed() => new Vector3(velocity.x, 0, velocity.z).magnitude;
    
    // Debug method to test ground detection
    [ContextMenu("Test Ground Detection")]
    public void TestGroundDetection()
    {
        RaycastHit hit;
        bool hasGround = Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, LayerMask.GetMask("Default"));
        float distance = hasGround ? Vector3.Distance(transform.position, hit.point) : -1f;
        
        Debug.Log($"Test Ground Detection:");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Ground Hit: {hasGround}");
        Debug.Log($"Distance to Ground: {distance}");
        Debug.Log($"Expected Grounded: {distance <= 1.0f}");
        Debug.Log($"Current isGrounded: {isGrounded}");
    }
    
    // Optional: Add footstep sounds, camera shake, etc.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Handle collision effects
        if (hit.moveDirection.y < -0.3f && isGrounded)
        {
            // Landing effect
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Ground check raycasts - multiple points for accurate detection
        Gizmos.color = isGrounded ? Color.green : Color.red;
        
        float raycastDistance = 3.0f;
        
        // Center raycast
        Gizmos.DrawRay(transform.position, Vector3.down * raycastDistance);
        
        // Front, back, left, right raycasts
        Vector3 frontPoint = transform.position + transform.forward * 0.3f;
        Vector3 backPoint = transform.position - transform.forward * 0.3f;
        Vector3 leftPoint = transform.position - transform.right * 0.3f;
        Vector3 rightPoint = transform.position + transform.right * 0.3f;
        
        Gizmos.DrawRay(frontPoint, Vector3.down * raycastDistance);
        Gizmos.DrawRay(backPoint, Vector3.down * raycastDistance);
        Gizmos.DrawRay(leftPoint, Vector3.down * raycastDistance);
        Gizmos.DrawRay(rightPoint, Vector3.down * raycastDistance);
        
        // Ground hit points (if grounded)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
                Gizmos.DrawWireSphere(hit.point, 0.05f);
            if (Physics.Raycast(frontPoint, Vector3.down, out hit, raycastDistance))
                Gizmos.DrawWireSphere(hit.point, 0.05f);
            if (Physics.Raycast(backPoint, Vector3.down, out hit, raycastDistance))
                Gizmos.DrawWireSphere(hit.point, 0.05f);
            if (Physics.Raycast(leftPoint, Vector3.down, out hit, raycastDistance))
                Gizmos.DrawWireSphere(hit.point, 0.05f);
            if (Physics.Raycast(rightPoint, Vector3.down, out hit, raycastDistance))
                Gizmos.DrawWireSphere(hit.point, 0.05f);
        }
        
        // Wall checks
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, -transform.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, transform.right * wallCheckDistance);
        
        // Velocity visualization
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, velocity);
        
        // Player position indicator
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, Vector3.one);
        
        // Ground check points
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(frontPoint, Vector3.one * 0.1f);
        Gizmos.DrawWireCube(backPoint, Vector3.one * 0.1f);
        Gizmos.DrawWireCube(leftPoint, Vector3.one * 0.1f);
        Gizmos.DrawWireCube(rightPoint, Vector3.one * 0.1f);
    }
    
    // Debug info in Inspector
    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            // Get current ground distance for debugging
            RaycastHit hit;
            float raycastDistance = 3.0f;
            bool hasGround = Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance);
            float groundDistance = hasGround ? Vector3.Distance(transform.position, hit.point) : -1f;
            
            GUI.Label(new Rect(10, 10, 300, 120), 
                $"Grounded: {isGrounded}\n" +
                $"Velocity Y: {velocity.y:F2}\n" +
                $"Gravity: {gravity}\n" +
                $"Position Y: {transform.position.y:F2}\n" +
                $"Ground Distance: {groundDistance:F2}\n" +
                $"Ground Hit: {hasGround}\n" +
                $"Raycast Distance: {raycastDistance}");
        }
    }
}
