using UnityEngine;

// This defines the different types of blocks we can spawn.
// We'll add more later, like Diagonal and Ring types.


// This class holds the information for a single event in the song.
// [System.Serializable] makes it show up in the Inspector.
[System.Serializable]
public class Note
{
    [Tooltip("The type of block to spawn.")]
    public BlockType type;

    [Tooltip("The time in seconds into the song to spawn this block.")]
    public float spawnTime;

    [Tooltip("The (X, Y) position where the block will appear. Z is handled by the spawner.")]
    public Vector2 position;
}

// This is the main BeatMap asset. It holds the song and a list of all the notes.
[CreateAssetMenu(fileName = "New BeatMap", menuName = "Aero Beat/BeatMap")]
public class BeatMap : ScriptableObject
{
    [Tooltip("The music file for this level.")]
    public AudioClip song;

    [Tooltip("The list of all notes to be spawned during the song.")]
    public Note[] notes;
}