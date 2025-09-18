using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // Required for using Action events

public class AssemblyGameManager : MonoBehaviour
{
    // --- Events that other scripts can subscribe to ---
    public static event Action OnNewRoundStarted;
    public static event Action OnPartPickedUp;
    public static event Action OnAssemblyCompleted;

    [Header("1. Object References")]
    public GameObject assemblyPartPrefab;
    public GameObject targetLocationPrefab;
    public Transform playerTarget;
    public AirMouseInput airMouseInput;

    [Header("2. Game Parameters")]
    public float pickupDistance = 0.5f;
    public float assemblyDistance = 0.2f;
    
    [Header("3. Haptic Feedback")]
    [Range(0, 255)]
    public int pickupVibrationIntensity = 200;
    public int pickupVibrationDuration = 150;
    
    [Range(0, 255)]
    public int assemblyVibrationIntensity = 255;
    public int assemblyVibrationDuration = 300;
    
    private GameState currentState;
    private GameObject currentAssemblyPart;
    private GameObject currentTargetLocation;
    
    public GameState CurrentState => currentState;
    public GameObject CurrentAssemblyPart => currentAssemblyPart;
    public GameObject CurrentTargetLocation => currentTargetLocation;

    void Start() {
        StartNewRound();
    }
    void Update() {
        switch (currentState) {
            case GameState.AwaitingPickup: CheckForPickup(); break;
            case GameState.AssemblingPart: CheckForSuccessfulAssembly(); break;
        }
    }

    void CheckForPickup() {
        if (currentAssemblyPart == null) return;
        if (Vector3.Distance(playerTarget.position, currentAssemblyPart.transform.position) < pickupDistance) {
            currentAssemblyPart.transform.SetParent(playerTarget);
            currentAssemblyPart.transform.localPosition = Vector3.zero;
            if (airMouseInput != null) {
                airMouseInput.SendVibrationCommand(pickupVibrationIntensity, pickupVibrationDuration);
            }
            currentState = GameState.AssemblingPart;
            OnPartPickedUp?.Invoke();
        }
    }

    void CheckForSuccessfulAssembly() {
        if (currentAssemblyPart == null || currentTargetLocation == null) return;
        if (Vector3.Distance(currentAssemblyPart.transform.position, currentTargetLocation.transform.position) < assemblyDistance) {
            if (airMouseInput != null) {
                StartCoroutine(PlaySuccessVibrationPattern());
            }
            OnAssemblyCompleted?.Invoke();
            Destroy(currentAssemblyPart);
            Destroy(currentTargetLocation);
            Invoke(nameof(StartNewRound), 2f);
        }
    }
    
    IEnumerator PlaySuccessVibrationPattern() {
        airMouseInput.SendVibrationCommand(assemblyVibrationIntensity, 100);
        yield return new WaitForSeconds(0.15f);
        airMouseInput.SendVibrationCommand(assemblyVibrationIntensity, 100);
    }
    
    void StartNewRound() {
        Vector3 randomLocationPos = GetRandomPointInBounds(locationSpawnBounds);
        currentTargetLocation = Instantiate(targetLocationPrefab, randomLocationPos, Quaternion.identity);
        Vector3 randomPartPos = GetRandomPointInBounds(partSpawnBounds);
        currentAssemblyPart = Instantiate(assemblyPartPrefab, randomPartPos, Quaternion.identity);
        currentState = GameState.AwaitingPickup;
        OnNewRoundStarted?.Invoke();
    }

    // --- THIS IS THE FIXED METHOD ---
    private Vector3 GetRandomPointInBounds(Bounds bounds) {
        return new Vector3(
            // FIX: We explicitly state we want to use UnityEngine's Random class.
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(partSpawnBounds.center, partSpawnBounds.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(locationSpawnBounds.center, locationSpawnBounds.size);
    }
    
    public Bounds partSpawnBounds = new Bounds(new Vector3(0, 1, 3), new Vector3(4, 2, 4));
    public Bounds locationSpawnBounds = new Bounds(new Vector3(0, 1, -3), new Vector3(4, 2, 4));
    public enum GameState { AwaitingPickup, AssemblingPart }
}