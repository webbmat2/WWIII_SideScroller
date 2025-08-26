using UnityEngine;
using UnityEngine.SceneManagement;

namespace WWIII.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private bool enableVSync = false;
        [SerializeField] private bool neverSleep = true;
        
        [Header("Performance Settings")]
        [SerializeField] private float fixedTimestep = 0.016666f; // 60 FPS
        [SerializeField] private int maximumParticleSystems = 100;
        
        public static GameManager Instance { get; private set; }
        
        public bool IsPaused { get; private set; }
        public float TimeScale { get; private set; } = 1f;
        
        public System.Action<bool> OnPauseStateChanged;
        public System.Action<string> OnSceneTransition;
        
        private void Awake()
        {
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
        
        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPausePressed += TogglePause;
            }
        }
        
        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPausePressed -= TogglePause;
            }
        }
        
        private void InitializeGame()
        {
            SetupPerformanceSettings();
            SetupQualitySettings();
            
            Debug.Log("GameManager initialized for WWIII SideScroller");
        }
        
        private void SetupPerformanceSettings()
        {
            Application.targetFrameRate = (int)targetFrameRate;
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            
            Time.fixedDeltaTime = fixedTimestep;
            
            QualitySettings.maxQueuedFrames = 2;
        }
        
        private void SetupQualitySettings()
        {
            QualitySettings.particleRaycastBudget = maximumParticleSystems;
            QualitySettings.shadowDistance = 20f;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.antiAliasing = 2;
        }
        
        public void TogglePause()
        {
            SetPause(!IsPaused);
        }
        
        public void SetPause(bool paused)
        {
            IsPaused = paused;
            TimeScale = paused ? 0f : 1f;
            Time.timeScale = TimeScale;
            
            OnPauseStateChanged?.Invoke(IsPaused);
            
            Debug.Log($"Game {(paused ? "Paused" : "Resumed")}");
        }
        
        public void LoadScene(string sceneName)
        {
            OnSceneTransition?.Invoke(sceneName);
            SceneManager.LoadScene(sceneName);
        }
        
        public void LoadSceneAsync(string sceneName)
        {
            OnSceneTransition?.Invoke(sceneName);
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
        }
        
        private System.Collections.IEnumerator LoadSceneAsyncCoroutine(string sceneName)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
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
        
        public void RestartCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }
        
        public void SetTimeScale(float scale)
        {
            if (!IsPaused)
            {
                TimeScale = scale;
                Time.timeScale = TimeScale;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SetPause(true);
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SetPause(true);
            }
        }
    }
}