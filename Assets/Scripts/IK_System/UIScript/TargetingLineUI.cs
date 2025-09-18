using UnityEngine;
using TMPro; // Required for TextMeshPro

[RequireComponent(typeof(LineRenderer))]
public class TargetingLineUI : MonoBehaviour
{
    [Header("Required References")]
    [Tooltip("Drag the GameManager object here.")]
    public AssemblyGameManager gameManager;
    
    [Tooltip("The TextMeshPro object used to display the distance.")]
    public TextMeshPro distanceText;

    private LineRenderer lineRenderer;
    private Transform playerTarget;

    void Awake()
    {
        // Get the LineRenderer component on this same object.
        lineRenderer = GetComponent<LineRenderer>();
        // Get a reference to the player's controllable target from the GameManager.
        playerTarget = gameManager.playerTarget;
    }

    void Start()
    {
        // Configure the LineRenderer
        lineRenderer.positionCount = 2; // A line has a start and an end.
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        
        // Hide the UI elements initially.
        lineRenderer.enabled = false;
        distanceText.gameObject.SetActive(false);
    }

    // We use LateUpdate to ensure the line is drawn after all objects have moved for the frame.
    void LateUpdate()
    {
        if (gameManager == null || playerTarget == null) return;

        Transform startPoint = null;
        Transform endPoint = null;

        // Determine the start and end points based on the current game state.
        switch (gameManager.CurrentState)
        {
            case AssemblyGameManager.GameState.AwaitingPickup:
                startPoint = playerTarget;
                if (gameManager.CurrentAssemblyPart != null)
                    endPoint = gameManager.CurrentAssemblyPart.transform;
                break;

            case AssemblyGameManager.GameState.AssemblingPart:
                if (gameManager.CurrentAssemblyPart != null)
                    startPoint = gameManager.CurrentAssemblyPart.transform;
                if (gameManager.CurrentTargetLocation != null)
                    endPoint = gameManager.CurrentTargetLocation.transform;
                break;
        }

        // If we have valid points to draw between...
        if (startPoint != null && endPoint != null)
        {
            // ...show the UI.
            lineRenderer.enabled = true;
            distanceText.gameObject.SetActive(true);

            // Update the line's start and end positions.
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, endPoint.position);

            // Calculate distance and update the text.
            float distance = Vector3.Distance(startPoint.position, endPoint.position);
            distanceText.text = distance.ToString("F2") + "m";

            // Position the text at the midpoint of the line.
            distanceText.transform.position = (startPoint.position + endPoint.position) / 2;
            
            // Make the text always face the camera.
            distanceText.transform.rotation = Quaternion.LookRotation(distanceText.transform.position - Camera.main.transform.position);
        }
        else
        {
            // If any object is missing (e.g., between rounds), hide the UI.
            lineRenderer.enabled = false;
            distanceText.gameObject.SetActive(false);
        }
    }
}