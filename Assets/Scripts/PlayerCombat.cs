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

    [Header("Attack Sounds")]
    [SerializeField] private AudioClip attackMissSound; 
    [SerializeField] private AudioClip attackHitSound;  
    [SerializeField] private AudioSource audioSource;  
    [SerializeField] private float attackVolume = 0.7f; 

    private float timeSinceLastAttack = 0f;
    private int lastAttackIndex = 0;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool hitEnemyThisAttack = false;

    private static readonly int Attack1 = Animator.StringToHash("Attack1");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
    private static readonly int Attack3 = Animator.StringToHash("Attack3");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        if (healthSystem == null)
            healthSystem = GetComponent<HealthSystem>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.spatialBlend = 0f; 
        audioSource.playOnAwake = false;
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
        if (!canAttack || isAttacking || healthSystem.IsBlocking) return;

        timeSinceLastAttack += Time.deltaTime;
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.H) && timeSinceLastAttack >= attackCooldown && canAttack && !healthSystem.IsBlocking)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        int nextAttackIndex = GetNextAttackIndex();

        animator.ResetTrigger(Attack1);
        animator.ResetTrigger(Attack2);
        animator.ResetTrigger(Attack3);

        hitEnemyThisAttack = false;

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

        isAttacking = true;
        animator.SetBool(IsAttacking, true);

        lastAttackIndex = nextAttackIndex;
        timeSinceLastAttack = 0f;

        Debug.Log($"Performing attack: {nextAttackIndex}");
    }

    public void OnAttackHit()
    {
        DetectAndDamageEnemies();
        PlayAttackSound();
    }

    private void PlayAttackSound()
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = hitEnemyThisAttack ? attackHitSound : attackMissSound;

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay, attackVolume);
            Debug.Log($"Playing sound: {(hitEnemyThisAttack ? "Hit" : "Miss")}");
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        animator.SetBool(IsAttacking, false);

        hitEnemyThisAttack = false;
    }

    private void DetectAndDamageEnemies()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        bool hitAnyEnemy = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null && !enemyHealth.IsInvincible)
            {
                enemyHealth.TakeDamage(attackDamage);
                hitAnyEnemy = true;
                Debug.Log($"Hit enemy: {enemy.name} for {attackDamage} damage");
            }
        }

        hitEnemyThisAttack = hitAnyEnemy;
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
        if (isAttacking)
        {
            OnAttackEnd();
        }
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