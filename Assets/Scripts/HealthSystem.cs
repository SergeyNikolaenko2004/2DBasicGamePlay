using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private bool isInvincible = false;

    [Header("Block Settings")]
    [SerializeField] private bool isBlocking = false;

    [Header("UI References")]
    [SerializeField] private Image currentHealthBar;
    [SerializeField] private Image currentHealthGlobe;
    [SerializeField] private Text healthText;

    public event System.Action<float> OnDamageTaken;
    public event System.Action OnDeath;
    public event System.Action OnInvincibilityStart;
    public event System.Action OnInvincibilityEnd;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsInvincible => isInvincible;
    public bool IsDead => isDead;
    public bool IsBlocking => isBlocking;

    private bool isDead = false;
    private float invincibilityTimer = 0f;

    private void Start()
    {
        InitializeHealth();

    }

    private void Update()
    {
        UpdateInvincibility();
    }

    private void InitializeHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateGraphics();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible || currentHealth <= 0) return;

        // Проверяем блок - если блокируем, не получаем урон
        if (isBlocking)
        {
            Debug.Log("Damage blocked!");
            return;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateGraphics();

        OnDamageTaken?.Invoke(damage);

        // Активируем неуязвимость после получения урона
        if (damage > 0)
        {
            StartInvincibility();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    #region Block System
    public void StartBlock()
    {
        if (isDead) return;

        isBlocking = true;
        Debug.Log("Block started");
    }

    public void EndBlock()
    {
        isBlocking = false;
        Debug.Log("Block ended");
    }
    #endregion

    #region Invincibility System
    private void StartInvincibility()
    {
        if (invincibilityDuration <= 0) return;

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        OnInvincibilityStart?.Invoke();

    }


    private void UpdateInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        if (invincibilityTimer <= 0)
        {
            EndInvincibility();
        }
    }

    private void EndInvincibility()
    {
        isInvincible = false;

        OnInvincibilityEnd?.Invoke();
    }

    public void SetInvincible(bool invincible, float duration = 0f)
    {
        if (invincible)
        {
            if (duration > 0) invincibilityDuration = duration;
            StartInvincibility();
        }
        else
        {
            EndInvincibility();
        }
    }
    #endregion

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{name} уничтожен!");

        EndInvincibility();
        EndBlock();

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