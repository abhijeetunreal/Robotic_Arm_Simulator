using UnityEngine;
using TMPro;
using System.Collections;

public class GameLogUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshPro UI element to display messages.")]
    public TextMeshProUGUI logText;

    [Header("Display Parameters")]
    [Tooltip("How long a message stays on screen at full opacity before fading.")]
    public float messageDuration = 2.5f;
    [Tooltip("How quickly the message text fades out.")]
    public float fadeSpeed = 1f;

    private Coroutine _fadeCoroutine;

    // When this component is enabled, it subscribes to the game events.
    void OnEnable()
    {
        AssemblyGameManager.OnNewRoundStarted += HandleNewRound;
        AssemblyGameManager.OnPartPickedUp += HandlePartPickedUp;
        AssemblyGameManager.OnAssemblyCompleted += HandleAssemblyCompleted;
    }

    // When this component is disabled, it unsubscribes to prevent errors.
    void OnDisable()
    {
        AssemblyGameManager.OnNewRoundStarted -= HandleNewRound;
        AssemblyGameManager.OnPartPickedUp -= HandlePartPickedUp;
        AssemblyGameManager.OnAssemblyCompleted -= HandleAssemblyCompleted;
    }

    // --- Event Handlers ---

    private void HandleNewRound()
    {
        ShowMessage("New Round! Locate the next part.");
    }

    private void HandlePartPickedUp()
    {
        ShowMessage("Part Picked Up! Move it to the target location.");
    }

    private void HandleAssemblyCompleted()
    {
        ShowMessage("Assembly Complete! Well done.");
    }

    // --- Core Logic ---

    private void ShowMessage(string message)
    {
        // If a message is already fading, stop it.
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        // Set the text and make it fully visible.
        logText.text = message;
        logText.alpha = 1f;
        
        // Start the new fade-out process.
        _fadeCoroutine = StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        // Wait for the specified duration.
        yield return new WaitForSeconds(messageDuration);

        // Gradually fade the text's alpha to zero.
        while (logText.alpha > 0f)
        {
            logText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null; // Wait for the next frame.
        }
    }
}