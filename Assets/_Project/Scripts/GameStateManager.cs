using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Gameplay/Game State Manager")]
public class GameStateManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private bool pauseOnEscape = true;
    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Level Management")]
    [SerializeField] private string nextLevelSceneName;
    [SerializeField] private bool autoProgressToNextLevel = false;

    [Header("Performance")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool enableVSync = true;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        LevelComplete,
        Loading
    }

    private GameState _currentState = GameState.Playing;
    private GameObject _pauseMenuInstance;
    private float _timeScaleBeforePause = 1f;

    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState => _currentState;

    public static event System.Action<GameState> OnGameStateChanged;

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

        SetupPerformance();
    }

    private void Start()
    {
        SetGameState(GameState.Playing);
    }

    private void Update()
    {
        HandleInput();
    }

    private void SetupPerformance()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
    }

    private void HandleInput()
    {
        if (pauseOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (_currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        // Debug controls
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
        {
            RestartLevel();
        }

        if (Input.GetKeyDown(KeyCode.N) && Input.GetKey(KeyCode.LeftControl))
        {
            LoadNextLevel();
        }
    }

    public void SetGameState(GameState newState)
    {
        if (_currentState == newState) return;

        GameState previousState = _currentState;
        _currentState = newState;

        HandleStateTransition(previousState, newState);
        OnGameStateChanged?.Invoke(newState);

        Debug.Log($"Game state changed: {previousState} → {newState}");
    }

    private void HandleStateTransition(GameState from, GameState to)
    {
        switch (to)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Paused:
                _timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ShowPauseMenu();
                break;

            case GameState.GameOver:
                Time.timeScale = 0.1f; // Slow motion effect
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.LevelComplete:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (autoProgressToNextLevel && !string.IsNullOrEmpty(nextLevelSceneName))
                {
                    Invoke(nameof(LoadNextLevel), 2f);
                }
                break;

            case GameState.Loading:
                Time.timeScale = 1f;
                break;
        }
    }

    public void PauseGame()
    {
        SetGameState(GameState.Paused);
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
        HidePauseMenu();
    }

    public void RestartLevel()
    {
        SetGameState(GameState.Loading);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SetGameState(GameState.Loading);
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.LogWarning("Next level scene name not set!");
        }
    }

    public void LoadMainMenu()
    {
        SetGameState(GameState.Loading);
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowPauseMenu()
    {
        if (pauseMenuPrefab != null && _pauseMenuInstance == null)
        {
            _pauseMenuInstance = Instantiate(pauseMenuPrefab);
        }
    }

    private void HidePauseMenu()
    {
        if (_pauseMenuInstance != null)
        {
            Destroy(_pauseMenuInstance);
            _pauseMenuInstance = null;
        }
    }

    public void OnLevelComplete()
    {
        SetGameState(GameState.LevelComplete);
        AudioFXManager.PlayLevelCompleteSound();
    }

    public void OnPlayerDeath()
    {
        // Could trigger game over state or just handle respawn
        if (FindFirstObjectByType<Checkpoint>() == null)
        {
            SetGameState(GameState.GameOver);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f; // Ensure time scale is restored
        }
    }
}