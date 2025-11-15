using UnityEngine;

public class MeleeEnemy : EnemyBehaviour
{
    [Header("Melee Settings")]
    public int damage = 10;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;
    private Animator animator;

    protected override void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        Debug.Log($"{name} performs melee attack!");
        player.GetComponent<HealthSystem>()?.TakeDamage(damage);

        lastAttackTime = Time.time;
    }
}
