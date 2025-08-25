using UnityEngine;

[AddComponentMenu("Setup/Scene Setup")]
public class SceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool addPlayerHealthToExistingPlayer = true;

    [ContextMenu("Setup Scene Now")]
    public void SetupScene()
    {
        Debug.Log("=== SCENE SETUP STARTING ===");

        SetupPlayer();
        SetupUI();
        ValidateSetup();

        Debug.Log("=== SCENE SETUP COMPLETE ===");
    }

    private void Start()
    {
        if (setupOnStart)
        {
            SetupScene();
        }
    }

    private void SetupPlayer()
    {
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player == null)
        {
            Debug.LogError("No PlayerController2D found in scene!");
            return;
        }

        // Fix Rigidbody2D constraints
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            var oldConstraints = rb.constraints;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"Fixed Player constraints: {oldConstraints} → {rb.constraints}");
        }

        // Add PlayerHealth if it doesn't exist
        if (addPlayerHealthToExistingPlayer)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = player.gameObject.AddComponent<PlayerHealth>();
                Debug.Log("Added PlayerHealth component to Player");
            }
        }

        Debug.Log("Player setup complete");
    }

    private void SetupUI()
    {
        // Find Canvas
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found in scene");
            return;
        }

        // Look for existing health UI
        var healthUI = FindFirstObjectByType<HealthUI>();
        if (healthUI != null)
        {
            Debug.Log("HealthUI already exists in scene");
            return;
        }

        // Create health UI GameObject
        var healthUIGO = new GameObject("HealthUI");
        healthUIGO.transform.SetParent(canvas.transform, false);

        // Add RectTransform and position in top-left
        var rectTransform = healthUIGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(20f, -20f);
        rectTransform.sizeDelta = new Vector2(200f, 50f);

        // Add TextMeshPro component
        var textComponent = healthUIGO.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = "♥♥♥";
        textComponent.fontSize = 24f;
        textComponent.color = Color.red;
        textComponent.fontStyle = TMPro.FontStyles.Bold;

        // Add HealthUI script
        var healthUIScript = healthUIGO.AddComponent<HealthUI>();

        // Assign the text component via reflection
        var field = typeof(HealthUI).GetField("healthText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(healthUIScript, textComponent);
        }

        Debug.Log("Created HealthUI in Canvas");
    }

    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== VALIDATION ===");

        // Check Player
        var player = FindFirstObjectByType<PlayerController2D>();
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        var rb = player?.GetComponent<Rigidbody2D>();

        bool playerOK = player != null;
        bool healthOK = playerHealth != null;
        bool constraintsOK = rb != null && rb.constraints == RigidbodyConstraints2D.FreezeRotation;

        Debug.Log($"✓ PlayerController2D: {(playerOK ? "OK" : "MISSING")}");
        Debug.Log($"✓ PlayerHealth: {(healthOK ? "OK" : "MISSING")}");
        Debug.Log($"✓ Player Constraints: {(constraintsOK ? "OK" : "NEEDS FIX")} - Current: {rb?.constraints}");

        // Check UI
        var healthUI = FindFirstObjectByType<HealthUI>();
        var canvas = FindFirstObjectByType<Canvas>();

        Debug.Log($"✓ Canvas: {(canvas != null ? "OK" : "MISSING")}");
        Debug.Log($"✓ HealthUI: {(healthUI != null ? "OK" : "MISSING")}");

        // Check Checkpoints
        var checkpoints = FindObjectsByType<CheckpointTrigger>(FindObjectsSortMode.None);
        Debug.Log($"✓ Checkpoints: {checkpoints.Length} found");

        // Check Hazards
        var hazards = FindObjectsByType<DamageOnTouch>(FindObjectsSortMode.None);
        Debug.Log($"✓ Hazards: {hazards.Length} found");

        Debug.Log("=== VALIDATION COMPLETE ===");
    }
}