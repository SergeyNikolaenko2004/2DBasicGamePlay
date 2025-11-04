using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Block Settings")]
    [SerializeField] private KeyCode blockKey = KeyCode.J;

    [Header("Components")]
    [SerializeField] private GroundSensor groundSensor;
    [SerializeField] private Transform graphics;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip blockStartSound;
    [SerializeField] private AudioClip runningSound; 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource movementAudioSource; 

    [Header("Sound Volumes")]
    [SerializeField] private float jumpVolume = 0.7f;
    [SerializeField] private float landVolume = 0.5f;
    [SerializeField] private float blockVolume = 0.6f;
    [SerializeField] private float runningVolume = 0.4f;

    private Rigidbody2D rb;
    private Animator animator;
    private HealthSystem healthSystem;
    private PlayerCombat combat;

    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isJumping;
    private bool wasGrounded;
    private bool isRunningSoundPlaying = false;


    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int AirSpeedY = Animator.StringToHash("AirSpeedY");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Block = Animator.StringToHash("Block");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (movementAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length > 1)
            {
                movementAudioSource = audioSources[1];
            }
            else
            {
                movementAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        movementAudioSource.spatialBlend = 0f;
        movementAudioSource.playOnAwake = false;
        movementAudioSource.loop = true;
        movementAudioSource.volume = runningVolume;
    }

    private void Start()
    {
        combat = GetComponent<PlayerCombat>();

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

        wasGrounded = groundSensor.IsGrounded;
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimations();
        HandleFlip();
        HandleRunningSound(); 
        CheckLandingSound();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && groundSensor.IsGrounded && !healthSystem.IsBlocking)
        {
            JumpAction();
        }

        if (Input.GetKeyDown(blockKey) && !healthSystem.IsBlocking)
        {
            StartBlock();
        }

        if (Input.GetKeyUp(blockKey) && healthSystem.IsBlocking)
        {
            EndBlock();
        }
    }

    private void StartBlock()
    {
        healthSystem.StartBlock();
        animator.SetTrigger(Block);
        PlayBlockStartSound();
        StopRunningSound(); 
    }

    private void EndBlock()
    {
        healthSystem.EndBlock();
    }

    private void HandleMovement()
    {
        float currentMoveSpeed = healthSystem.IsBlocking ? moveSpeed * 0.5f : moveSpeed;

        Vector2 velocity = rb.velocity;
        velocity.x = horizontalInput * currentMoveSpeed;
        rb.velocity = velocity;
    }

    private void JumpAction()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = true;
        animator.SetTrigger(Jump);
        PlayJumpSound();
        StopRunningSound(); 
    }

    private void HandleFlip()
    {
        if (healthSystem.IsBlocking) return;

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
        if (Mathf.Abs(horizontalInput) > Mathf.Epsilon && !healthSystem.IsBlocking)
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

    #region Sound Methods

    private void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    private void PlayLandSound()
    {
        if (audioSource != null && landSound != null)
        {
            audioSource.PlayOneShot(landSound, landVolume);
        }
    }

    private void PlayBlockStartSound()
    {
        if (audioSource != null && blockStartSound != null)
        {
            audioSource.PlayOneShot(blockStartSound, blockVolume);
        }
    }

    private void HandleRunningSound()
    {
        bool shouldPlayRunningSound = ShouldPlayRunningSound();

        if (shouldPlayRunningSound && !isRunningSoundPlaying)
        {
            StartRunningSound();
        }
        else if (!shouldPlayRunningSound && isRunningSoundPlaying)
        {
            StopRunningSound();
        }
    }

    private bool ShouldPlayRunningSound()
    {
        return groundSensor.IsGrounded &&
               Mathf.Abs(horizontalInput) > Mathf.Epsilon &&
               !healthSystem.IsBlocking &&
               !isJumping;
    }

    private void StartRunningSound()
    {
        if (movementAudioSource != null && runningSound != null && !isRunningSoundPlaying)
        {
            movementAudioSource.clip = runningSound;
            movementAudioSource.volume = runningVolume;
            movementAudioSource.Play();
            isRunningSoundPlaying = true;
            Debug.Log("Running sound started");
        }
    }

    private void StopRunningSound()
    {
        if (movementAudioSource != null && isRunningSoundPlaying)
        {
            movementAudioSource.Stop();
            isRunningSoundPlaying = false;
            Debug.Log("Running sound stopped");
        }
    }

    private void CheckLandingSound()
    {
        if (!wasGrounded && groundSensor.IsGrounded)
        {
            if (!isJumping)
            {
                PlayLandSound();
            }
        }

        wasGrounded = groundSensor.IsGrounded;

        if (groundSensor.IsGrounded)
        {
            isJumping = false;
        }
    }

    #endregion

    #region Event Handlers

    private void OnGrounded()
    {
        isJumping = false;
    }

    private void OnLeftGround()
    {
        StopRunningSound();
    }

    private void OnDamageTaken(float damage)
    {
        animator.SetTrigger(Hurt);
        StopRunningSound();
    }

    private void OnDeath()
    {
        enabled = false;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Death");
        StopRunningSound(); 
        Debug.Log("Player died!");
    }

    #endregion

    #region Public Methods for Sound Configuration

    public void SetJumpSound(AudioClip clip, float volume = 0.7f)
    {
        jumpSound = clip;
        jumpVolume = Mathf.Clamp01(volume);
    }

    public void SetLandSound(AudioClip clip, float volume = 0.5f)
    {
        landSound = clip;
        landVolume = Mathf.Clamp01(volume);
    }

    public void SetBlockSound(AudioClip clip, float volume = 0.6f)
    {
        blockStartSound = clip;
        blockVolume = Mathf.Clamp01(volume);
    }

    public void SetRunningSound(AudioClip clip, float volume = 0.4f)
    {
        runningSound = clip;
        runningVolume = Mathf.Clamp01(volume);

        // Обновляем громкость если звук уже играет
        if (movementAudioSource != null)
        {
            movementAudioSource.volume = runningVolume;
        }
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