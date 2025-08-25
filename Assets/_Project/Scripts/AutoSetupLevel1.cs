using UnityEngine;

[AddComponentMenu("WWIII/Auto Setup Level 1")]
public class AutoSetupLevel1 : MonoBehaviour
{
    [Header("Auto Configuration")]
    [SerializeField] private bool setupOnAwake = true;
    
    private void Awake()
    {
        if (setupOnAwake)
        {
            SetupEverything();
        }
    }

    [ContextMenu("Setup Everything Now")]
    public void SetupEverything()
    {
        Debug.Log("🚀 AUTO-SETTING UP LEVEL 1...");
        
        FixPlayerImmediately();
        AddPlayerHealthIfMissing();
        ConfigureExistingSpikes();
        ConfigureExistingCheckpoint();
        SetupCameraFollow();
        CreateHealthUIIfMissing();
        
        Debug.Log("✅ AUTO-SETUP COMPLETE! Level 1 should be playable now.");
        
        // Run quick validation
        ValidateQuick();
    }

    private void FixPlayerImmediately()
    {
        var player = GameObject.Find("Player");
        if (player == null) return;
        
        var playerController = player.GetComponent<PlayerController2D>();
        if (playerController == null)
        {
            playerController = player.AddComponent<PlayerController2D>();
        }
        
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = player.AddComponent<Rigidbody2D>();
        }
        
        // CRITICAL FIX: Set correct constraints
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 3.5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        Debug.Log($"✅ Player fixed: constraints={rb.constraints}, gravity={rb.gravityScale}");
    }

    private void AddPlayerHealthIfMissing()
    {
        var player = GameObject.Find("Player");
        if (player == null) return;
        
        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.AddComponent<PlayerHealth>();
            Debug.Log("✅ Added PlayerHealth to Player");
        }
    }

    private void ConfigureExistingSpikes()
    {
        var spikes = GameObject.Find("Spikes");
        if (spikes == null) return;
        
        var damageComponent = spikes.GetComponent<DamageOnTouch>();
        if (damageComponent == null)
        {
            damageComponent = spikes.AddComponent<DamageOnTouch>();
        }
        
        var collider = spikes.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = spikes.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
        
        // Set to Water layer (fallback for Hazard)
        int hazardLayer = LayerMask.NameToLayer("Water");
        if (hazardLayer != -1)
        {
            spikes.layer = hazardLayer;
        }
        
        Debug.Log("✅ Configured existing Spikes with DamageOnTouch");
    }

    private void ConfigureExistingCheckpoint()
    {
        var checkpoint = GameObject.Find("Checkpoint");
        if (checkpoint == null) return;
        
        var checkpointTrigger = checkpoint.GetComponent<CheckpointTrigger>();
        if (checkpointTrigger == null)
        {
            checkpointTrigger = checkpoint.AddComponent<CheckpointTrigger>();
        }
        
        var collider = checkpoint.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = checkpoint.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
        
        Debug.Log("✅ Configured existing Checkpoint");
    }

    private void SetupCameraFollow()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        var cameraFollow = mainCamera.GetComponent<CameraFollow2D>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow2D>();
        }
        
        var player = GameObject.Find("Player");
        if (player != null)
        {
            // Use reflection to set the target
            var targetField = typeof(CameraFollow2D).GetField("target", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null)
            {
                targetField.SetValue(cameraFollow, player.transform);
                Debug.Log("✅ Camera configured to follow Player");
            }
        }
    }

    private void CreateHealthUIIfMissing()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        
        var existingHealthUI = canvas.GetComponentInChildren<HealthUI>();
        if (existingHealthUI != null)
        {
            Debug.Log("✅ HealthUI already exists");
            return;
        }
        
        // Create new health UI
        var healthUIGO = new GameObject("HealthUI");
        healthUIGO.transform.SetParent(canvas.transform, false);
        
        var rectTransform = healthUIGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(20f, -20f);
        rectTransform.sizeDelta = new Vector2(150f, 40f);
        
        var textComponent = healthUIGO.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = "♥♥♥";
        textComponent.fontSize = 20f;
        textComponent.color = Color.red;
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        
        var healthUIScript = healthUIGO.AddComponent<HealthUI>();
        
        // Set the text field via reflection
        var field = typeof(HealthUI).GetField("healthText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(healthUIScript, textComponent);
        }
        
        Debug.Log("✅ Created HealthUI");
    }

    private void ValidateQuick()
    {
        Debug.Log("--- Quick Validation ---");
        
        var player = GameObject.Find("Player");
        var playerController = player?.GetComponent<PlayerController2D>();
        var playerHealth = player?.GetComponent<PlayerHealth>();
        var rb = player?.GetComponent<Rigidbody2D>();
        var cameraFollow = Camera.main?.GetComponent<CameraFollow2D>();
        var healthUI = FindFirstObjectByType<HealthUI>();
        
        Debug.Log($"Player Controller: {(playerController != null ? "✅" : "❌")}");
        Debug.Log($"Player Health: {(playerHealth != null ? "✅" : "❌")}");
        Debug.Log($"Player Physics: {(rb?.constraints == RigidbodyConstraints2D.FreezeRotation ? "✅" : "❌")}");
        Debug.Log($"Camera Follow: {(cameraFollow != null ? "✅" : "❌")}");
        Debug.Log($"Health UI: {(healthUI != null ? "✅" : "❌")}");
        
        Debug.Log("🎮 Ready to test! Use WASD/Arrows + Space to play.");
    }
}