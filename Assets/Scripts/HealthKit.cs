using UnityEngine;

public class HealthKit : InteractableBase
{
    [Header("Health Kit Settings")]
    [SerializeField] private float healAmount = 30f;
    [SerializeField] private bool isConsumable = true;
    [SerializeField] private bool isUsed = false;

    [Header("Respawn Settings")]
    [SerializeField] private bool canRespawn = false;
    [SerializeField] private float respawnTime = 10f;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip healSound;
    [SerializeField] private float healVolume = 0.7f;
    [SerializeField] private AudioSource audioSource;

    private Collider2D healthKitCollider;
    private SpriteRenderer spriteRenderer;

    public override bool CanInteract => !isUsed && player != null;
    public override string InteractionText => isUsed ? "Already used" : $"Press {interactionKey} to heal";

    protected override void Start()
    {
        base.Start();
        InitializeComponents();
        InitializeAudioSource();
    }

    private void InitializeComponents()
    {
        healthKitCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    public override void Interact()
    {
        if (isUsed) return;

        PlayHealSound();
        HealPlayer();
        MarkAsUsed();

        if (canRespawn)
        {
            ScheduleRespawn();
        }

        Debug.Log($"Player healed for {healAmount} health");
    }

    private void PlayHealSound()
    {
        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound, healVolume);
        }
    }

    private void HealPlayer()
    {
        HealthSystem playerHealth = player.GetComponent<HealthSystem>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
        }
    }

    private void MarkAsUsed()
    {
        isUsed = true;

        if (healthKitCollider != null)
        {
            healthKitCollider.enabled = false;
        }

        HidePrompt();
    }

    private void ScheduleRespawn()
    {
        Invoke("Respawn", respawnTime);
    }

    private void Respawn()
    {
        isUsed = false;


        if (healthKitCollider != null)
        {
            healthKitCollider.enabled = true;
        }

        if (showPrompt)
        {
            ShowPrompt();
        }

        Debug.Log("Health kit respawned");
    }

    protected override void OnPlayerEnterRange()
    {
        if (!isUsed && showPrompt)
        {
            ShowPrompt();
        }
    }

    protected override void OnPlayerExitRange()
    {
        if (showPrompt)
        {
            HidePrompt();
        }
    }

    #region Public Methods for Configuration
    public void SetHealSound(AudioClip clip, float volume = 0.7f)
    {
        healSound = clip;
        healVolume = Mathf.Clamp01(volume);
    }

    public void ResetHealthKit()
    {
        isUsed = false;
        CancelInvoke("Respawn");

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        if (healthKitCollider != null)
        {
            healthKitCollider.enabled = true;
        }
    }
    #endregion
}