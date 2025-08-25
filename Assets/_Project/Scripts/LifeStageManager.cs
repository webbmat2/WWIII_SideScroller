using UnityEngine;
using DG.Tweening;

/// <summary>
/// Manages the different life stages of James William Webb III
/// </summary>
public class LifeStageManager : MonoBehaviour
{
    [System.Serializable]
    public class LifeStage
    {
        public string stageName;
        public string decade;
        public int startYear;
        public int endYear;
        public Color themeColor;
        public Sprite backgroundMusic;
        public string[] keyMilestones;
        public Vector2 characterScale = Vector2.one; // Character grows over time
    }

    [Header("Life Journey")]
    public LifeStage[] lifeStages;
    public int currentStageIndex = 0;
    
    [Header("Character Progression")]
    public Transform character;
    public SpriteRenderer characterRenderer;
    
    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI yearDisplay;
    public TMPro.TextMeshProUGUI stageTitle;
    public UnityEngine.UI.Image stageTint;

    public LifeStage CurrentStage => 
        lifeStages != null && currentStageIndex < lifeStages.Length ? 
        lifeStages[currentStageIndex] : null;

    private void Start()
    {
        if (lifeStages == null || lifeStages.Length == 0)
        {
            InitializeDefaultLifeStages();
        }
        
        ApplyCurrentStage();
    }

    private void InitializeDefaultLifeStages()
    {
        lifeStages = new LifeStage[]
        {
            new LifeStage
            {
                stageName = "Childhood",
                decade = "1980s-1990s",
                startYear = 1985,
                endYear = 1995,
                themeColor = new Color(0.9f, 0.8f, 0.6f), // Warm, nostalgic
                characterScale = new Vector2(0.6f, 0.6f),
                keyMilestones = new string[] { "First Steps", "School Starts", "Making Friends" }
            },
            new LifeStage
            {
                stageName = "Teenage Years",
                decade = "1990s-2000s",
                startYear = 1995,
                endYear = 2005,
                themeColor = new Color(0.6f, 0.9f, 0.8f), // Energetic green
                characterScale = new Vector2(0.8f, 0.8f),
                keyMilestones = new string[] { "High School", "First Job", "Driving License" }
            },
            new LifeStage
            {
                stageName = "Young Adult",
                decade = "2000s-2010s",
                startYear = 2005,
                endYear = 2015,
                themeColor = new Color(0.8f, 0.6f, 0.9f), // Ambitious purple
                characterScale = new Vector2(1.0f, 1.0f),
                keyMilestones = new string[] { "College", "Career Start", "Independence" }
            },
            new LifeStage
            {
                stageName = "Accomplished Adult",
                decade = "2010s-2020s",
                startYear = 2015,
                endYear = 2025,
                themeColor = new Color(0.9f, 0.7f, 0.5f), // Mature gold
                characterScale = new Vector2(1.0f, 1.0f),
                keyMilestones = new string[] { "Career Success", "Family", "Legacy Building" }
            }
        };
    }

    public void AdvanceToNextStage()
    {
        if (currentStageIndex < lifeStages.Length - 1)
        {
            currentStageIndex++;
            ApplyCurrentStage();
            
            Debug.Log($"Advanced to {CurrentStage.stageName} ({CurrentStage.decade})");
        }
    }

    public void GoToPreviousStage()
    {
        if (currentStageIndex > 0)
        {
            currentStageIndex--;
            ApplyCurrentStage();
            
            Debug.Log($"Returned to {CurrentStage.stageName} ({CurrentStage.decade})");
        }
    }

    private void ApplyCurrentStage()
    {
        if (CurrentStage == null) return;

        // Update UI
        if (yearDisplay != null)
        {
            yearDisplay.text = $"{CurrentStage.startYear} - {CurrentStage.endYear}";
            yearDisplay.DOColor(CurrentStage.themeColor, 0.5f);
        }

        if (stageTitle != null)
        {
            stageTitle.text = CurrentStage.stageName;
            stageTitle.DOColor(CurrentStage.themeColor, 0.5f);
        }

        if (stageTint != null)
        {
            stageTint.DOColor(CurrentStage.themeColor, 1f);
        }

        // Update character appearance
        if (character != null)
        {
            character.DOScale(CurrentStage.characterScale, 0.8f).SetEase(Ease.OutBack);
        }

        // Apply theme color to environment (optional)
        Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, 
                                                CurrentStage.themeColor * 0.3f, 0.5f);
    }

    /// <summary>
    /// Trigger a life milestone achievement
    /// </summary>
    public void TriggerMilestone(string milestoneName)
    {
        Debug.Log($"🎉 Milestone achieved: {milestoneName} in {CurrentStage.stageName}");
        
        // Visual celebration
        if (character != null)
        {
            character.DOPunchScale(Vector3.one * 0.2f, 0.5f, 5, 1f);
        }

        // Could trigger milestone UI, achievements, etc.
        ShowMilestoneNotification(milestoneName);
    }

    private void ShowMilestoneNotification(string milestone)
    {
        // Create floating milestone text
        var milestoneGO = new GameObject("Milestone");
        var textMesh = milestoneGO.AddComponent<TMPro.TextMeshPro>();
        textMesh.text = $"✨ {milestone}";
        textMesh.fontSize = 4;
        textMesh.color = CurrentStage.themeColor;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        
        milestoneGO.transform.position = character.position + Vector3.up * 2;
        
        // Animate the milestone text
        DOTweenHelper.FloatingText(milestoneGO.transform, textMesh.text, 
                                  CurrentStage.themeColor, 2f);
    }

    /// <summary>
    /// Get the current life stage for other systems to reference
    /// </summary>
    public string GetCurrentDecade() => CurrentStage?.decade ?? "Unknown";
    
    public Color GetCurrentThemeColor() => CurrentStage?.themeColor ?? Color.white;
    
    public float GetCurrentAge() => CurrentStage != null ? 
        (CurrentStage.startYear - 1985) + 10 : 0; // Assuming born around 1985

#if UNITY_EDITOR
    [Header("Debug Controls")]
    [Space]
    [Tooltip("Check this box to advance to next stage")]
    public bool debugNextStage;
    
    [Tooltip("Check this box to go to previous stage")]
    public bool debugPrevStage;

    private void OnValidate()
    {
        if (debugNextStage)
        {
            debugNextStage = false;
            if (Application.isPlaying) AdvanceToNextStage();
        }
        
        if (debugPrevStage)
        {
            debugPrevStage = false;
            if (Application.isPlaying) GoToPreviousStage();
        }
    }
#endif
}