using UnityEngine;
using UnityEngine.UI;

namespace WWIII.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        
        [Header("Audio")]
        [SerializeField] private AudioSource menuAudioSource;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip backgroundMusic;
        
        private void Start()
        {
            InitializeMainMenu();
            SetupButtonListeners();
            PlayBackgroundMusic();
        }
        
        private void InitializeMainMenu()
        {
            ShowMainPanel();
            
            // Enable input for menu navigation
            if (WWIII.Core.InputManager.Instance != null)
            {
                WWIII.Core.InputManager.Instance.EnableInput();
            }
            
            Debug.Log("Main Menu initialized");
        }
        
        private void SetupButtonListeners()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
                
            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsButtonClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        
        public void OnPlayButtonClicked()
        {
            PlayButtonSound();
            
            // Navigate to level select
            if (WWIII.Core.SceneTransitionManager.Instance != null)
            {
                WWIII.Core.SceneTransitionManager.Instance.LoadLevelSelect();
            }
            else
            {
                Debug.LogWarning("SceneTransitionManager not found!");
            }
        }
        
        public void OnSettingsButtonClicked()
        {
            PlayButtonSound();
            ShowSettingsPanel();
        }
        
        public void OnCreditsButtonClicked()
        {
            PlayButtonSound();
            ShowCreditsPanel();
        }
        
        public void OnQuitButtonClicked()
        {
            PlayButtonSound();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        public void OnBackButtonClicked()
        {
            PlayButtonSound();
            ShowMainPanel();
        }
        
        private void ShowMainPanel()
        {
            SetPanelActive(mainPanel, true);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, false);
        }
        
        private void ShowSettingsPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(settingsPanel, true);
            SetPanelActive(creditsPanel, false);
        }
        
        private void ShowCreditsPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, true);
        }
        
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
        
        private void PlayButtonSound()
        {
            if (menuAudioSource != null && buttonClickSound != null)
            {
                menuAudioSource.PlayOneShot(buttonClickSound);
            }
        }
        
        private void PlayBackgroundMusic()
        {
            if (menuAudioSource != null && backgroundMusic != null)
            {
                menuAudioSource.clip = backgroundMusic;
                menuAudioSource.loop = true;
                menuAudioSource.Play();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (playButton != null)
                playButton.onClick.RemoveAllListeners();
                
            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();
                
            if (creditsButton != null)
                creditsButton.onClick.RemoveAllListeners();
                
            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
    }
}