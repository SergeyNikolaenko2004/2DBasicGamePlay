using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleMainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private AudioClip buttonClickSound;

    public void StartGame()
    {
        PlaySound();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenGitHub()
    {
        PlaySound();
        Application.OpenURL("https://github.com/SergeyNikolaenko2004/2DBasicGamePlay.git");
    }

    public void QuitGame()
    {
        PlaySound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void PlaySound()
    {
        if (buttonClickSound != null && GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(buttonClickSound, 0.7f);
        }
    }
}