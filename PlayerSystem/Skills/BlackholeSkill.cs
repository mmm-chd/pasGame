using UnityEngine;

public class BlackholeSkill : MonoBehaviour
{
    [Header("Blackhole Settings")]
    public float pullForce = 12f;
    public float slowAmount = 0.4f;
    public float radius = 2f;
    public float duration = 2.2f;

    [Header("Range Upgrade Scaling")]
    public float rangeMultiplier = 1f;

    [Header("Damage Settings")]
    public float damage = 1f;  // <- 1 damage setiap 1 ms
    private float damageTimer = 0f;
    private float damageInterval = 0.2f; // 1ms = 0.001 detik

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        PullEnemies();
        DamageEnemiesPerMs();
    }

    // -------------------------------
    // DAMAGE EVERY MILLISECOND
    // -------------------------------
    void DamageEnemiesPerMs()
    {
        damageTimer += Time.deltaTime;

        while (damageTimer >= damageInterval)
        {
            ApplyDamageTick();
            damageTimer -= damageInterval;
        }
    }

    void ApplyDamageTick()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius * rangeMultiplier);

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyHealthSystem hp = enemy.GetComponent<EnemyHealthSystem>();
                if (hp != null)
                {
                    hp.TakeDamage(damage);
                }
            }
        }
    }

    // -------------------------------
    // PULL LOGIC
    // -------------------------------
    void PullEnemies()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius * rangeMultiplier);

        foreach (var enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy")) continue;

            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (transform.position - enemy.transform.position).normalized;
                rb.linearVelocity = dir * pullForce;
            }

            EnemyBehaviour eb = enemy.GetComponent<EnemyBehaviour>();
            if (eb != null)
            {
                eb.ApplyTemporarySlow(slowAmount, duration);
            }
        }
    }
}
