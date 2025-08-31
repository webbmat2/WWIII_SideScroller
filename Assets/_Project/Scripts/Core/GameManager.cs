using UnityEngine;
// using WWIII.Data; // Will be enabled once Data assembly is created

namespace WWIII.Core
{
    /// <summary>
    /// Core game state management following project rules
    /// Singleton pattern for global game state
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private bool isPaused = false;
        
        [Header("Level Management")]
        // [SerializeField] private LevelDef currentLevel; // Will be enabled once Data assembly is created
        [SerializeField] private int currentLevelIndex = 0;
        
        [Header("Mobile Optimization")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool adaptiveFrameRate = true;
        
        // Events
        public System.Action<GameState> OnGameStateChanged;
        // public System.Action<LevelDef> OnLevelLoaded; // Will be enabled once Data assembly is created
        public System.Action<bool> OnPauseStateChanged;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeGame()
        {
            // Mobile optimization per project rules
            Application.targetFrameRate = targetFrameRate;
            
            // Disable VSync for mobile battery optimization
            QualitySettings.vSyncCount = 0;
            
            Debug.Log($"🎮 GameManager initialized - Target FPS: {targetFrameRate}");
        }
        
        private void Update()
        {
            // Dynamic frame rate adjustment for mobile
            if (adaptiveFrameRate && Application.isMobilePlatform)
            {
                AdjustFrameRateForPerformance();
            }
        }
        
        public void ChangeGameState(GameState newState)
        {
            if (currentState != newState)
            {
                GameState previousState = currentState;
                currentState = newState;
                
                Debug.Log($"🔄 Game state changed: {previousState} → {newState}");
                OnGameStateChanged?.Invoke(newState);
                
                HandleStateTransition(previousState, newState);
            }
        }
        
        public void SetPauseState(bool paused)
        {
            if (isPaused != paused)
            {
                isPaused = paused;
                Time.timeScale = isPaused ? 0f : 1f;
                
                Debug.Log($"⏸️ Game {(isPaused ? "paused" : "resumed")}");
                OnPauseStateChanged?.Invoke(isPaused);
            }
        }
        
        /*
        public void LoadLevel(LevelDef levelDef)
        {
            if (levelDef != null)
            {
                currentLevel = levelDef;
                OnLevelLoaded?.Invoke(levelDef);
                
                Debug.Log($"📍 Loading level: {levelDef.levelName}");
                ChangeGameState(GameState.Playing);
            }
        }
        */
        
        private void HandleStateTransition(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Playing:
                    Time.timeScale = 1f;
                    isPaused = false;
                    break;
                    
                case GameState.Paused:
                    SetPauseState(true);
                    break;
                    
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }
        }
        
        private void AdjustFrameRateForPerformance()
        {
            // Simple adaptive frame rate for mobile battery optimization
            float averageFrameTime = Time.smoothDeltaTime;
            float currentFPS = 1f / averageFrameTime;
            
            if (currentFPS < targetFrameRate * 0.8f) // If dropping below 80% of target
            {
                Application.targetFrameRate = Mathf.Max(30, targetFrameRate - 10);
            }
            else if (currentFPS > targetFrameRate * 1.1f) // If well above target
            {
                Application.targetFrameRate = Mathf.Min(60, targetFrameRate + 5);
            }
        }
        
        // Accessors
        public GameState CurrentState => currentState;
        public bool IsPaused => isPaused;
        // public LevelDef CurrentLevel => currentLevel; // Will be enabled once Data assembly is created
        public int CurrentLevelIndex => currentLevelIndex;
    }
    
    public enum GameState
    {
        MainMenu,
        LevelSelect,
        Playing,
        Paused,
        GameOver,
        Victory
    }
}