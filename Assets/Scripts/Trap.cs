using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private bool isContinuousDamage = false;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private Animator animator;

    [Header("Spike Trap Settings")]
    [SerializeField] private bool isSpikeTrap = false;
    [SerializeField] private float spikeActivationDelay = 0.5f;
    [SerializeField] private float spikeActiveDuration = 1f;

    private bool isSpikeActive = false;
    private float lastDamageTime = 0f;

    // Animation parameters
    private static readonly int Activate = Animator.StringToHash("Activate");

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // Для шипованных ловушек запускаем цикл активации
        if (isSpikeTrap)
        {
            InvokeRepeating("ToggleSpikeTrap", spikeActivationDelay, spikeActivationDelay + spikeActiveDuration);
        }
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
        // Для непрерывного урона (например, огонь)
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

        // Проверяем кд урона
        if (Time.time - lastDamageTime < damageCooldown) return;

        // Для шипованных ловушек проверяем активность
        if (isSpikeTrap && !isSpikeActive) return;

        // Наносим урон
        playerHealth.TakeDamage(damage);
        lastDamageTime = Time.time;

        // Визуальные эффекты
        PlayDamageEffects();

        Debug.Log($"Trap dealt {damage} damage to player");
    }

    private void ToggleSpikeTrap()
    {
        isSpikeActive = !isSpikeActive;

        if (animator != null)
        {
            animator.SetBool(Activate, isSpikeActive);
        }

        // Можно добавить звук активации/деактивации
    }

    private void PlayDamageEffects()
    {
        // Партиклы
        if (damageEffect != null)
        {
            ParticleSystem effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }

        // Звук
        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
        }
    }

    // Для анимационных событий
    public void OnSpikeActivate()
    {
        isSpikeActive = true;
    }

    public void OnSpikeDeactivate()
    {
        isSpikeActive = false;
    }
}