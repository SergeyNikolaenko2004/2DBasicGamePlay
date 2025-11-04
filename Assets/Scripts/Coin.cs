using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int value = 1;
    [SerializeField] private float collectDelay = 0.2f;

    private bool isCollected = false;
    private Collider2D coinCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        coinCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        coinCollider.enabled = false;
        spriteRenderer.enabled = false;

        GameManager.Instance?.AddCoins(value);

        Destroy(gameObject, collectDelay);
    }

    #region Public Methods for Configuration
    public void SetValue(int coinValue)
    {
        value = coinValue;
    }
    #endregion
}