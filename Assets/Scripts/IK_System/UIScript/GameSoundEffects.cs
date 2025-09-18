using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameSoundEffects : MonoBehaviour
{
    [Header("Sound Clips")]
    [Tooltip("The sound to play when a part is picked up.")]
    public AudioClip pickupSound;
    
    [Tooltip("The sound to play on a successful assembly.")]
    public AudioClip assemblySuccessSound;

    private AudioSource audioSource;

    void Awake()
    {
        // Get the AudioSource component on this GameObject.
        audioSource = GetComponent<AudioSource>();
    }

    // When this object is enabled, it starts listening for game events.
    void OnEnable()
    {
        AssemblyGameManager.OnPartPickedUp += PlayPickupSound;
        AssemblyGameManager.OnAssemblyCompleted += PlayAssemblySuccessSound;
    }

    // When this object is disabled, it stops listening to prevent errors.
    void OnDisable()
    {
        AssemblyGameManager.OnPartPickedUp -= PlayPickupSound;
        AssemblyGameManager.OnAssemblyCompleted -= PlayAssemblySuccessSound;
    }

    private void PlayPickupSound()
    {
        // Check if a sound is assigned before trying to play it.
        if (pickupSound != null)
        {
            // PlayOneShot is perfect for sound effects that might overlap.
            audioSource.PlayOneShot(pickupSound);
        }
    }

    private void PlayAssemblySuccessSound()
    {
        if (assemblySuccessSound != null)
        {
            audioSource.PlayOneShot(assemblySuccessSound);
        }
    }
}