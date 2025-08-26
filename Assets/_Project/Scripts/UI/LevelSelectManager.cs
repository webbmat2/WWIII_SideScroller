using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace WWIII.UI
{
    public class LevelSelectManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private ScrollRect scrollRect;
        
        [Header("Level Configuration")]
        [SerializeField] private LevelData[] levels;
        
        [Header("Audio")]
        [SerializeField] private AudioSource menuAudioSource;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip levelSelectSound;
        
        private List<LevelButton> levelButtons = new List<LevelButton>();
        
        private void Start()
        {
            InitializeLevelSelect();
            SetupBackButton();
            CreateLevelButtons();
        }
        
        private void InitializeLevelSelect()
        {
            Debug.Log("Level Select initialized");
        }
        
        private void SetupBackButton()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }
        }
        
        private void CreateLevelButtons()
        {
            if (levelButtonContainer == null || levelButtonPrefab == null)
            {
                Debug.LogError("Level button container or prefab not assigned!");
                return;
            }
            
            // Clear existing buttons
            foreach (Transform child in levelButtonContainer)
            {
                DestroyImmediate(child.gameObject);
            }
            levelButtons.Clear();
            
            // Create buttons for each level
            for (int i = 0; i < levels.Length; i++)
            {
                CreateLevelButton(levels[i], i);
            }
        }
        
        private void CreateLevelButton(LevelData levelData, int levelIndex)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();
            
            if (levelButton == null)
            {
                levelButton = buttonObj.AddComponent<LevelButton>();
            }
            
            // Configure the button
            levelButton.Setup(levelData, levelIndex, OnLevelSelected);
            levelButtons.Add(levelButton);
        }
        
        private void OnLevelSelected(int levelIndex)
        {
            PlayLevelSelectSound();
            
            if (levelIndex >= 0 && levelIndex < levels.Length)
            {
                LevelData selectedLevel = levels[levelIndex];
                Debug.Log($"Selected level: {selectedLevel.levelName}");
                
                // Load the selected level
                if (WWIII.Core.SceneTransitionManager.Instance != null)
                {
                    WWIII.Core.SceneTransitionManager.Instance.LoadLevel(levelIndex + 1);
                }
                else
                {
                    Debug.LogWarning("SceneTransitionManager not found!");
                }
            }
        }
        
        private void OnBackButtonClicked()
        {
            PlayButtonSound();
            
            // Return to main menu
            if (WWIII.Core.SceneTransitionManager.Instance != null)
            {
                WWIII.Core.SceneTransitionManager.Instance.LoadMainMenu();
            }
            else
            {
                Debug.LogWarning("SceneTransitionManager not found!");
            }
        }
        
        private void PlayButtonSound()
        {
            if (menuAudioSource != null && buttonClickSound != null)
            {
                menuAudioSource.PlayOneShot(buttonClickSound);
            }
        }
        
        private void PlayLevelSelectSound()
        {
            if (menuAudioSource != null && levelSelectSound != null)
            {
                menuAudioSource.PlayOneShot(levelSelectSound);
            }
        }
        
        public void RefreshLevelButtons()
        {
            foreach (LevelButton button in levelButtons)
            {
                if (button != null)
                {
                    button.RefreshState();
                }
            }
        }
        
        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
            }
        }
    }
    
    [System.Serializable]
    public class LevelData
    {
        [Header("Level Info")]
        public string levelName = "Level 1";
        public string sceneName = "L1_Tutorial";
        public Sprite levelThumbnail;
        public string description = "Learn the basics";
        
        [Header("Progress")]
        public bool isUnlocked = true;
        public bool isCompleted = false;
        public int bestScore = 0;
        public float bestTime = 0f;
        
        [Header("Requirements")]
        public int requiredLevel = 0; // Previous level that must be completed
        public bool isStub = false; // Indicates if this is a stub level
    }
    
    public class LevelButton : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject completedIndicator;
        [SerializeField] private GameObject stubIndicator;
        
        private LevelData levelData;
        private int levelIndex;
        private System.Action<int> onLevelSelected;
        
        public void Setup(LevelData data, int index, System.Action<int> callback)
        {
            levelData = data;
            levelIndex = index;
            onLevelSelected = callback;
            
            // Get components if not assigned
            if (button == null) button = GetComponent<Button>();
            if (thumbnailImage == null) thumbnailImage = GetComponentInChildren<Image>();
            if (levelNameText == null) levelNameText = GetComponentInChildren<Text>();
            
            // Setup button
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onLevelSelected?.Invoke(levelIndex));
            }
            
            UpdateDisplay();
        }
        
        public void RefreshState()
        {
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (levelData == null) return;
            
            // Update thumbnail
            if (thumbnailImage != null && levelData.levelThumbnail != null)
            {
                thumbnailImage.sprite = levelData.levelThumbnail;
            }
            
            // Update text
            if (levelNameText != null)
            {
                levelNameText.text = levelData.levelName;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = levelData.description;
            }
            
            // Update state indicators
            SetActive(lockedOverlay, !levelData.isUnlocked);
            SetActive(completedIndicator, levelData.isCompleted);
            SetActive(stubIndicator, levelData.isStub);
            
            // Update button interactability
            if (button != null)
            {
                button.interactable = levelData.isUnlocked;
            }
        }
        
        private void SetActive(GameObject obj, bool active)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }
}