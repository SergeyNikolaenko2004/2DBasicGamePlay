using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private HealthSystem healthSystem;

    private float timeSinceLastAttack = 0f;
    private int lastAttackIndex = 0;
    private bool canAttack = true;

    // Animation parameters
    private static readonly int Attack1 = Animator.StringToHash("Attack1");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
    private static readonly int Attack3 = Animator.StringToHash("Attack3");

    private void Awake()
    {
        if (healthSystem == null)
            healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += OnDamageTaken;
            healthSystem.OnInvincibilityStart += OnInvincibilityStart;
            healthSystem.OnInvincibilityEnd += OnInvincibilityEnd;
        }
    }

    private void Update()
    {
        if (!canAttack) return;

        timeSinceLastAttack += Time.deltaTime;
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.H) && timeSinceLastAttack >= attackCooldown && canAttack)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        int nextAttackIndex = GetNextAttackIndex();

        // Сбрасываем все триггеры
        animator.ResetTrigger(Attack1);
        animator.ResetTrigger(Attack2);
        animator.ResetTrigger(Attack3);

        // Запускаем анимацию
        switch (nextAttackIndex)
        {
            case 1:
                animator.SetTrigger(Attack1);
                break;
            case 2:
                animator.SetTrigger(Attack2);
                break;
            case 3:
                animator.SetTrigger(Attack3);
                break;
        }

        // Наносим урон врагам в зоне атаки
        DetectAndDamageEnemies();

        lastAttackIndex = nextAttackIndex;
        timeSinceLastAttack = 0f;

        Debug.Log($"Performing attack: {nextAttackIndex}");
    }

    private void DetectAndDamageEnemies()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null && !enemyHealth.IsInvincible)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"Hit enemy: {enemy.name} for {attackDamage} damage");
            }
        }
    }

    private int GetNextAttackIndex()
    {
        int nextAttack;
        do
        {
            nextAttack = Random.Range(1, 4);
        }
        while (nextAttack == lastAttackIndex);

        return nextAttack;
    }

    #region Event Handlers
    private void OnDamageTaken(float damage)
    {
        canAttack = false;
    }

    private void OnInvincibilityStart()
    {
        canAttack = false;
    }

    private void OnInvincibilityEnd()
    {
        canAttack = true;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= OnDamageTaken;
            healthSystem.OnInvincibilityStart -= OnInvincibilityStart;
            healthSystem.OnInvincibilityEnd -= OnInvincibilityEnd;
        }
    }
}