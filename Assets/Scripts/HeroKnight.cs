using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Components")]
    [SerializeField] private GroundSensor groundSensor;
    [SerializeField] private Transform graphics;

    private Rigidbody2D rb;
    private Animator animator;
    private HealthSystem healthSystem;
    private PlayerCombat combat;

    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isJumping;

    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int AirSpeedY = Animator.StringToHash("AirSpeedY");
    private static readonly int Hurt = Animator.StringToHash("Hurt");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {

        combat = GetComponent<PlayerCombat>();

        // Subscribe to events
        if (groundSensor != null)
        {
            groundSensor.OnGrounded += OnGrounded;
            groundSensor.OnLeftGround += OnLeftGround;
        }

        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += OnDamageTaken;
            healthSystem.OnDeath += OnDeath;
        }

        healthSystem.OnDamageTaken += (damage) => {
            animator.SetTrigger("Hurt");
        };

        healthSystem.OnDeath += () => {
            enabled = false;
            animator.SetTrigger("Death");
        };
    }

    private void Update()
    {

        HandleInput();
        UpdateAnimations();
        HandleFlip();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && groundSensor.IsGrounded)
        {
            JumpAction();
        }
    }

    private void HandleMovement()
    {
        // Horizontal movement
        Vector2 velocity = rb.velocity;
        velocity.x = horizontalInput * moveSpeed;
        rb.velocity = velocity;
    }

    private void JumpAction()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = true;
        animator.SetTrigger(Jump);
    }

    private void HandleFlip()
    {
        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
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
        if (Mathf.Abs(horizontalInput) > Mathf.Epsilon)
        {
            animator.SetInteger(AnimState, 1);
        }
        else
        {
            animator.SetInteger(AnimState, 0);
        }

        animator.SetBool(Grounded, groundSensor.IsGrounded);
        animator.SetFloat(AirSpeedY, rb.velocity.y);
    }

    #region Event Handlers

    private void OnGrounded()
    {
        isJumping = false;
    }

    private void OnLeftGround()
    {
        // Optional: Add any logic when leaving ground
    }

    private void OnDamageTaken(float damage)
    {
        animator.SetTrigger(Hurt);
    }

    private void OnDeath()
    {
        enabled = false;
        rb.velocity = Vector2.zero;

        animator.SetTrigger("Death");

        Debug.Log("Player died!");
    }

    #endregion

    private void OnDestroy()
    {
        if (groundSensor != null)
        {
            groundSensor.OnGrounded -= OnGrounded;
            groundSensor.OnLeftGround -= OnLeftGround;
        }

        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= OnDamageTaken;
            healthSystem.OnDeath -= OnDeath;
        }
    }
}