using UnityEngine;

public class RangedEnemy : EnemyBehaviour
{
    [Header("Ranged Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    protected override void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        Debug.Log($"{name} fires a ranged attack!");
        Vector2 dir = (player.position - transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * bulletSpeed;

        lastAttackTime = Time.time;
    }
}
