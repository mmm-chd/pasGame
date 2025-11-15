using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public int maxHealth = 100;
    public float currentHealth { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fillImage;

    [Header("Health Bar Colors")]
    [SerializeField] private Color maxHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    [Header("Animation (Optional)")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool useHitAnimation = true;
    [SerializeField] private bool useDeathAnimation = true;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip healSound;
    private AudioSource audioSource;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Get components if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        audioSource = GetComponent<AudioSource>();

        // Setup health bar
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;

            if (fillImage == null)
            {
                fillImage = healthBar.fillRect.GetComponent<Image>();
            }

            UpdateHealthBarColor();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return; // Prevent damage after death

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth); // Prevent negative health

        UpdateHealthBar();
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}");

        // Play hit sound
        PlaySound(hitSound);

        // Trigger hit animation (handled by PlayerController now)
        // But we keep this for non-player entities
        if (useHitAnimation && animator != null && currentHealth > 0)
        {
            animator.SetTrigger("isHit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return; // Cannot heal if dead

        float oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Don't exceed max

        float actualHealed = currentHealth - oldHealth;

        if (actualHealed > 0)
        {
            Debug.Log($"💚 {gameObject.name} healed {actualHealed}. Health: {currentHealth}/{maxHealth}");
            UpdateHealthBar();
            PlaySound(healSound);
        }
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        maxHealth = newMaxHealth;

        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            UpdateHealthBarColor();
        }
    }

    private void UpdateHealthBarColor()
    {
        if (fillImage != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;

            if (healthPercent > 0.5f)
            {
                // Between mid and max health (50% - 100%)
                float t = (healthPercent - 0.5f) * 2; // Normalize to 0-1
                fillImage.color = Color.Lerp(midHealthColor, maxHealthColor, t);
            }
            else
            {
                // Between low and mid health (0% - 50%)
                float t = healthPercent * 2; // Normalize to 0-1
                fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, t);
            }

            fillImage.enabled = healthPercent > 0; // Hide fill if health is zero
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void Die()
    {
        if (isDead) return; // Prevent multiple death calls

        isDead = true;
        Debug.Log($"{gameObject.name} has been defeated.");

        // Play death sound
        PlaySound(deathSound);

        // Trigger death animation
        if (useDeathAnimation && animator != null)
        {
            animator.SetTrigger("isDied");
        }

        // Save/Reset health for player
        if (gameObject.CompareTag("Player"))
        {

            // Reset difficulty
            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.ResetProgress();
            }
        }

        // Destroy with delay if death animation exists
        if (useDeathAnimation && animator != null)
        {
            Destroy(gameObject, 2f); // Wait for death animation
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Public getter methods
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }

    public bool IsLowHealth(float threshold = 0.3f)
    {
        return GetHealthPercent() <= threshold;
    }
}