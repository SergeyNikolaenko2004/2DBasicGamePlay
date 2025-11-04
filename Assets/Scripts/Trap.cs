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

    private bool isSpikeActive = false;
    private float lastDamageTime = 0f;

    private static readonly int Activate = Animator.StringToHash("Activate");

    private void Start()
    {
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

        Debug.Log($"Trap dealt {damage} damage to player");
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