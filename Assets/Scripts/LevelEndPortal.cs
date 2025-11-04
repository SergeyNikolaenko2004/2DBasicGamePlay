using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private float transitionDelay = 3f;

    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMPro.TextMeshProUGUI victoryText;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip portalActivateSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private float soundVolume = 0.7f;

    private bool isActivated = false;
    private Animator animator;

    private static readonly int Activate = Animator.StringToHash("Activate");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            ActivatePortal();
        }
    }

    private void ActivatePortal()
    {
        isActivated = true;

        if (animator != null)
        {
            animator.SetTrigger(Activate);
        }

        PlayPortalSound();

        DisablePlayerControl();

        ShowVictoryPanel();

        StartCoroutine(TransitionToMainMenu());

        Debug.Log("Level completed! Portal activated.");
    }

    private void PlayPortalSound()
    {
        if (portalActivateSound != null)
        {
            GameManager.Instance?.PlaySound(portalActivateSound, soundVolume);
        }
    }

    private void PlayVictorySound()
    {
        if (victorySound != null)
        {
            GameManager.Instance?.PlaySound(victorySound, soundVolume);
        }
    }

    private void DisablePlayerControl()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;

            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryText != null)
            {
                victoryText.text = "Тестовый уровень пройден!\nСпасибо за игру!\nПереход в главное меню...";
            }
        }

        PlayVictorySound();
    }

    private IEnumerator TransitionToMainMenu()
    {
        yield return new WaitForSeconds(transitionDelay);

        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


    #region Public Methods for Configuration
    public void SetTransitionDelay(float delay)
    {
        transitionDelay = delay;
    }

    public void SetVictoryText(string text)
    {
        if (victoryText != null)
        {
            victoryText.text = text;
        }
    }
    #endregion
}