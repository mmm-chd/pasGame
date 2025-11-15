using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveInput;
    private Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Attack")]
    public GameObject blackholePrefab;
    public float baseCooldown = 2f;
    public float currentCooldown;
    private float lastAttackTime = -999f;
    public bool canAttack = true;

    [Header("Attack Stats")]
    public float blackholeDamage = 20f;
    public float blackholeRangeMultiplier = 1f;
    public float maxRangeMultiplier = 4f;

    [Header("Health System")]
    public HealthSystem healthSystem;

    [Header("Shield System")]
    public float shieldDuration = 3f;
    private bool isShieldActive = false;
    private float lastShieldTime;

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (healthSystem == null)
            healthSystem = GetComponent<HealthSystem>();

        currentCooldown = baseCooldown;
        lastMoveInput = new Vector2(0, -1);

        UISystem.Instance.UpdateCooldownDisplay(0);
    }

    void Update()
    {
        if (isDead) return;

        if (healthSystem.IsDead())
            Die();

        HandleMovementInput();
        HandleAnimations();
        HandleShieldBlink();

        if (canAttack && Input.GetMouseButtonDown(0))
            TryAttack();
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    void HandleMovementInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        if (moveInput != Vector2.zero)
            lastMoveInput = moveInput;
    }

    void HandleAnimations()
    {
        animator.SetBool("isWalking", moveInput != Vector2.zero);
        animator.SetFloat("moveX", moveInput.x);
        animator.SetFloat("moveY", moveInput.y);
        animator.SetFloat("lastMoveX", lastMoveInput.x);
        animator.SetFloat("lastMoveY", lastMoveInput.y);

        if (moveInput.x != 0)
            spriteRenderer.flipX = moveInput.x > 0;
    }

    void TryAttack()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsInventoryOpen) return;
        if (Time.time < lastAttackTime + currentCooldown) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z)
        );
        mousePos.z = 0f;

        GameObject bh = Instantiate(blackholePrefab, mousePos, Quaternion.identity);
        bh.transform.localScale *= blackholeRangeMultiplier;

        BlackholeSkill skill = bh.GetComponent<BlackholeSkill>();
        if (skill != null)
        {
            skill.damage = blackholeDamage;
            skill.rangeMultiplier = blackholeRangeMultiplier;
        }

        lastAttackTime = Time.time;
        UISystem.Instance.UpdateCooldownDisplay(currentCooldown);

        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        float remaining = currentCooldown;
        while (remaining > 0)
        {
            UISystem.Instance.UpdateCooldownDisplay(remaining);
            remaining -= Time.deltaTime;
            yield return null;
        }
        UISystem.Instance.UpdateCooldownDisplay(0);
    }

    // =========================
    // HEALTH WRAPPER
    // =========================
    public void TakeDamage(float dmg)
    {
        if (isShieldActive || isDead) return;

        healthSystem.TakeDamage(dmg);
        animator.SetTrigger("isHit");

        if (healthSystem.IsDead())
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        healthSystem.Heal((int)amount);
    }

    public void UpgradeMaxHP(int amount)
    {
        if (healthSystem != null)
            healthSystem.SetMaxHealth(healthSystem.maxHealth + amount);
    }

    // =========================
    // PLAYER DEATH
    // =========================
    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 🔥 Stop movement
        speed = 0f;
        moveInput = Vector2.zero;

        animator.SetTrigger("isDead");


        // 🔥 Stop rigidbody motion
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 🔥 Disable collider supaya tidak terdorong musuh
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Debug.Log("Player has died.");
    }


    // =========================
    // SHIELD
    // =========================
    public void ActivateShield()
    {
        isShieldActive = true;
        lastShieldTime = Time.time;
    }

    void HandleShieldBlink()
    {
        if (!isShieldActive) return;

        spriteRenderer.color = new Color(1, 1, 1, Mathf.PingPong(Time.time * 8f, 1));

        if (Time.time >= lastShieldTime + shieldDuration)
        {
            isShieldActive = false;
            spriteRenderer.color = Color.white;
        }
    }

    // Upgrade stats
    public void UpgradeDamage(float amount) => blackholeDamage += amount;
    public void UpgradeRange(float amount)
    {
        blackholeRangeMultiplier += amount;
        if (blackholeRangeMultiplier > maxRangeMultiplier)
            blackholeRangeMultiplier = maxRangeMultiplier;
    }
    public void UpgradeCooldown(float amount)
        => currentCooldown = Mathf.Max(0.2f, currentCooldown - amount);
}
