using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private bool isContinuousDamage = false;

    [Header("Spike Trap Settings")]
    [SerializeField] private bool isSpikeTrap = false;
    [SerializeField] private float spikeActivationDelay = 0.5f;
    [SerializeField] private float spikeActiveDuration = 1f;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip trapDamageSound;
    [SerializeField] private AudioClip trapActivateSound; 
    [SerializeField] private float trapVolume = 0.7f;
    [SerializeField] private AudioSource audioSource;

    private bool isSpikeActive = false;
    private float lastDamageTime = 0f;

    private static readonly int Activate = Animator.StringToHash("Activate");

    private void Start()
    {
        InitializeAudioSource();

        if (isSpikeTrap)
        {
            InvokeRepeating("ToggleSpikeTrap", spikeActivationDelay, spikeActivationDelay + spikeActiveDuration);
        }
    }

    private void InitializeAudioSource()
    {
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerContact(other.GetComponent<HealthSystem>());
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isContinuousDamage && other.CompareTag("Player"))
        {
            HandlePlayerContact(other.GetComponent<HealthSystem>());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerContact(collision.gameObject.GetComponent<HealthSystem>());
        }
    }

    private void HandlePlayerContact(HealthSystem playerHealth)
    {
        if (playerHealth == null) return;

        if (Time.time - lastDamageTime < damageCooldown) return;

        if (isSpikeTrap && !isSpikeActive) return;

        playerHealth.TakeDamage(damage);
        lastDamageTime = Time.time;

        PlayDamageSound();

        Debug.Log($"Trap dealt {damage} damage to player");
    }

    private void PlayDamageSound()
    {
        if (audioSource != null && trapDamageSound != null)
        {
            audioSource.PlayOneShot(trapDamageSound, trapVolume);
        }
    }

    private void ToggleSpikeTrap()
    {
        if (!isSpikeActive)
        {
            ActivateSpikeTrap();
        }
        else
        {
            DeactivateSpikeTrap();
        }
    }

    private void ActivateSpikeTrap()
    {
        isSpikeActive = true;

        if (isSpikeTrap && trapActivateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(trapActivateSound, trapVolume);
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger(Activate);
        }

        Invoke("DeactivateSpikeTrap", spikeActiveDuration);
    }

    private void DeactivateSpikeTrap()
    {
        isSpikeActive = false;
    }

    public void OnSpikeActivate()
    {
        isSpikeActive = true;

        if (isSpikeTrap && trapActivateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(trapActivateSound, trapVolume);
        }
    }

    public void OnSpikeDeactivate()
    {
        isSpikeActive = false;
    }

    #region Public Methods for Configuration
    public void SetDamageSound(AudioClip clip, float volume = 0.7f)
    {
        trapDamageSound = clip;
        trapVolume = Mathf.Clamp01(volume);
    }

    public void SetActivateSound(AudioClip clip, float volume = 0.7f)
    {
        trapActivateSound = clip;
        trapVolume = Mathf.Clamp01(volume);
    }

    public void SetTrapDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetDamageCooldown(float cooldown)
    {
        damageCooldown = cooldown;
    }
    #endregion
}