using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WWIII.Core;
using WWIII.Gameplay;

namespace WWIII.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("HUD References")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private GameObject hudPanel;
        
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private Image coinIcon;
        [SerializeField] private string scoreFormat = "Score: {0:N0}";
        [SerializeField] private string coinsFormat = "{0}";
        
        [Header("Health/Lives Display")]
        [SerializeField] private GameObject healthContainer;
        [SerializeField] private Image[] healthHearts;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private string livesFormat = "Lives: {0}";
        
        [Header("Progress Display")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private string progressFormat = "{0:F0}%";
        
        [Header("Controls")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject mobileControls;
        
        [Header("Notifications")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool showFPS = false;
        [SerializeField] private TextMeshProUGUI fpsText;
        
        public static HUDManager Instance { get; private set; }
        
        // Game state
        private int currentScore = 0;
        private int currentCoins = 0;
        private int currentHealth = 3;
        private int maxHealth = 3;
        private int currentLives = 3;
        private float currentProgress = 0f;
        
        // FPS tracking
        private float deltaTime = 0f;
        
        public System.Action OnPausePressed;
        
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
            
            InitializeHUD();
        }
        
        private void Start()
        {
            SetupHUD();
            RegisterEvents();
        }
        
        private void Update()
        {
            if (showFPS && fpsText != null)
            {
                UpdateFPS();
            }
        }
        
        private void InitializeHUD()
        {
            // Ensure canvas is set up for UI scaling
            if (hudCanvas == null)
                hudCanvas = GetComponent<Canvas>();
            
            if (hudCanvas != null)
            {
                hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                hudCanvas.sortingOrder = 100; // High priority for HUD
                
                // Add CanvasScaler if not present
                CanvasScaler scaler = hudCanvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = hudCanvas.gameObject.AddComponent<CanvasScaler>();
                }
                
                // Configure for mobile-friendly scaling
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
            
            // Setup pause button
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseButtonPressed);
            }
        }
        
        private void SetupHUD()
        {
            // Initialize displays
            UpdateScoreDisplay();
            UpdateCoinsDisplay();
            UpdateHealthDisplay();
            UpdateLivesDisplay();
            UpdateProgressDisplay();
            
            // Configure mobile controls visibility
            ConfigureMobileControls();
            
            Debug.Log("HUD initialized");
        }
        
        private void RegisterEvents()
        {
            // Register for pickup events
            var pickups = FindObjectsByType<Pickup>(FindObjectsSortMode.None);
            foreach (var pickup in pickups)
            {
                pickup.OnPickedUp += HandlePickupCollected;
            }
            
            // Register for checkpoint events
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.OnCheckpointActivated += HandleCheckpointActivated;
                CheckpointManager.Instance.OnPlayerRespawned += HandlePlayerRespawned;
            }
        }
        
        private void ConfigureMobileControls()
        {
            bool showMobileControls = true;
            
            #if UNITY_EDITOR
            showMobileControls = false;
            #elif UNITY_STANDALONE
            showMobileControls = false;
            #elif UNITY_WEBGL
            showMobileControls = false;
            #endif
            
            if (mobileControls != null)
            {
                mobileControls.SetActive(showMobileControls);
            }
        }
        
        private void UpdateFPS()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            int fps = Mathf.CeilToInt(1.0f / deltaTime);
            
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps}";
            }
        }
        
        // Score Management
        public void AddScore(int points)
        {
            currentScore += points;
            UpdateScoreDisplay();
            
            // Show score popup notification
            if (points > 0)
            {
                ShowNotification($"+{points} points!");
            }
        }
        
        public void SetScore(int score)
        {
            currentScore = score;
            UpdateScoreDisplay();
        }
        
        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = string.Format(scoreFormat, currentScore);
            }
        }
        
        // Coins Management
        public void AddCoins(int coins)
        {
            currentCoins += coins;
            UpdateCoinsDisplay();
            
            // Animate coin icon
            if (coinIcon != null)
            {
                StartCoroutine(AnimateCoinIcon());
            }
        }
        
        public void SetCoins(int coins)
        {
            currentCoins = coins;
            UpdateCoinsDisplay();
        }
        
        private void UpdateCoinsDisplay()
        {
            if (coinsText != null)
            {
                coinsText.text = string.Format(coinsFormat, currentCoins);
            }
        }
        
        private System.Collections.IEnumerator AnimateCoinIcon()
        {
            if (coinIcon == null) yield break;
            
            Vector3 originalScale = coinIcon.transform.localScale;
            Vector3 targetScale = originalScale * 1.3f;
            
            // Scale up
            float duration = 0.15f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                coinIcon.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // Scale back down
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                coinIcon.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            coinIcon.transform.localScale = originalScale;
        }
        
        // Health Management
        public void SetHealth(int health, int maxHealthValue = -1)
        {
            if (maxHealthValue > 0)
            {
                maxHealth = maxHealthValue;
            }
            
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            UpdateHealthDisplay();
        }
        
        public void TakeDamage(int damage = 1)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            UpdateHealthDisplay();
            
            // Flash health display
            StartCoroutine(FlashHealthDisplay());
        }
        
        public void Heal(int healAmount = 1)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
            UpdateHealthDisplay();
        }
        
        private void UpdateHealthDisplay()
        {
            if (healthHearts != null)
            {
                for (int i = 0; i < healthHearts.Length; i++)
                {
                    if (healthHearts[i] != null)
                    {
                        healthHearts[i].gameObject.SetActive(i < maxHealth);
                        
                        if (i < maxHealth)
                        {
                            // Show filled or empty heart
                            healthHearts[i].color = i < currentHealth ? Color.white : Color.gray;
                        }
                    }
                }
            }
        }
        
        private System.Collections.IEnumerator FlashHealthDisplay()
        {
            if (healthContainer == null) yield break;
            
            Color originalColor = Color.white;
            Color flashColor = Color.red;
            float flashDuration = 0.1f;
            
            Image[] images = healthContainer.GetComponentsInChildren<Image>();
            
            // Flash red
            foreach (var img in images)
            {
                img.color = flashColor;
            }
            
            yield return new WaitForSecondsRealtime(flashDuration);
            
            // Return to normal
            foreach (var img in images)
            {
                img.color = originalColor;
            }
        }
        
        // Lives Management
        public void SetLives(int lives)
        {
            currentLives = lives;
            UpdateLivesDisplay();
        }
        
        public void LoseLife()
        {
            currentLives = Mathf.Max(0, currentLives - 1);
            UpdateLivesDisplay();
        }
        
        public void GainLife()
        {
            currentLives++;
            UpdateLivesDisplay();
        }
        
        private void UpdateLivesDisplay()
        {
            if (livesText != null)
            {
                livesText.text = string.Format(livesFormat, currentLives);
            }
        }
        
        // Progress Management
        public void UpdateProgress(float progress)
        {
            currentProgress = Mathf.Clamp01(progress);
            UpdateProgressDisplay();
        }
        
        private void UpdateProgressDisplay()
        {
            if (progressBar != null)
            {
                progressBar.value = currentProgress;
            }
            
            if (progressText != null)
            {
                progressText.text = string.Format(progressFormat, currentProgress * 100f);
            }
        }
        
        // Notification System
        public void ShowNotification(string message)
        {
            StartCoroutine(ShowNotificationCoroutine(message));
        }
        
        private System.Collections.IEnumerator ShowNotificationCoroutine(string message)
        {
            if (notificationPanel == null || notificationText == null) yield break;
            
            notificationText.text = message;
            notificationPanel.SetActive(true);
            
            // Fade in
            CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            float fadeInDuration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            
            // Wait
            yield return new WaitForSecondsRealtime(notificationDuration);
            
            // Fade out
            float fadeOutDuration = 0.3f;
            elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            notificationPanel.SetActive(false);
        }
        
        // Event Handlers
        private void HandlePickupCollected(PickupType pickupType, int value)
        {
            switch (pickupType)
            {
                case PickupType.Coin:
                    AddCoins(1);
                    AddScore(value);
                    break;
                case PickupType.Gem:
                    AddScore(value);
                    break;
                case PickupType.Health:
                    Heal(value);
                    break;
                default:
                    AddScore(value);
                    break;
            }
        }
        
        private void HandleCheckpointActivated(Checkpoint checkpoint)
        {
            ShowNotification("Checkpoint reached!");
            
            // Update progress based on checkpoint manager
            if (CheckpointManager.Instance != null)
            {
                float progress = CheckpointManager.Instance.GetProgressPercentage() / 100f;
                UpdateProgress(progress);
            }
        }
        
        private void HandlePlayerRespawned()
        {
            ShowNotification("Respawning...");
        }
        
        private void OnPauseButtonPressed()
        {
            OnPausePressed?.Invoke();
            
            // Trigger pause menu
            var pauseMenu = FindFirstObjectByType<PauseMenuManager>();
            if (pauseMenu != null)
            {
                pauseMenu.ShowPauseMenu();
            }
        }
        
        // Public Getters
        public int GetScore() => currentScore;
        public int GetCoins() => currentCoins;
        public int GetHealth() => currentHealth;
        public int GetLives() => currentLives;
        public float GetProgress() => currentProgress;
        
        // HUD Visibility
        public void ShowHUD()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(true);
            }
        }
        
        public void HideHUD()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
        }
        
        public void SetFPSDisplay(bool show)
        {
            showFPS = show;
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(show);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up events
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseButtonPressed);
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}