using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Setup")]
    public Camera mainCamera;
    public AirMouseInput airMouseInput;

    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Note: You may need to adjust this speed
    public float rollSpeed = 180f;
    
    [Tooltip("Controls how strongly the mouse cursor pulls the aircraft. 0 = no mouse control.")]
    [Range(0f, 1f)]
    public float mouseInfluence = 0.5f;

    [Header("Screen Boundaries")]
    [Tooltip("How far the plane stays from the screen edges.")]
    public float padding = 0.8f;
    
    private float minX, maxX, minY, maxY;
    private float camDistance;

    void Start()
    {
        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("Error: Main Camera has not been assigned in the Inspector! Disabling script.");
            enabled = false;
            return;
        }
        CalculateScreenBounds();
    }

    void Update()
    {
        // --- Get Keyboard & Sensor Directional Input ---
        float keyboardX = Input.GetAxis("Horizontal");
        float keyboardY = Input.GetAxis("Vertical");
        Vector2 keyboardSensorInput = new Vector2(keyboardX, keyboardY);
        
        if (airMouseInput != null)
        {
            keyboardSensorInput += airMouseInput.GetInput();
        }

        // --- Get Mouse Directional Input ---
        // Calculate a vector pointing from the aircraft to the mouse cursor
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = camDistance;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        
        // This vector's length is proportional to the distance from the cursor
        Vector3 mouseDirection = mouseWorldPosition - transform.position;

        // --- Combine All Inputs ---
        // Add the directional vector from the keyboard/sensor to the directional pull of the mouse.
        Vector3 combinedMove = new Vector3(keyboardSensorInput.x, keyboardSensorInput.y, 0) + (mouseDirection * mouseInfluence);
        
        // --- Calculate and Clamp New Position ---
        Vector3 newPosition = transform.position + combinedMove * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        transform.position = newPosition;

        // --- Handle Rolling (Unchanged) ---
        float keyboardRoll = Input.GetAxis("Roll");
        float sensorRoll = 0f;
        if (airMouseInput != null)
        {
            sensorRoll = airMouseInput.GetRollInput();
        }
        float totalRoll = keyboardRoll + sensorRoll;
        transform.Rotate(0, 0, -totalRoll * rollSpeed * Time.deltaTime);
    }
    
    void CalculateScreenBounds()
    {
        camDistance = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, camDistance));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, camDistance));
        minX = bottomLeft.x + padding;
        maxX = topRight.x - padding;
        minY = bottomLeft.y + padding;
        maxY = topRight.y - padding;
    }
}