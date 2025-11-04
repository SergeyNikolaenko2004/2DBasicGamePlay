using UnityEngine;

public class TreasureChest : InteractableBase
{
    [Header("Chest Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinsToSpawn = 5;
    [SerializeField] private float spawnRadius = 1.5f;
    [SerializeField] private bool isOpened = false;

    public override bool CanInteract => !isOpened && player != null;
    public override string InteractionText => interactionText;

    protected override void Start()
    {
        base.Start();

    }

    public override void Interact()
    {
        if (isOpened) return;

        OpenChest();
        SpawnCoins();

        // Скрываем подсказку после взаимодействия
        HidePrompt();

        Debug.Log("Chest opened! Coins spawned: " + coinsToSpawn);
    }

    private void OpenChest()
    {
        isOpened = true;

    }

    private void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("Coin prefab is not assigned to the chest!");
            return;
        }

        for (int i = 0; i < coinsToSpawn; i++)
        {
            Vector3 spawnPosition = CalculateSpawnPosition();
            Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private Vector3 CalculateSpawnPosition()
    {

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, Mathf.Abs(randomCircle.y), 0);

        return spawnPosition;
    }


    public void ResetChest()
    {
        isOpened = false;

        ShowPrompt();
    }
}