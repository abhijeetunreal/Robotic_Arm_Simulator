using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("Target Settings")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private LayerMask playerLayerMask = 1;
    
    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hitSound;
    
    // Components
    private Health health;
    private NavMeshAgent navAgent;
    private AudioSource audioSource;
    private Animator animator;
    
    // Enemy state
    private bool isDead = false;
    private bool isAttacking = false;
    private float lastAttackTime;
    private Vector3 startPosition;
    
    // Animation parameters
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    
    void Start()
    {
        // Get components
        health = GetComponent<Health>();
        navAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
        // Initialize
        startPosition = transform.position;
        lastAttackTime = -attackCooldown;
        
        // Configure NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = attackRange;
        }
        
        // Find player if not assigned
        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // Subscribe to health events
        if (health != null)
        {
            health.onDamage.AddListener(OnTakeDamage);
            health.onDeath.AddListener(OnDeath);
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        HandleMovement();
        HandleAttack();
    }
    
    void HandleMovement()
    {
        if (playerTarget == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Move towards player
            navAgent.SetDestination(playerTarget.position);
            
            // Update animation
            if (animator != null)
            {
                animator.SetBool(IsMovingHash, navAgent.velocity.magnitude > 0.1f);
            }
        }
        else
        {
            // Return to start position
            navAgent.SetDestination(startPosition);
            
            // Update animation
            if (animator != null)
            {
                animator.SetBool(IsMovingHash, navAgent.velocity.magnitude > 0.1f);
            }
        }
    }
    
    void HandleAttack()
    {
        if (playerTarget == null || isAttacking) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Check if player is in attack range
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }
    
    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Update animation
        if (animator != null)
        {
            animator.SetBool(IsAttackingHash, true);
        }
        
        // Deal damage to player
        if (playerTarget != null)
        {
            var playerHealth = playerTarget.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
        
        // Reset attack state after animation
        Invoke(nameof(ResetAttack), 1f);
    }
    
    void ResetAttack()
    {
        isAttacking = false;
        
        if (animator != null)
        {
            animator.SetBool(IsAttackingHash, false);
        }
    }
    
    void OnTakeDamage()
    {
        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // You can add hit effects here (particle effects, screen shake, etc.)
    }
    
    void OnDeath()
    {
        isDead = true;
        
        // Update animation
        if (animator != null)
        {
            animator.SetBool(IsDeadHash, true);
        }
        
        // Disable NavMeshAgent
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        
        // Disable collider to prevent further interactions
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Destroy enemy after delay (to allow death animation and sound to play)
        Destroy(gameObject, 3f);
        
        Debug.Log($"{gameObject.name} died!");
    }
    
    // Method to set player target
    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
    }
    
    // Method to get current health
    public float GetCurrentHealth()
    {
        return health != null ? health.CurrentHealth : 0f;
    }
    
    // Method to get max health
    public float GetMaxHealth()
    {
        return health != null ? health.MaxHealth : 0f;
    }
    
    // Method to check if enemy is dead
    public bool IsDead()
    {
        return isDead;
    }
    
    // Method to force death (for debugging or special events)
    public void ForceDeath()
    {
        if (health != null)
        {
            health.Kill();
        }
    }
    
    // Draw gizmos for debugging
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Start position
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(startPosition, Vector3.one);
    }
}
