using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float reloadTime = 2f;
    
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask bulletLayerMask = 1;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    
    [Header("Recoil")]
    [SerializeField] private float recoilForce = 2f;
    [SerializeField] private float recoilRecoverySpeed = 5f;
    
    // Components
    private AudioSource audioSource;
    private Camera playerCamera;
    
    // Gun state
    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;
    private Vector3 originalRotation;
    private Vector3 currentRecoil;
    
    // Input
    private bool firePressed;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerCamera = Camera.main;
        
        // Initialize gun state
        currentAmmo = maxAmmo;
        originalRotation = transform.localEulerAngles;
        
        // Configure AudioSource
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
        
        // Validate fire point
        if (firePoint == null)
        {
            firePoint = transform;
            Debug.LogWarning("Gun: No fire point assigned, using gun transform as fire point");
        }
    }
    
    void Update()
    {
        HandleInput();
        HandleFiring();
        HandleRecoil();
    }
    
    void HandleInput()
    {
        // Check for mouse input (left click)
        if (Input.GetMouseButtonDown(0))
        {
            firePressed = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            firePressed = false;
        }
        
        // Check for reload input (R key)
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            StartReload();
        }
    }
    
    void HandleFiring()
    {
        if (firePressed && CanFire() && Time.time >= nextFireTime)
        {
            Fire();
        }
    }
    
    bool CanFire()
    {
        return currentAmmo > 0 && !isReloading;
    }
    
    void Fire()
    {
        // Update fire rate timer
        nextFireTime = Time.time + fireRate;
        
        // Decrease ammo
        currentAmmo--;
        
        // Create bullet
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            
            if (bulletRb != null)
            {
                bulletRb.isKinematic = false;
                bulletRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                bulletRb.interpolation = RigidbodyInterpolation.Interpolate;
                bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
            }
            
            // Destroy bullet after lifetime
            Destroy(bullet, bulletLifetime);
        }
        
        // Apply recoil
        ApplyRecoil();
        
        // Play effects
        PlayFireEffects();
        
        // Check if out of ammo
        if (currentAmmo <= 0)
        {
            PlayEmptySound();
        }
    }
    
    void ApplyRecoil()
    {
        // Calculate recoil
        Vector3 recoil = new Vector3(
            Random.Range(-recoilForce, recoilForce),
            Random.Range(-recoilForce, recoilForce),
            0f
        );
        
        currentRecoil += recoil;
        
        // Apply recoil to gun rotation
        transform.localEulerAngles = originalRotation + currentRecoil;
    }
    
    void HandleRecoil()
    {
        // Gradually recover from recoil
        if (currentRecoil.magnitude > 0.01f)
        {
            currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);
            transform.localEulerAngles = originalRotation + currentRecoil;
        }
        else
        {
            currentRecoil = Vector3.zero;
            transform.localEulerAngles = originalRotation;
        }
    }
    
    void StartReload()
    {
        if (currentAmmo == maxAmmo || isReloading)
            return;
        
        isReloading = true;
        
        // Play reload sound
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        
        // Reload after delay
        Invoke(nameof(FinishReload), reloadTime);
    }
    
    void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    
    void PlayFireEffects()
    {
        // Play muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        
        // Play fire sound
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }
    
    void PlayEmptySound()
    {
        if (emptySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(emptySound);
        }
    }
    
    // Public methods for UI and other systems
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }
    
    public int GetMaxAmmo()
    {
        return maxAmmo;
    }
    
    public bool IsReloading()
    {
        return isReloading;
    }
    
    public float GetReloadProgress()
    {
        if (!isReloading)
            return 1f;
        
        float elapsed = Time.time - (nextFireTime - fireRate);
        return Mathf.Clamp01(elapsed / reloadTime);
    }
    
    // Method to add ammo (for pickups)
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
    }
    
    // Method to set ammo (for cheats/debug)
    public void SetAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(amount, 0, maxAmmo);
    }
}
