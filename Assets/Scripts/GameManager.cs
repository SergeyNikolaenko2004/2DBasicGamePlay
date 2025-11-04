using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Coin System")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private string coinsPrefix = "Coins: ";

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private UnityEngine.UI.Button restartButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;

    [Header("Game State")]
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private bool isGameOver = false;

    private int initialCoins = 0;
    private string currentSceneName;

    public int CurrentCoins => currentCoins;
    public bool IsGamePaused => isGamePaused;
    public bool IsGameOver => isGameOver;

    // События
    public event System.Action<int> OnCoinsChanged;
    public event System.Action OnGamePaused;
    public event System.Action OnGameResumed;
    public event System.Action OnGameOver;
    public event System.Action OnGameRestart;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        initialCoins = currentCoins;

        UpdateCoinsUI();
        HideAllPanels();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        HandlePauseInput();
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }
    }

    #region Coin System
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        currentCoins += amount;
        UpdateCoinsUI();

        OnCoinsChanged?.Invoke(currentCoins);
        Debug.Log($"Coins collected: {amount}. Total: {currentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || currentCoins < amount) return false;

        currentCoins -= amount;
        UpdateCoinsUI();

        OnCoinsChanged?.Invoke(currentCoins);
        return true;
    }

    private void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = coinsPrefix + currentCoins.ToString();
        }
    }
    #endregion

    #region Game State Management
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        ShowGameOverPanel();
        OnGameOver?.Invoke();

        Debug.Log("Game Over!");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isGamePaused = false;

        HideAllPanels();
        OnGameRestart?.Invoke();

        SceneManager.LoadScene(currentSceneName);
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }
        else
        {
            Time.timeScale = 1f;
            HideAllPanels();
            OnGameResumed?.Invoke();
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    #endregion

    #region UI Management
    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    #endregion

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        HideAllPanels();
        UpdateCoinsUI();

        FindUIReferences();

        Debug.Log($"Scene loaded: {scene.name}");
    }

    private void FindUIReferences()
    {
        if (coinsText == null)
        {
            GameObject coinsTextObj = GameObject.FindGameObjectWithTag("CoinsText");
            if (coinsTextObj != null) coinsText = coinsTextObj.GetComponent<TMP_Text>();
        }

        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.FindGameObjectWithTag("GameOverPanel");
        }
    }
    #endregion

    #region Public Methods
    public void ResetCoins()
    {
        currentCoins = initialCoins;
        UpdateCoinsUI();
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public void SetInitialCoins(int coins)
    {
        initialCoins = coins;
        currentCoins = coins;
        UpdateCoinsUI();
    }
    #endregion

    private void OnDestroy()
    {
        // Отписываемся от событий
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}