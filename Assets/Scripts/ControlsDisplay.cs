using UnityEngine;
using TMPro;
using System.Collections;

public class ControlsDisplay : MonoBehaviour
{
    [Header("Controls Display")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private TMP_Text controlsText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        ShowControls();
    }

    private void InitializeComponents()
    {
        if (controlsPanel != null)
        {
            canvasGroup = controlsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = controlsPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    public void ShowControls()
    {
        if (controlsPanel == null) return;

        controlsPanel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        StartCoroutine(HideAfterDelay());
    }

    public void HideControls()
    {
        if (controlsPanel == null) return;
        StartCoroutine(FadeOutControls());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        HideControls();
    }

    private IEnumerator FadeOutControls()
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        controlsPanel.SetActive(false);
    }

    public void SetControlsText(string text)
    {
        if (controlsText != null)
        {
            controlsText.text = text;
        }
    }
}