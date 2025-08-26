using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WWIII.Core;

namespace WWIII.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Pause Menu References")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Canvas pauseCanvas;
        [SerializeField] private CanvasGroup pauseCanvasGroup;
        
        [Header("Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Toggle hapticToggle;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip pauseSound;
        [SerializeField] private AudioClip resumeSound;
        
        public static PauseMenuManager Instance { get; private set; }
        
        private bool isPaused = false;
        private AudioSource audioSource;
        private GameManager gameManager;
        private SceneTransitionManager sceneManager;
        
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializePauseMenu();
        }
        
        private void Start()
        {
            SetupPauseMenu();
            RegisterInputEvents();
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void InitializePauseMenu()
        {
            // Get component references
            gameManager = GameManager.Instance;
            sceneManager = SceneTransitionManager.Instance;
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            
            // Setup canvas
            if (pauseCanvas == null)
                pauseCanvas = GetComponent<Canvas>();
            
            if (pauseCanvas != null)
            {
                pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                pauseCanvas.sortingOrder = 200; // Higher than HUD
            }
            
            // Setup canvas group for fading
            if (pauseCanvasGroup == null)
                pauseCanvasGroup = pauseCanvas.GetComponent<CanvasGroup>();
            
            if (pauseCanvasGroup == null)
                pauseCanvasGroup = pauseCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        private void SetupPauseMenu()
        {
            // Initially hide pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            
            // Setup button listeners
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
            
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartLevel);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettings);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
            
            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(HideSettings);
            
            // Setup settings controls
            SetupSettingsControls();
            
            Debug.Log("Pause menu initialized");
        }
        
        private void SetupSettingsControls()
        {
            // Volume slider
            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            
            // SFX slider (if you have an SFX manager)
            if (sfxSlider != null)
            {
                sfxSlider.value = 1f; // Default value
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            // Fullscreen toggle
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            }
            
            // VSync toggle
            if (vsyncToggle != null)
            {
                vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
                vsyncToggle.onValueChanged.AddListener(OnVSyncToggled);
            }
            
            // Haptic toggle
            if (hapticToggle != null)
            {
                bool hapticEnabled = MobileOptimizer.Instance?.IsHapticEnabled ?? true;
                hapticToggle.isOn = hapticEnabled;
                hapticToggle.onValueChanged.AddListener(OnHapticToggled);
            }
        }
        
        private void RegisterInputEvents()
        {
            // Register for escape key / back button
            var inputManager = InputManager.Instance;
            if (inputManager != null)
            {
                inputManager.OnPausePressed += TogglePauseMenu;
            }
        }
        
        private void HandleInput()
        {
            // Handle Android back button
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsPanel != null && settingsPanel.activeInHierarchy)
                {
                    HideSettings();
                }
                else if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    ShowPauseMenu();
                }
            }
        }
        
        public void TogglePauseMenu()
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                ShowPauseMenu();
            }
        }
        
        public void ShowPauseMenu()
        {
            if (isPaused) return;
            
            isPaused = true;
            
            // Pause the game
            Time.timeScale = 0f;
            AudioListener.pause = true;
            
            // Show pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
            
            // Play pause sound
            PlaySound(pauseSound);
            
            // Animate in
            StartCoroutine(FadeIn());
            
            // Trigger haptic feedback
            if (MobileOptimizer.Instance != null)
            {
                MobileOptimizer.Instance.TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
            
            // Hide HUD
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.HideHUD();
            }
            
            // Notify listeners
            OnGamePaused?.Invoke();
            
            Debug.Log("Game paused");
        }
        
        public void ResumeGame()
        {
            if (!isPaused) return;
            
            StartCoroutine(ResumeGameCoroutine());
        }
        
        private System.Collections.IEnumerator ResumeGameCoroutine()
        {
            // Hide settings if open
            if (settingsPanel != null && settingsPanel.activeInHierarchy)
            {
                settingsPanel.SetActive(false);
            }
            
            // Play resume sound
            PlaySound(resumeSound);
            
            // Animate out
            yield return StartCoroutine(FadeOut());
            
            // Hide pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            
            // Resume the game
            isPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            
            // Show HUD
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowHUD();
            }
            
            // Notify listeners
            OnGameResumed?.Invoke();
            
            Debug.Log("Game resumed");
        }
        
        private void RestartLevel()
        {
            PlayButtonSound();
            
            // Resume time before scene transition
            Time.timeScale = 1f;
            AudioListener.pause = false;
            
            // Restart current scene
            if (sceneManager != null)
            {
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                sceneManager.LoadScene(currentScene);
            }
            else
            {
                // Fallback: reload scene directly
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );
            }
        }
        
        private void ShowSettings()
        {
            PlayButtonSound();
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }
        
        private void HideSettings()
        {
            PlayButtonSound();
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
        
        private void GoToMainMenu()
        {
            PlayButtonSound();
            
            // Resume time before scene transition
            Time.timeScale = 1f;
            AudioListener.pause = false;
            
            // Load main menu
            if (sceneManager != null)
            {
                sceneManager.LoadScene("MainMenu");
            }
            else
            {
                // Fallback
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
        
        private void QuitGame()
        {
            PlayButtonSound();
            
            Debug.Log("Quitting game...");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        // Settings Event Handlers
        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            
            // Save preference
            PlayerPrefs.SetFloat("MasterVolume", value);
            PlayerPrefs.Save();
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            // Implement SFX volume control if you have an audio manager
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        }
        
        private void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void OnVSyncToggled(bool vsyncEnabled)
        {
            QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
            
            PlayerPrefs.SetInt("VSync", vsyncEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void OnHapticToggled(bool hapticEnabled)
        {
            if (MobileOptimizer.Instance != null)
            {
                MobileOptimizer.Instance.SetHapticEnabled(hapticEnabled);
            }
            
            PlayerPrefs.SetInt("HapticFeedback", hapticEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        // Animation Methods
        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeInDuration;
                float curveValue = fadeInCurve.Evaluate(t);
                
                if (pauseCanvasGroup != null)
                {
                    pauseCanvasGroup.alpha = curveValue;
                }
                
                yield return null;
            }
            
            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = 1f;
            }
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeOutDuration;
                float curveValue = fadeOutCurve.Evaluate(t);
                
                if (pauseCanvasGroup != null)
                {
                    pauseCanvasGroup.alpha = curveValue;
                }
                
                yield return null;
            }
            
            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = 0f;
            }
        }
        
        // Audio Methods
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void PlayButtonSound()
        {
            PlaySound(buttonClickSound);
        }
        
        // Public Properties
        public bool IsPaused => isPaused;
        
        // Public Methods for external control
        public void SetVolumeSlider(float value)
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = value;
            }
        }
        
        public void LoadSettings()
        {
            // Load saved preferences
            if (volumeSlider != null)
            {
                float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                volumeSlider.value = volume;
                AudioListener.volume = volume;
            }
            
            if (sfxSlider != null)
            {
                float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxSlider.value = sfxVolume;
            }
            
            if (fullscreenToggle != null)
            {
                bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
                fullscreenToggle.isOn = fullscreen;
                Screen.fullScreen = fullscreen;
            }
            
            if (vsyncToggle != null)
            {
                bool vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
                vsyncToggle.isOn = vsync;
                QualitySettings.vSyncCount = vsync ? 1 : 0;
            }
            
            if (hapticToggle != null)
            {
                bool haptic = PlayerPrefs.GetInt("HapticFeedback", 1) == 1;
                hapticToggle.isOn = haptic;
                if (MobileOptimizer.Instance != null)
                {
                    MobileOptimizer.Instance.SetHapticEnabled(haptic);
                }
            }
        }
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(ResumeGame);
            
            if (restartButton != null)
                restartButton.onClick.RemoveListener(RestartLevel);
            
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(ShowSettings);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            
            if (quitButton != null)
                quitButton.onClick.RemoveListener(QuitGame);
            
            if (settingsBackButton != null)
                settingsBackButton.onClick.RemoveListener(HideSettings);
            
            // Clean up input events
            var inputManager = InputManager.Instance;
            if (inputManager != null)
            {
                inputManager.OnPausePressed -= TogglePauseMenu;
            }
            
            // Resume game if paused when destroyed
            if (isPaused)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}