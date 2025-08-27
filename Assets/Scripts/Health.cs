using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvulnerable = false;
    
    [Header("Events")]
    [SerializeField] private UnityEvent _onDamage;
    [SerializeField] private UnityEvent _onDeath;
    [SerializeField] private UnityEvent<float> _onHealthChanged; // Passes health percentage (0-1)
    
    // Public event properties
    public UnityEvent onDamage => _onDamage;
    public UnityEvent onDeath => _onDeath;
    public UnityEvent<float> onHealthChanged => _onHealthChanged;
    
    // Properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead => currentHealth <= 0f;
    
    void Start()
    {
        currentHealth = maxHealth;
        _onHealthChanged?.Invoke(HealthPercentage);
    }
    
    public void TakeDamage(float damage)
    {
        if (isInvulnerable || IsDead) return;
        
        // Apply damage
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        
        // Invoke events
        _onDamage?.Invoke();
        _onHealthChanged?.Invoke(HealthPercentage);
        
        // Check if dead
        if (IsDead)
        {
            Die();
        }
        
        // Debug log
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }
    
    public void Heal(float amount)
    {
        if (IsDead) return;
        
        // Apply healing
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // Invoke events
        _onHealthChanged?.Invoke(HealthPercentage);
        
        // Debug log
        Debug.Log($"{gameObject.name} healed {amount} health. Health: {currentHealth}/{maxHealth}");
    }
    
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        _onHealthChanged?.Invoke(HealthPercentage);
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        _onHealthChanged?.Invoke(HealthPercentage);
    }
    
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
    
    public void Resurrect()
    {
        currentHealth = maxHealth;
        _onHealthChanged?.Invoke(HealthPercentage);
    }
    
    void Die()
    {
        // Invoke death event
        _onDeath?.Invoke();
        
        // Debug log
        Debug.Log($"{gameObject.name} died!");
        
        // You can add more death logic here (e.g., play death animation, drop items, etc.)
    }
    
    // Method to instantly kill the object
    public void Kill()
    {
        TakeDamage(currentHealth);
    }
    
    // Method to fully heal the object
    public void FullHeal()
    {
        Heal(maxHealth - currentHealth);
    }
}
