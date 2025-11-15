using System.Collections;
using UnityEngine;

public abstract class EnemyBehaviour : MonoBehaviour
{
    [Header("Basic Settings")]
    public float moveSpeed = 2f;
    public float chaseRange = 8f;
    public float attackRange = 1.5f;

    [Header("Blackhole Settings")]
    public bool isSlowedByBlackhole = false;

    protected Transform player;
    protected Rigidbody2D rb;
    protected float distanceToPlayer;

    private float originalMoveSpeed;
    private Vector2 moveDirection;
    public SpriteRenderer spriteRenderer;
    private Animator animator;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;

        originalMoveSpeed = moveSpeed;
    }

    protected virtual void Update()
    {
        if (player == null) return;

        // ============================================================
        // 🛡 SAFE ZONE CHECK — enemy cannot move or attack
        // ============================================================
        if (SafeZoneManager.Instance != null && SafeZoneManager.Instance.IsPlayerSafe())
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            return; // STOP ALL ENEMY BEHAVIOUR
        }

        // ============================================================
        // Normal AI Logic
        // ============================================================

        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardPlayer();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                AttackPlayer();
            }

            HandleAnimations();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
        }
    }

    protected virtual void MoveTowardPlayer()
    {
        moveDirection = (player.position - transform.position).normalized;
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    void HandleAnimations()
    {
        animator.SetBool("isMoving", rb.linearVelocity.magnitude > 0.01f);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (player.position.x > transform.position.x);
        }
    }

    public void ApplySlow(float slowPercent)
    {
        moveSpeed = originalMoveSpeed * slowPercent;
        isSlowedByBlackhole = true;
    }

    public void ResetSlow()
    {
        moveSpeed = originalMoveSpeed;
        isSlowedByBlackhole = false;
    }

    public void ApplyTemporarySlow(float slowAmount, float duration)
    {
        if (isSlowedByBlackhole) return;

        StartCoroutine(TemporarySlowRoutine(slowAmount, duration));
    }

    private IEnumerator TemporarySlowRoutine(float slowAmount, float duration)
    {
        isSlowedByBlackhole = true;
        float prevSpeed = moveSpeed;
        moveSpeed *= slowAmount;

        yield return new WaitForSeconds(duration);

        moveSpeed = prevSpeed;
        isSlowedByBlackhole = false;
    }

    protected abstract void AttackPlayer();
}
