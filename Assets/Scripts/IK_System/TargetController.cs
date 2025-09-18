using UnityEngine;

// This enum is unchanged
public enum ControlMode { Mouse, AirMouse }

public class TargetController : MonoBehaviour
{
    [Header("1. Control Mode")]
    [Tooltip("Choose whether to control the target with the mouse or the Air Mouse sensor.")]
    public ControlMode currentControlMode = ControlMode.Mouse;

    [Header("2. Required Components")]
    [Tooltip("Drag the GameObject that has the AirMouseInput script attached here.")]
    public AirMouseInput airMouseInput;

    [Header("3. Mouse Settings")]
    [Tooltip("The distance the target will be from the camera's forward ray.")]
    public float distanceFromCamera = 10f;

    [Header("4. Air Mouse Settings")]
    [Tooltip("How fast the target moves in response to Pitch/Yaw from the sensor.")]
    public float airMouseMoveSpeed = 3f;
    [Tooltip("How fast the target rolls in response to Roll from the sensor.")]
    public float airMouseRollSpeed = 50f;

    // --- NEW: Bounding Box Settings ---
    [Header("5. Movement Bounds")]
    [Tooltip("Enable to restrict the target's movement within a defined box.")]
    public bool useBounds = true;

    [Tooltip("The center of the bounding box.")]
    public Vector3 boundsCenter = Vector3.zero;

    [Tooltip("The total size (width, height, depth) of the bounding box.")]
    public Vector3 boundsSize = new Vector3(10f, 5f, 10f);
    // --- End of New Settings ---


    void Update()
    {
        Vector3 newPosition;

        // Step 1: Calculate the desired new position based on the control mode
        switch (currentControlMode)
        {
            case ControlMode.AirMouse:
                // Air mouse roll is applied directly here, but position is calculated
                ApplyAirMouseRoll();
                newPosition = CalculateAirMousePosition();
                break;
            case ControlMode.Mouse:
            default:
                // Reset roll when in mouse mode
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * 5f);
                newPosition = CalculateMousePosition();
                break;
        }

        // Step 2: If bounds are enabled, clamp the new position
        if (useBounds)
        {
            newPosition = ApplyBounds(newPosition);
        }

        // Step 3: Apply the final, potentially clamped, position
        transform.position = newPosition;
    }

    private Vector3 CalculateMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return ray.GetPoint(distanceFromCamera);
    }

    private Vector3 CalculateAirMousePosition()
    {
        if (airMouseInput == null) return transform.position;

        Vector2 moveInput = airMouseInput.GetInput();
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0);
        
        // Return the potential new position
        return transform.position + (movement * airMouseMoveSpeed * Time.deltaTime);
    }

    private void ApplyAirMouseRoll()
    {
        if (airMouseInput == null) return;
        
        float rollInput = airMouseInput.GetRollInput();
        transform.Rotate(0, 0, -rollInput * airMouseRollSpeed * Time.deltaTime);
    }

    // --- NEW: Bounding Box Logic ---
    /// <summary>
    /// Clamps the given position to be within the defined bounds.
    /// </summary>
    /// <param name="position">The position to clamp.</param>
    /// <returns>The clamped position.</returns>
    private Vector3 ApplyBounds(Vector3 position)
    {
        Vector3 halfSize = boundsSize / 2f;
        Vector3 minBounds = boundsCenter - halfSize;
        Vector3 maxBounds = boundsCenter + halfSize;

        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        position.z = Mathf.Clamp(position.z, minBounds.z, maxBounds.z);

        return position;
    }

    /// <summary>
    /// Draws a visual representation of the bounds in the Scene editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // A nice orange color
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}