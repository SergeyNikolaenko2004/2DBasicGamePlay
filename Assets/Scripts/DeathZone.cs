using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Death Zone Settings")]
    [SerializeField] private bool instantDeath = true;
    [SerializeField] private float damage = 1000f; 

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private AudioClip deathSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerFall(other.GetComponent<HealthSystem>());
        }
        else if (other.CompareTag("Enemy"))
        {
            HandleEnemyFall(other.GetComponent<HealthSystem>());
        }
    }

    private void HandlePlayerFall(HealthSystem playerHealth)
    {
        if (playerHealth == null) return;

        if (instantDeath)
        {
            // Мгновенная смерть
            playerHealth.TakeDamage(playerHealth.MaxHealth);
        }
        else
        {
            // Большой урон
            playerHealth.TakeDamage(damage);
        }

        PlayDeathEffects();
        Debug.Log("Player fell into death zone!");
    }

    private void HandleEnemyFall(HealthSystem enemyHealth)
    {
        if (enemyHealth == null) return;

        enemyHealth.TakeDamage(enemyHealth.MaxHealth);
        Debug.Log("Enemy fell into death zone!");
    }

    private void PlayDeathEffects()
    {
        if (deathEffect != null)
        {
            ParticleSystem effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
    }

    // Для визуализации в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}