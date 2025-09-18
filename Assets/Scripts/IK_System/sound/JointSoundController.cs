using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JointSoundController : MonoBehaviour
{
    public enum Waveform { Sine, Sawtooth, Square }

    [Header("--- Main Sound Shape (The Whine) ---")]
    [Tooltip("The base waveform of the sound. Sawtooth often sounds the most like a motor.")]
    public Waveform waveformType = Waveform.Sawtooth;
    [Tooltip("How much random noise to mix into the main whine.")]
    [Range(0f, 1f)]
    public float noiseAmount = 0.2f;

    [Header("--- Mechanical Clicks ---")]
    [Tooltip("Enable the rapid clicking sound of a stepper motor or gears.")]
    public bool enableClicks = true;
    [Tooltip("How many clicks happen per second based on movement speed.")]
    public float clickFrequency = 20f;
    [Tooltip("How loud the clicks are.")]
    [Range(0f, 1f)]
    public float clickVolume = 0.5f;
    [Tooltip("How quickly the click sound fades out. A lower value (e.g., 0.9) makes a sharper tick.")]
    [Range(0.8f, 0.999f)]
    public float clickDecay = 0.95f;

    [Header("--- Mechanical Grind ---")]
    [Tooltip("Enable a low-frequency grinding/wobble effect.")]
    public bool enableGrind = true;
    [Tooltip("The speed of the grinding wobble.")]
    public float grindFrequency = 30f;
    [Tooltip("The intensity of the grinding wobble.")]
    [Range(0f, 0.5f)]
    public float grindAmount = 0.1f;
    
    [Header("--- General Parameters ---")]
    [Tooltip("Overall volume. Keep this low (e.g., 0.05).")]
    [Range(0f, 1f)]
    public float masterVolume = 0.05f;
    [Tooltip("How much speed affects the whine's pitch.")]
    public float frequencyMultiplier = 10f;
    [Tooltip("The base pitch of the whine.")]
    public float baseFrequency = 50f;
    [Tooltip("How smoothly the sound fades and changes pitch.")]
    public float soundSmoothing = 8f;

    // --- Private Variables ---
    private AudioSource audioSource;
    private Quaternion lastRotation;
    private float currentAmplitude, currentFrequency;
    private float phase, clickPhase, grindPhase;
    private float currentClickAmplitude;
    private double sampleRate;
    private System.Random random = new System.Random();

    void Awake() {
        audioSource = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;
        audioSource.playOnAwake = true;
        audioSource.loop = true;
        audioSource.Play();
    }
    void Start() { lastRotation = transform.rotation; }

    void Update() {
        Quaternion currentRotation = transform.rotation;
        float angleDifference = Quaternion.Angle(lastRotation, currentRotation);
        float angularSpeed = angleDifference / Time.deltaTime;

        float targetAmplitude = (angularSpeed > 0.1f) ? masterVolume : 0f;
        float targetFrequency = baseFrequency + (angularSpeed * frequencyMultiplier);

        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * soundSmoothing);
        currentFrequency = Mathf.Lerp(currentFrequency, targetFrequency, Time.deltaTime * soundSmoothing);
        
        lastRotation = currentRotation;
    }
    
    void OnAudioFilterRead(float[] data, int channels) {
        float phaseIncrement = (currentFrequency * 2f * Mathf.PI) / (float)sampleRate;
        float clickPhaseIncrement = (clickFrequency * currentFrequency * 2f * Mathf.PI) / (float)sampleRate;
        float grindPhaseIncrement = (grindFrequency * 2f * Mathf.PI) / (float)sampleRate;

        for (int i = 0; i < data.Length; i += channels) {
            // --- 1. The Whine Layer ---
            phase += phaseIncrement;
            float waveSample = 0f;
            switch (waveformType) {
                case Waveform.Sine: waveSample = Mathf.Sin(phase); break;
                case Waveform.Sawtooth: waveSample = (phase / Mathf.PI) % 2 - 1; break;
                case Waveform.Square: waveSample = Mathf.Sign(Mathf.Sin(phase)); break;
            }
            float noiseSample = (float)(random.NextDouble() * 2.0 - 1.0);
            float whineLayer = Mathf.Lerp(waveSample, noiseSample, noiseAmount);

            // --- 2. The Clicks Layer ---
            float clickLayer = 0f;
            if (enableClicks) {
                clickPhase += clickPhaseIncrement;
                if (clickPhase > (2f * Mathf.PI)) {
                    clickPhase -= (2f * Mathf.PI);
                    currentClickAmplitude = clickVolume; // Trigger a new click
                }
                clickLayer = (float)(random.NextDouble() * 2.0 - 1.0) * currentClickAmplitude;
                currentClickAmplitude *= clickDecay; // Decay the click volume
            }
            
            // --- 3. The Grind Layer ---
            float grindModulator = 1.0f;
            if (enableGrind) {
                grindPhase += grindPhaseIncrement;
                grindModulator = 1.0f - grindAmount + (Mathf.Sin(grindPhase) * grindAmount);
            }

            // --- Mix all layers ---
            float finalSample = (whineLayer * grindModulator) + clickLayer;
            finalSample *= currentAmplitude; // Apply master volume and fade

            for (int j = 0; j < channels; j++) {
                data[i + j] = finalSample;
            }
        }
        
        // Wrap phases
        if (phase > (2f * Mathf.PI)) phase -= (2f * Mathf.PI);
        if (grindPhase > (2f * Mathf.PI)) grindPhase -= (2f * Mathf.PI);
    }
}