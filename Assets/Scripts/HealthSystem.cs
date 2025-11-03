using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("UI References")]
    [SerializeField] private Image currentHealthBar;
    [SerializeField] private Image currentHealthGlobe;
    [SerializeField] private TMP_Text healthText;

    public event System.Action<float> OnDamageTaken;
    public event System.Action OnDeath;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => currentHealth / maxHealth;

    private bool isDead = false;

    private void Start()
    {
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateGraphics();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || currentHealth <= 0) return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateGraphics();

        OnDamageTaken?.Invoke(damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{name} уничтожен!");

        // Уведомляем GameManager о смерти игрока
        if (gameObject.CompareTag("Player"))
        {
            GameManager.Instance?.GameOver();
        }

        OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateGraphics();
    }

    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateGraphics();
    }

    private void UpdateHealthBar()
    {
        if (currentHealthBar != null)
        {
            float ratio = currentHealth / maxHealth;
            currentHealthBar.rectTransform.localPosition = new Vector3(
                currentHealthBar.rectTransform.rect.width * ratio - currentHealthBar.rectTransform.rect.width,
                0,
                0
            );
        }
    }

    private void UpdateHealthGlobe()
    {
        if (currentHealthGlobe != null)
        {
            float ratio = currentHealth / maxHealth;
            currentHealthGlobe.rectTransform.localPosition = new Vector3(
                0,
                currentHealthGlobe.rectTransform.rect.height * ratio - currentHealthGlobe.rectTransform.rect.height,
                0
            );
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString("0") + "/" + maxHealth.ToString("0");
        }
    }

    private void UpdateGraphics()
    {
        UpdateHealthBar();
        UpdateHealthGlobe();
        UpdateHealthText();
    }

    private void OnValidate()
    {
        if (currentHealthBar != null || currentHealthGlobe != null || healthText != null)
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateGraphics();
        }
    }
}