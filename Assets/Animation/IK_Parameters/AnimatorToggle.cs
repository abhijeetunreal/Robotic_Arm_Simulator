using UnityEngine;
using TMPro; // Add this line to use TextMeshPro

public class AnimatorToggle : MonoBehaviour
{
    [Header("Animator Setup")]
    [Tooltip("The Animator component you want to control.")]
    public Animator targetAnimator;

    // --- NEW: Section for controlling the button's text ---
    [Header("Button Text Control")]
    [Tooltip("(Optional) The TextMeshPro component on the button to update.")]
    public TextMeshProUGUI buttonText;

    [Tooltip("The text to display when the panel is OPEN (e.g., 'Close').")]
    public string textWhenOpen = "Close";

    [Tooltip("The text to display when the panel is CLOSED (e.g., 'Open').")]
    public string textWhenClosed = "Open";
    // --- End of new section ---

    [Header("State & Trigger Names")]
    [Tooltip("The EXACT name of the animation state when the object is considered 'Open'.")]
    public string openStateName = "Panel_Open";

    [Tooltip("The name of the trigger to fire to begin the 'Open' animation.")]
    public string openTriggerName = "Open";

    [Tooltip("The name of the trigger to fire to begin the 'Close' animation.")]
    public string closeTriggerName = "Close";

    void Awake()
    {
        if (targetAnimator == null)
        {
            Debug.LogError("AnimatorToggle Error: The Target Animator has not been assigned!", this);
            enabled = false;
        }
    }

    void Start()
    {
        // NEW: Set the initial text of the button when the game starts.
        // Since the panel starts closed, the button's text should prompt the user to open it.
        if (buttonText != null)
        {
            buttonText.text = textWhenClosed;
        }
    }

    public void ToggleAnimationState()
    {
        if (targetAnimator == null) return;

        AnimatorStateInfo currentState = targetAnimator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName(openStateName))
        {
            // If it's open, fire the 'Close' trigger...
            targetAnimator.SetTrigger(closeTriggerName);
            // ...and update the button text to show the 'Open' prompt for the next time.
            if (buttonText != null)
            {
                buttonText.text = textWhenClosed;
            }
        }
        else
        {
            // If it's closed, fire the 'Open' trigger...
            targetAnimator.SetTrigger(openTriggerName);
            // ...and update the button text to show the 'Close' prompt for the next time.
            if (buttonText != null)
            {
                buttonText.text = textWhenOpen;
            }
        }
    }
}