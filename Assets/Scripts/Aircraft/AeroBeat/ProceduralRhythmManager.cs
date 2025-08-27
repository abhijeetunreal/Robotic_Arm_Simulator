using UnityEngine;

public class ProceduralRhythmManager : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioClip song;
    private AudioSource audioSource;

    [Header("Spawning Setup")]
    public float spawnZPosition = 100f;
    public float horizontalSpawnRange = 8f;
    public float verticalSpawnRange = 5f;

    [Header("Block Prefabs")]
    public GameObject horizontalBlockPrefab;
    public GameObject verticalBlockPrefab;
    public GameObject minePrefab;

    [Header("Beat Detection Tuning")]
    [Range(0f, 1f)]
    public float bassThreshold = 0.5f;
    [Range(0f, 1f)]
    public float midThreshold = 0.4f;
    [Range(0f, 1f)]
    public float highThreshold = 0.3f;
    public float beatCooldown = 0.25f;
    private float lastBeatTime = 0f;

    // --- Private variables for audio analysis ---
    private float[] spectrumData = new float[512];
    // Define the index ranges for our frequency bands
    private const int BASS_START_INDEX = 1;
    private const int BASS_END_INDEX = 4;
    private const int MID_START_INDEX = 10;
    private const int MID_END_INDEX = 30;
    private const int HIGH_START_INDEX = 40;
    private const int HIGH_END_INDEX = 100;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = song;
        audioSource.Play();
    }

    void Update()
    {
        if (!audioSource.isPlaying) return;

        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

        float bassEnergy = GetEnergyInRange(BASS_START_INDEX, BASS_END_INDEX);
        float midEnergy = GetEnergyInRange(MID_START_INDEX, MID_END_INDEX);
        float highEnergy = GetEnergyInRange(HIGH_START_INDEX, HIGH_END_INDEX);
        
        if (Time.time - lastBeatTime > beatCooldown)
        {
            if (bassEnergy > bassThreshold) {
                // Find the dominant pitch within the bass range
                int dominantIndex = GetDominantIndexInRange(BASS_START_INDEX, BASS_END_INDEX);
                // Spawn the block, passing in the beat properties
                SpawnBlock(horizontalBlockPrefab, dominantIndex, BASS_START_INDEX, BASS_END_INDEX, BlockType.Horizontal);
            } else if (midEnergy > midThreshold) {
                int dominantIndex = GetDominantIndexInRange(MID_START_INDEX, MID_END_INDEX);
                SpawnBlock(verticalBlockPrefab, dominantIndex, MID_START_INDEX, MID_END_INDEX, BlockType.Vertical);
            } else if (highEnergy > highThreshold) {
                int dominantIndex = GetDominantIndexInRange(HIGH_START_INDEX, HIGH_END_INDEX);
                SpawnBlock(minePrefab, dominantIndex, HIGH_START_INDEX, HIGH_END_INDEX, BlockType.Mine);
            }
        }
    }
    
    // This function finds the index (representing pitch) with the most energy in a given band
    private int GetDominantIndexInRange(int startIndex, int endIndex)
    {
        int dominantIndex = startIndex;
        float maxEnergy = 0;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (spectrumData[i] > maxEnergy)
            {
                maxEnergy = spectrumData[i];
                dominantIndex = i;
            }
        }
        return dominantIndex;
    }

    private float GetEnergyInRange(int startIndex, int endIndex)
    {
        float totalEnergy = 0;
        for (int i = startIndex; i <= endIndex; i++)
        {
            totalEnergy += spectrumData[i];
        }
        return totalEnergy / (endIndex - startIndex + 1);
    }
    
    // This SpawnBlock function is now much smarter and no longer uses Random.Range for position
    void SpawnBlock(GameObject prefab, int dominantIndex, int bandMinIndex, int bandMaxIndex, BlockType type)
    {
        // --- Calculate X position based on pitch ---
        // We map the dominant frequency index to a horizontal "lane"
        float xPos = Map(dominantIndex, bandMinIndex, bandMaxIndex, -horizontalSpawnRange, horizontalSpawnRange);

        // --- Calculate Y position based on frequency band ---
        // This makes bass notes appear low, mid notes in the middle, and high notes high up.
        float yPos = 0;
        switch (type)
        {
            case BlockType.Horizontal: // Bass
                yPos = Random.Range(-verticalSpawnRange, -verticalSpawnRange / 3); // Bottom third
                break;
            case BlockType.Vertical:   // Mids
                yPos = Random.Range(-verticalSpawnRange / 3, verticalSpawnRange / 3); // Middle third
                break;
            case BlockType.Mine:       // Highs
                yPos = Random.Range(verticalSpawnRange / 3, verticalSpawnRange); // Top third
                break;
        }

        Vector3 spawnPosition = new Vector3(xPos, yPos, spawnZPosition);
        Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        lastBeatTime = Time.time;
    }
    
    // Helper function to map a value from one range to another (like Arduino's map() function)
    private float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}