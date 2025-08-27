using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;
    [SerializeField] private bool useSceneRespawn = true;
    
    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject damageIndicator;
    
    // Components
    private Health health;
    private PlayerMovement playerMovement;
    private CharacterController characterController;
    
    // Player state
    private bool isDead = false;
    private Vector3 originalPosition;
    
    void Start()
    {
        // Get components
        health = GetComponent<Health>();
        playerMovement = GetComponent<PlayerMovement>();
        characterController = GetComponent<CharacterController>();
        
        // Store original position
        originalPosition = transform.position;
        
        // Set respawn position if not specified
        if (respawnPosition == Vector3.zero)
        {
            respawnPosition = originalPosition;
        }
        
        // Subscribe to health events
        if (health != null)
        {
            health.onDamage.AddListener(OnTakeDamage);
            health.onDeath.AddListener(OnPlayerDeath);
        }
        
        // Hide UI elements initially
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        if (damageIndicator != null)
        {
            damageIndicator.SetActive(false);
        }
    }
    
    void OnTakeDamage()
    {
        // Show damage indicator
        if (damageIndicator != null)
        {
            damageIndicator.SetActive(true);
            Invoke(nameof(HideDamageIndicator), 0.5f);
        }
        
        // You can add screen shake, blood effects, etc. here
        Debug.Log($"Player took damage! Health: {health.CurrentHealth}/{health.MaxHealth}");
    }
    
    void OnPlayerDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // Disable character controller
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        Debug.Log("Player died!");
        
        // Respawn after delay
        Invoke(nameof(Respawn), respawnDelay);
    }
    
    void Respawn()
    {
        if (useSceneRespawn)
        {
            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Respawn at specific position
            RespawnAtPosition();
        }
    }
    
    void RespawnAtPosition()
    {
        // Reset health
        if (health != null)
        {
            health.Resurrect();
        }
        
        // Reset position
        transform.position = respawnPosition;
        
        // Re-enable components
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Hide game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        // Reset state
        isDead = false;
        
        Debug.Log("Player respawned!");
    }
    
    void HideDamageIndicator()
    {
        if (damageIndicator != null)
        {
            damageIndicator.SetActive(false);
        }
    }
    
    // Public methods for external control
    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }
    
    public Vector3 GetRespawnPosition()
    {
        return respawnPosition;
    }
    
    public bool IsPlayerDead()
    {
        return isDead;
    }
    
    public void ForceRespawn()
    {
        if (isDead)
        {
            RespawnAtPosition();
        }
    }
    
    // Method to add health (for health pickups)
    public void AddHealth(float amount)
    {
        if (health != null && !isDead)
        {
            health.Heal(amount);
        }
    }
    
    // Method to set invulnerable (for power-ups)
    public void SetInvulnerable(bool invulnerable)
    {
        if (health != null)
        {
            health.SetInvulnerable(invulnerable);
        }
    }
}
