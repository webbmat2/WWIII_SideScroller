using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WWIII.Core;

namespace WWIII.UI
{
    public class GameOverManager : MonoBehaviour
    {
        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Canvas gameOverCanvas;
        [SerializeField] private CanvasGroup gameOverCanvasGroup;
        
        [Header("Victory Panel")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI victoryTitleText;
        [SerializeField] private TextMeshProUGUI victoryMessageText;
        
        [Header("Defeat Panel")]
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TextMeshProUGUI defeatTitleText;
        [SerializeField] private TextMeshProUGUI defeatMessageText;
        
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI finalCoinsText;
        [SerializeField] private TextMeshProUGUI completionTimeText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        
        [Header("Star Rating")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite filledStar;
        [SerializeField] private Sprite emptyStar;
        
        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button levelSelectButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float starAnimationDelay = 0.2f;
        [SerializeField] private float starAnimationDuration = 0.3f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Audio")]
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip defeatSound;
        [SerializeField] private AudioClip starSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        public static GameOverManager Instance { get; private set; }
        
        private AudioSource audioSource;
        private SceneTransitionManager sceneManager;
        private bool isGameOver = false;
        private float levelStartTime;
        private int currentStars = 0;
        
        public System.Action<GameOverType> OnGameOver;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                levelStartTime = Time.time;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeGameOver();
        }
        
        private void Start()
        {
            SetupGameOver();
        }
        
        private void InitializeGameOver()
        {
            sceneManager = SceneTransitionManager.Instance;
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            
            // Setup canvas
            if (gameOverCanvas == null)
                gameOverCanvas = GetComponent<Canvas>();
            
            if (gameOverCanvas != null)
            {
                gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameOverCanvas.sortingOrder = 300; // Highest priority
            }
            
            // Setup canvas group
            if (gameOverCanvasGroup == null)
                gameOverCanvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
            
            if (gameOverCanvasGroup == null)
                gameOverCanvasGroup = gameOverCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        private void SetupGameOver()
        {
            // Initially hide all panels
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            
            if (victoryPanel != null)
                victoryPanel.SetActive(false);
            
            if (defeatPanel != null)
                defeatPanel.SetActive(false);
            
            // Setup button listeners
            if (retryButton != null)
                retryButton.onClick.AddListener(RetryLevel);
            
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(NextLevel);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
            
            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(GoToLevelSelect);
            
            Debug.Log("Game Over manager initialized");
        }
        
        public void ShowVictory(int finalScore = 0, int finalCoins = 0, int stars = 3)
        {
            if (isGameOver) return;
            
            ShowGameOver(GameOverType.Victory, finalScore, finalCoins, stars);
        }
        
        public void ShowDefeat(int finalScore = 0, int finalCoins = 0)
        {
            if (isGameOver) return;
            
            ShowGameOver(GameOverType.Defeat, finalScore, finalCoins, 0);
        }
        
        private void ShowGameOver(GameOverType gameOverType, int finalScore, int finalCoins, int stars)
        {
            isGameOver = true;
            currentStars = stars;
            
            // Pause the game
            Time.timeScale = 0f;
            
            // Show appropriate panel
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
            
            if (gameOverType == GameOverType.Victory)
            {
                if (victoryPanel != null)
                    victoryPanel.SetActive(true);
                
                if (defeatPanel != null)
                    defeatPanel.SetActive(false);
                
                PlaySound(victorySound);
                
                // Configure next level button
                if (nextLevelButton != null)
                {
                    nextLevelButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (defeatPanel != null)
                    defeatPanel.SetActive(true);
                
                if (victoryPanel != null)
                    victoryPanel.SetActive(false);
                
                PlaySound(defeatSound);
                
                // Hide next level button
                if (nextLevelButton != null)
                {
                    nextLevelButton.gameObject.SetActive(false);
                }
            }
            
            // Update displays
            UpdateScoreDisplay(finalScore, finalCoins);
            UpdateStarDisplay(stars);
            
            // Hide HUD
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.HideHUD();
            }
            
            // Trigger haptic feedback
            if (MobileOptimizer.Instance != null)
            {
                HapticFeedbackType hapticType = gameOverType == GameOverType.Victory 
                    ? HapticFeedbackType.HeavyImpact 
                    : HapticFeedbackType.MediumImpact;
                MobileOptimizer.Instance.TriggerHapticFeedback(hapticType);
            }
            
            // Start animations
            StartCoroutine(ShowGameOverCoroutine());
            
            // Save progress if victory
            if (gameOverType == GameOverType.Victory)
            {
                SaveLevelProgress(finalScore, finalCoins, stars);
            }
            
            // Notify listeners
            OnGameOver?.Invoke(gameOverType);
            
            Debug.Log($"Game Over: {gameOverType} - Score: {finalScore}, Coins: {finalCoins}, Stars: {stars}");
        }
        
        private void UpdateScoreDisplay(int finalScore, int finalCoins)
        {
            float completionTime = Time.time - levelStartTime;
            
            if (finalScoreText != null)
                finalScoreText.text = $"Score: {finalScore:N0}";
            
            if (finalCoinsText != null)
                finalCoinsText.text = $"Coins: {finalCoins}";
            
            if (completionTimeText != null)
            {
                int minutes = Mathf.FloorToInt(completionTime / 60f);
                int seconds = Mathf.FloorToInt(completionTime % 60f);
                completionTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            
            // Show best score if available
            if (bestScoreText != null)
            {
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                int bestScore = PlayerPrefs.GetInt($"BestScore_{currentScene}", 0);
                
                if (finalScore > bestScore)
                {
                    bestScoreText.text = "NEW BEST!";
                    bestScoreText.color = Color.yellow;
                }
                else
                {
                    bestScoreText.text = $"Best: {bestScore:N0}";
                    bestScoreText.color = Color.white;
                }
            }
        }
        
        private void UpdateStarDisplay(int stars)
        {
            if (starImages == null) return;
            
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = i < stars ? filledStar : emptyStar;
                    starImages[i].color = i < stars ? Color.white : Color.gray;
                }
            }
        }
        
        private System.Collections.IEnumerator ShowGameOverCoroutine()
        {
            // Fade in panel
            yield return StartCoroutine(FadeInPanel());
            
            // Animate stars
            if (currentStars > 0)
            {
                yield return StartCoroutine(AnimateStars());
            }
        }
        
        private System.Collections.IEnumerator FadeInPanel()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeInDuration;
                float curveValue = fadeInCurve.Evaluate(t);
                
                if (gameOverCanvasGroup != null)
                {
                    gameOverCanvasGroup.alpha = curveValue;
                }
                
                yield return null;
            }
            
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 1f;
            }
        }
        
        private System.Collections.IEnumerator AnimateStars()
        {
            if (starImages == null) yield break;
            
            for (int i = 0; i < currentStars && i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    yield return new WaitForSecondsRealtime(starAnimationDelay);
                    
                    // Animate star
                    StartCoroutine(AnimateSingleStar(starImages[i]));
                    
                    // Play star sound
                    PlaySound(starSound);
                }
            }
        }
        
        private System.Collections.IEnumerator AnimateSingleStar(Image starImage)
        {
            Vector3 originalScale = starImage.transform.localScale;
            Vector3 targetScale = originalScale * 1.5f;
            
            float elapsed = 0f;
            
            // Scale up
            while (elapsed < starAnimationDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (starAnimationDuration * 0.5f);
                starImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Scale back down
            while (elapsed < starAnimationDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (starAnimationDuration * 0.5f);
                starImage.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            starImage.transform.localScale = originalScale;
        }
        
        private void SaveLevelProgress(int finalScore, int finalCoins, int stars)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Save best score
            int currentBest = PlayerPrefs.GetInt($"BestScore_{currentScene}", 0);
            if (finalScore > currentBest)
            {
                PlayerPrefs.SetInt($"BestScore_{currentScene}", finalScore);
            }
            
            // Save star rating
            int currentStars = PlayerPrefs.GetInt($"Stars_{currentScene}", 0);
            if (stars > currentStars)
            {
                PlayerPrefs.SetInt($"Stars_{currentScene}", stars);
            }
            
            // Mark level as completed
            PlayerPrefs.SetInt($"Completed_{currentScene}", 1);
            
            // Unlock next level (simple progression)
            string nextLevelKey = GetNextLevelKey(currentScene);
            if (!string.IsNullOrEmpty(nextLevelKey))
            {
                PlayerPrefs.SetInt($"Unlocked_{nextLevelKey}", 1);
            }
            
            PlayerPrefs.Save();
        }
        
        private string GetNextLevelKey(string currentLevel)
        {
            // Simple level progression logic
            if (currentLevel.Contains("L1"))
                return "L2_Stub";
            else if (currentLevel.Contains("L2"))
                return "L3_Stub";
            else if (currentLevel.Contains("L3"))
                return "L4_Stub";
            else if (currentLevel.Contains("L4"))
                return "L5_Stub";
            
            return null; // No next level
        }
        
        // Button Event Handlers
        private void RetryLevel()
        {
            PlayButtonSound();
            
            // Resume time before scene transition
            Time.timeScale = 1f;
            
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            if (sceneManager != null)
            {
                sceneManager.LoadScene(currentScene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
            }
        }
        
        private void NextLevel()
        {
            PlayButtonSound();
            
            // Resume time before scene transition
            Time.timeScale = 1f;
            
            string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string nextLevel = GetNextLevelKey(currentLevel);
            
            if (!string.IsNullOrEmpty(nextLevel))
            {
                if (sceneManager != null)
                {
                    sceneManager.LoadScene(nextLevel);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevel);
                }
            }
            else
            {
                // No next level, go to level select
                GoToLevelSelect();
            }
        }
        
        private void GoToMainMenu()
        {
            PlayButtonSound();
            
            // Resume time before scene transition
            Time.timeScale = 1f;
            
            if (sceneManager != null)
            {
                sceneManager.LoadScene("MainMenu");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
        
        private void GoToLevelSelect()
        {
            PlayButtonSound();
            
            // Resume time before scene transition
            Time.timeScale = 1f;
            
            if (sceneManager != null)
            {
                sceneManager.LoadScene("Level_Select");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Select");
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
        public bool IsGameOver => isGameOver;
        
        // Public Methods
        public void ResetLevelTimer()
        {
            levelStartTime = Time.time;
        }
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (retryButton != null)
                retryButton.onClick.RemoveListener(RetryLevel);
            
            if (nextLevelButton != null)
                nextLevelButton.onClick.RemoveListener(NextLevel);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            
            if (levelSelectButton != null)
                levelSelectButton.onClick.RemoveListener(GoToLevelSelect);
            
            // Resume game if over when destroyed
            if (isGameOver)
            {
                Time.timeScale = 1f;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    public enum GameOverType
    {
        Victory,
        Defeat
    }
}