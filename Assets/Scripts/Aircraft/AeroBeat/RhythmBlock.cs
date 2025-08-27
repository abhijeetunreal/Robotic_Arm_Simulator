using UnityEngine;

// This enum is used by the RhythmBlock script. You can place it here
// or in its own file.


public class RhythmBlock : MonoBehaviour
{
    [Tooltip("The type of this block. Set this in the prefab Inspector.")]
    public BlockType type;

    [Tooltip("How fast the block moves towards the player.")]
    public float speed = 20f;

    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

        if (transform.position.z < -10f)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (type == BlockType.Mine)
        {
            Debug.Log("<color=red>GAME OVER: Hit a Mine!</color>");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            return;
        }
        
        Debug.Log("<color=green>SUCCESSFUL SLASH!</color>");
        
        Destroy(gameObject);
    }
}