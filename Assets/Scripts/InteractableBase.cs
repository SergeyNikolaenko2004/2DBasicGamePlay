using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] protected float interactionRange = 2f;
    [SerializeField] protected string interactionText = "Press E to interact";
    [SerializeField] protected KeyCode interactionKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] protected GameObject interactionPrompt;
    [SerializeField] protected bool showPrompt = true;

    protected Transform player;
    protected bool isPlayerInRange = false;

    public abstract bool CanInteract { get; }
    public abstract string InteractionText { get; }

    protected virtual void Start()
    {
        FindPlayer();
        HidePrompt();
    }

    protected virtual void Update()
    {
        if (player == null) return;

        CheckPlayerDistance();
        HandleInteractionInput();
    }

    protected virtual void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    protected virtual void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;

        if (isPlayerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        else if (!isPlayerInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
    }

    protected virtual void HandleInteractionInput()
    {
        if (isPlayerInRange && CanInteract && Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    protected virtual void OnPlayerEnterRange()
    {
        if (showPrompt)
        {
            ShowPrompt();
        }
    }

    protected virtual void OnPlayerExitRange()
    {
        if (showPrompt)
        {
            HidePrompt();
        }
    }

    public virtual void ShowPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    public virtual void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public abstract void Interact();

    // Для визуализации радиуса взаимодействия в редакторе
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}