using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Base Health Settings")]
    [SerializeField] private float baseHealth = 100;
    [SerializeField] private float healthIncreasePerLevel = 25;
    [SerializeField] private float bossHealthMultiplier = 1.5f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color maxHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    private float maxHealth;
    private float currentHealth;
    private bool isDead = false; // ✅ TAMBAHKAN FLAG

    void Start()
    {
        int level = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.currentLevel
            : 1;

        maxHealth = baseHealth + (healthIncreasePerLevel * (level - 1));

        if (DifficultyManager.Instance != null && DifficultyManager.Instance.IsBossWave())
        {
            maxHealth = Mathf.Round(maxHealth * bossHealthMultiplier);
        }

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;

            if (fillImage == null)
                fillImage = healthBar.fillRect.GetComponent<Image>();

            UpdateHealthBarColor();
        }

        // ✅ UPDATE UI SAAT START
        UISystem.Instance?.UpdateEnemyCount();
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return; // ✅ PREVENT DAMAGE AFTER DEATH

        currentHealth -= Mathf.RoundToInt(dmg);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
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
            float hpPercent = currentHealth / maxHealth;

            if (hpPercent > 0.5f)
            {
                float t = (hpPercent - 0.5f) * 2;
                fillImage.color = Color.Lerp(midHealthColor, maxHealthColor, t);
            }
            else
            {
                float t = hpPercent * 2;
                fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, t);
            }

            fillImage.enabled = hpPercent > 0;
        }
    }

    private void Die()
    {
        if (isDead) return; // ✅ PREVENT MULTIPLE DEATH CALLS
        isDead = true;

        // ✅ DISABLE COLLIDER IMMEDIATELY
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // ✅ REMOVE TAG IMMEDIATELY
        gameObject.tag = "Untagged";

        // Drop items
        CollectibleDropSystem dropSystem = GetComponent<CollectibleDropSystem>();
        if (dropSystem != null)
            dropSystem.DropCollectibles();

        // ✅ UPDATE UI IMMEDIATELY
        UISystem.Instance?.OnEnemyDied();

        // ✅ DESTROY IMMEDIATELY (no delay)
        Destroy(gameObject);
    }
}