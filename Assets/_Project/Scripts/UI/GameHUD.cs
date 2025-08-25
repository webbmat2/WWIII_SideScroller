using UnityEngine;
using TMPro;

[AddComponentMenu("UI/Game HUD")]
public class GameHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI collectibleText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private RectTransform heartContainer;
    
    [Header("Health Display")]
    [SerializeField] private bool useHearts = true;
    [SerializeField] private string fullHeart = "♥";
    [SerializeField] private string emptyHeart = "♡";
    [SerializeField] private Color healthColor = Color.red;
    
    [Header("Auto Setup")]
    [SerializeField] private bool createUIOnStart = true;
    
    private ChapterManager chapterManager;
    private PlayerHealth playerHealth;
    private PlayerAbilities playerAbilities;

    private void Awake()
    {
        if (createUIOnStart)
        {
            CreateUIElements();
        }
    }

    private void Start()
    {
        chapterManager = ChapterManager.Instance;
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerAbilities = FindFirstObjectByType<PlayerAbilities>();
        
        SubscribeToEvents();
        RefreshAllUI();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void CreateUIElements()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("GameHUD must be child of Canvas");
            return;
        }

        // Create Health Text
        if (healthText == null)
        {
            healthText = CreateUIText("HealthText", new Vector2(20f, -20f), new Vector2(150f, 30f));
            healthText.color = healthColor;
            healthText.fontSize = 18f;
            healthText.fontStyle = FontStyles.Bold;
        }

        // Create Collectible Text
        if (collectibleText == null)
        {
            collectibleText = CreateUIText("CollectibleText", new Vector2(20f, -60f), new Vector2(120f, 30f));
            collectibleText.fontSize = 16f;
        }

        // Create Ability Text
        if (abilityText == null)
        {
            abilityText = CreateUIText("AbilityText", new Vector2(20f, -100f), new Vector2(150f, 30f));
            abilityText.fontSize = 14f;
            abilityText.color = Color.cyan;
        }

        Debug.Log("GameHUD UI elements created");
    }

    private TextMeshProUGUI CreateUIText(string name, Vector2 position, Vector2 size)
    {
        var textGO = new GameObject(name);
        textGO.transform.SetParent(transform, false);
        
        var rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        
        var textComponent = textGO.AddComponent<TextMeshProUGUI>();
        
        return textComponent;
    }

    private void SubscribeToEvents()
    {
        if (chapterManager != null)
        {
            chapterManager.OnChapterLoaded += OnChapterLoaded;
            chapterManager.OnCollectibleChanged += OnCollectibleChanged;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
        }

        if (playerAbilities != null)
        {
            playerAbilities.OnAbilityChanged += OnAbilityChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (chapterManager != null)
        {
            chapterManager.OnChapterLoaded -= OnChapterLoaded;
            chapterManager.OnCollectibleChanged -= OnCollectibleChanged;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }

        if (playerAbilities != null)
        {
            playerAbilities.OnAbilityChanged -= OnAbilityChanged;
        }
    }

    private void OnChapterLoaded(ChapterData chapter)
    {
        RefreshCollectibleUI();
    }

    private void OnCollectibleChanged(int found, int max)
    {
        RefreshCollectibleUI();
    }

    private void OnHealthChanged(int current, int max)
    {
        RefreshHealthUI();
    }

    private void OnAbilityChanged(PowerUpType ability)
    {
        RefreshAbilityUI();
    }

    private void RefreshAllUI()
    {
        RefreshHealthUI();
        RefreshCollectibleUI();
        RefreshAbilityUI();
    }

    private void RefreshHealthUI()
    {
        if (healthText == null) return;

        if (playerHealth != null)
        {
            if (useHearts)
            {
                string heartsDisplay = "";
                for (int i = 0; i < playerHealth.MaxHP; i++)
                {
                    heartsDisplay += (i < playerHealth.CurrentHP) ? fullHeart : emptyHeart;
                }
                healthText.text = heartsDisplay;
            }
            else
            {
                healthText.text = $"HP: {playerHealth.CurrentHP}/{playerHealth.MaxHP}";
            }
        }
        else
        {
            healthText.text = useHearts ? "♥♥♥" : "HP: 3/3";
        }
    }

    private void RefreshCollectibleUI()
    {
        if (collectibleText == null) return;

        if (chapterManager != null)
        {
            collectibleText.text = $"Items: {chapterManager.CollectiblesFound}/{chapterManager.MaxCollectibles}";
        }
        else
        {
            collectibleText.text = "Items: 0/5";
        }
    }

    private void RefreshAbilityUI()
    {
        if (abilityText == null) return;

        if (playerAbilities != null)
        {
            string abilityName = GetAbilityDisplayName(playerAbilities.CurrentAbility);
            abilityText.text = $"Ability: {abilityName}";
            
            // Special indication for Chiliguaro
            if (playerAbilities.HasChiliguaro)
            {
                abilityText.color = Color.red;
                abilityText.text += " (ACTIVE)";
            }
            else
            {
                abilityText.color = Color.cyan;
            }
        }
        else
        {
            abilityText.text = "Ability: None";
        }
    }

    private string GetAbilityDisplayName(PowerUpType ability)
    {
        switch (ability)
        {
            case PowerUpType.Hose: return "Hose";
            case PowerUpType.Chiliguaro: return "Chiliguaro";
            case PowerUpType.CherryPie: return "Cherry Pie";
            case PowerUpType.SmartJim: return "Smart Jim";
            case PowerUpType.BeefJerky: return "Beef Jerky";
            case PowerUpType.CheeseBall: return "Cheese Ball";
            default: return "None";
        }
    }

    [ContextMenu("Force Refresh UI")]
    public void ForceRefreshUI()
    {
        RefreshAllUI();
    }
}