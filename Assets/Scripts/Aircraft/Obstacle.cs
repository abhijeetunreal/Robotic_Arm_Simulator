// In Scripts folder, create a new C# Script named Obstacle.cs
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Public variable for obstacle speed.
    public float speed = 15f;

    void Update()
    {
        // Move the obstacle forward (along its local Z-axis).
        // We use -Vector3.forward to move it towards the negative Z direction.
        transform.Translate(-Vector3.forward * speed * Time.deltaTime);

        // Destroy the obstacle if it goes too far behind the player to save memory.
        if (transform.position.z < -5f)
        {
            Destroy(gameObject);
        }
    }
}