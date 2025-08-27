using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private LayerMask hitLayers = -1;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSound;
    
    // Components
    private Rigidbody rb;
    private AudioSource audioSource;
    
    // Bullet state
    private bool hasHit = false;
    private float spawnTime;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // Set spawn time
        spawnTime = Time.time;
        
        // Set initial velocity if Rigidbody exists
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = transform.forward * bulletSpeed;
        }
        
        // Destroy bullet after lifetime
        Destroy(gameObject, bulletLifetime);
    }
    
    void Update()
    {
        // Check if bullet has exceeded lifetime
        if (Time.time - spawnTime > bulletLifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        hasHit = true;
        
        // Check if we hit a valid target
        if (((1 << collision.gameObject.layer) & hitLayers) != 0)
        {
            // Apply damage to the hit object
            ApplyDamage(collision.gameObject);
        }
        
        // Play hit effects
        PlayHitEffects(collision.contacts[0].point, collision.contacts[0].normal);
        
        // Destroy the bullet
        Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        hasHit = true;
        
        // Check if we hit a valid target
        if (((1 << other.gameObject.layer) & hitLayers) != 0)
        {
            // Apply damage to the hit object
            ApplyDamage(other.gameObject);
        }
        
        // Play hit effects
        PlayHitEffects(transform.position, -transform.forward);
        
        // Destroy the bullet
        Destroy(gameObject);
    }
    
    void ApplyDamage(GameObject target)
    {
        // Try to get a health component
        var health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        
        // You can add more damage logic here (e.g., different damage types, armor penetration, etc.)
    }
    
    void PlayHitEffects(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Spawn hit effect
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(effect, 2f); // Destroy effect after 2 seconds
        }
        
        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    // Method to set bullet properties (called by the gun)
    public void SetBulletProperties(float newDamage, float newSpeed, float newLifetime)
    {
        damage = newDamage;
        bulletSpeed = newSpeed;
        bulletLifetime = newLifetime;
    }
    
    // Method to set bullet direction (called by the gun)
    public void SetDirection(Vector3 direction)
    {
        transform.forward = direction.normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }
}
