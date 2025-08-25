using UnityEngine;
using TMPro;

[AddComponentMenu("UI/Health UI")]
public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private bool useHeartSymbols = true;
    [SerializeField] private string heartSymbol = "♥";
    [SerializeField] private string emptyHeartSymbol = "♡";

    private PlayerHealth playerHealth;

    private void Start()
    {
        // Find player health component
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogWarning("HealthUI: No PlayerHealth found in scene!");
            return;
        }

        // Subscribe to health changes
        playerHealth.OnHealthChanged += UpdateHealthDisplay;
        
        // Initial display
        UpdateHealthDisplay(playerHealth.CurrentHP, playerHealth.MaxHP);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }
    }

    private void UpdateHealthDisplay(int currentHP, int maxHP)
    {
        if (healthText == null) return;

        if (useHeartSymbols)
        {
            string hearts = "";
            
            // Add filled hearts
            for (int i = 0; i < currentHP; i++)
            {
                hearts += heartSymbol;
            }
            
            // Add empty hearts
            for (int i = currentHP; i < maxHP; i++)
            {
                hearts += emptyHeartSymbol;
            }
            
            healthText.text = hearts;
        }
        else
        {
            healthText.text = $"HP: {currentHP}/{maxHP}";
        }
    }

    // Public method to manually refresh the display
    public void RefreshDisplay()
    {
        if (playerHealth != null)
        {
            UpdateHealthDisplay(playerHealth.CurrentHP, playerHealth.MaxHP);
        }
    }
}