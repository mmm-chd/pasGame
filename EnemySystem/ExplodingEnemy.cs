using UnityEngine;

public class ExplodingEnemy : EnemyBehaviour
{
    [Header("Explosion Settings")]
    public int explosionDamage = 20;

    protected override void AttackPlayer()
    {
        Debug.Log($"{name} explodes!");
        Explode();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Cari player di sekitar (bisa juga detect collision)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(explosionDamage);
                Debug.Log($"{name} dealt {explosionDamage} damage to player!");
            }
        }

        // NOTIFY UI bahwa enemy mati
        UISystem.Instance?.OnEnemyDied();

        // Destroy enemy
        Destroy(gameObject);
    }
}
