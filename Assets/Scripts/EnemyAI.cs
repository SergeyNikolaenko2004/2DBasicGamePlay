using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float patrolDistance = 3.0f;
    [SerializeField] private float chaseSpeed = 3.0f;
    [SerializeField] private float waitTimeAtPoint = 1.0f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 5.0f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stopDistance = 1.0f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float contactDamage = 10f;

    [Header("Components")]
    [SerializeField] private Transform graphics;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GroundSensor groundSensor;

    private Animator animator;
    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private Transform player;

    private Vector2 startPosition;
    private Vector2 patrolPointA;
    private Vector2 patrolPointB;
    private bool isFacingRight = false;
    private bool isDead = false;
    private bool isChasing = false;
    private bool isWaiting = false;
    private float timeSinceLastAttack = 0f;
    private float waitTimer = 0f;
    private float currentPatrolDirection = 1f;

    // Animation parameters
    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Grounded = Animator.StringToHash("Grounded");

    private enum EnemyState { Patrolling, Chasing, Attacking, Waiting, Dead }
    private EnemyState currentState = EnemyState.Patrolling;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        FindPlayer();
        InitializePatrolPoints();
        SubscribeToHealthEvents();
    }

    private void Update()
    {
        if (isDead) return;

        UpdateTimers();
        HandleStateMachine();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        HandleMovement();
    }

    #region Initialization
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void InitializePatrolPoints()
    {
        startPosition = transform.position;
        patrolPointA = startPosition + Vector2.left * patrolDistance;
        patrolPointB = startPosition + Vector2.right * patrolDistance;
    }

    private void SubscribeToHealthEvents()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += OnDamageTaken;
            healthSystem.OnDeath += OnDeath;
        }
    }
    #endregion

    #region State Management
    private void UpdateTimers()
    {
        timeSinceLastAttack += Time.deltaTime;
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
        }
    }

    private void HandleStateMachine()
    {
        if (currentState == EnemyState.Waiting)
        {
            HandleWaitingState();
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrollingState(distanceToPlayer);
                break;
            case EnemyState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
            case EnemyState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }
    }

    private void HandlePatrollingState(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
            isChasing = true;
        }
    }

    private void HandleChasingState(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange * 1.2f)
        {
            currentState = EnemyState.Patrolling;
            isChasing = false;
        }
        else if (distanceToPlayer <= attackRange && timeSinceLastAttack >= attackCooldown)
        {
            currentState = EnemyState.Attacking;
        }
    }

    private void HandleAttackingState(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void HandleWaitingState()
    {
        if (waitTimer >= waitTimeAtPoint)
        {
            isWaiting = false;
            waitTimer = 0f;
            currentState = EnemyState.Patrolling;
            currentPatrolDirection *= -1; // Меняем направление после ожидания
        }
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.Attacking:
                AttackBehavior();
                break;
            case EnemyState.Waiting:
            case EnemyState.Dead:
                StopMovement();
                break;
        }
    }

    private void Patrol()
    {
        if (isWaiting) return;

        float leftBound = patrolPointA.x;
        float rightBound = patrolPointB.x;

        // Проверяем достижение границы
        if ((currentPatrolDirection > 0 && transform.position.x >= rightBound) ||
            (currentPatrolDirection < 0 && transform.position.x <= leftBound))
        {
            StartWaiting();
            return;
        }

        // Двигаемся в текущем направлении
        rb.velocity = new Vector2(currentPatrolDirection * moveSpeed, rb.velocity.y);
        UpdateFacingDirection(currentPatrolDirection);
    }

    private void Chase()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= stopDistance)
        {
            StopMovement();
            return;
        }

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
        UpdateFacingDirection(direction);
    }

    private void AttackBehavior()
    {
        StopMovement();
        PerformAttack();
        currentState = EnemyState.Chasing; // Возвращаемся к преследованию после атаки
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = 0f;
        currentState = EnemyState.Waiting;
        StopMovement();
    }

    private void StopMovement()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    #endregion

    #region Combat
    private void PerformAttack()
    {
        animator.SetTrigger(Attack);
        timeSinceLastAttack = 0f;
        DetectAndDamagePlayer();
    }

    private void DetectAndDamagePlayer()
    {
        if (attackPoint == null) return;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        foreach (Collider2D playerCollider in hitPlayers)
        {
            HealthSystem playerHealth = playerCollider.GetComponent<HealthSystem>();
            playerHealth?.TakeDamage(attackDamage);
        }
    }
    #endregion

    #region Visual
    private void UpdateFacingDirection(float direction)
    {
        bool shouldFaceRight = direction > 0;
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = graphics.localScale;
        scale.x *= -1;
        graphics.localScale = scale;
    }

    private void UpdateAnimations()
    {
        // Movement animation
        bool isMoving = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon && !isWaiting;
        animator.SetInteger(AnimState, isMoving ? 2 : 0);

        // Grounded
        if (groundSensor != null)
        {
            animator.SetBool(Grounded, groundSensor.IsGrounded);
        }
    }
    #endregion

    #region Event Handlers
    private void OnDamageTaken(float damage)
    {
        if (isDead) return;
        animator.SetTrigger(Hurt);
    }

    private void OnDeath()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        StopMovement();
        animator.SetTrigger(Death);

        DisablePhysics();
        Destroy(gameObject, 2f);
    }

    private void DisablePhysics()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        rb.isKinematic = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>();
            playerHealth?.TakeDamage(contactDamage);
        }
    }
    #endregion

    #region Editor
    private void OnDrawGizmosSelected()
    {
        // Патрульные точки
        Gizmos.color = Color.blue;
        Vector2 visualStartPos = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Vector2 visualPatrolA = visualStartPos + Vector2.left * patrolDistance;
        Vector2 visualPatrolB = visualStartPos + Vector2.right * patrolDistance;

        Gizmos.DrawWireSphere(visualPatrolA, 0.2f);
        Gizmos.DrawWireSphere(visualPatrolB, 0.2f);
        Gizmos.DrawLine(visualPatrolA, visualPatrolB);

        // Зоны
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
    #endregion

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= OnDamageTaken;
            healthSystem.OnDeath -= OnDeath;
        }
    }
}