using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnRate = 1.5f;
    public float horizontalRange = 10f;
    public float verticalRange = 5f;

    // ADD THIS LINE: A dedicated variable for the Z spawn position.
    public float spawnZPosition = 50f;

    void Start()
    {
        // Start the SpawnObstacles coroutine as soon as the game begins.
        StartCoroutine(SpawnObstacles());
    }

    private IEnumerator SpawnObstacles()
    {
        // This creates an infinite loop that will keep spawning obstacles.
        while (true)
        {
            // Wait for the specified amount of time before spawning the next obstacle.
            yield return new WaitForSeconds(spawnRate);

            // Determine a random position for the new obstacle.
            float randomX = Random.Range(-horizontalRange, horizontalRange);
            float randomY = Random.Range(-verticalRange, verticalRange);
            
            // CHANGE THIS LINE: Use the new spawnZPosition variable instead of transform.position.z
            Vector3 spawnPosition = new Vector3(randomX, randomY, spawnZPosition);

            // Create a new instance of the obstacle prefab at the calculated position.
            Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        }
    }
}