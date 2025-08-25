using UnityEngine;

[AddComponentMenu("WWIII/Level 1 Setup")]
public class Level1Setup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool forcePlayerConstraintsFix = true;

    [Header("Camera Configuration")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float cameraSmoothTime = 0.18f;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupLevel1();
        }
    }

    [ContextMenu("Setup Level 1 Now")]
    public void SetupLevel1()
    {
        Debug.Log("=== LEVEL 1 SETUP STARTING ===");

        FixPlayerConstraints();
        SetupPlayerHealth();
        SetupCamera();
        SetupUI();
        ValidateLevel();

        Debug.Log("=== LEVEL 1 SETUP COMPLETE ===");
    }

    private void FixPlayerConstraints()
    {
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player == null)
        {
            Debug.LogError("❌ No PlayerController2D found in scene!");
            return;
        }

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            var oldConstraints = rb.constraints;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // Ensure physics settings are correct
            rb.gravityScale = Mathf.Max(3.5f, rb.gravityScale);
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            Debug.Log($"✅ Player constraints fixed: {oldConstraints} → {rb.constraints}");
            Debug.Log($"✅ Player physics: gravity={rb.gravityScale}, interpolation={rb.interpolation}");
        }
    }

    private void SetupPlayerHealth()
    {
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player == null) return;

        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.gameObject.AddComponent<PlayerHealth>();
            Debug.Log("✅ Added PlayerHealth component to Player");
        }
        else
        {
            Debug.Log("✅ PlayerHealth already exists");
        }
    }

    private void SetupCamera()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("❌ No main camera found!");
            return;
        }

        var cameraFollow = mainCamera.GetComponent<CameraFollow2D>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow2D>();
            Debug.Log("✅ Added CameraFollow2D to Main Camera");
        }

        // Configure camera follow
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player != null)
        {
            // Set target via reflection since CameraFollow2D fields might be private
            var targetField = typeof(CameraFollow2D).GetField("target", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var offsetField = typeof(CameraFollow2D).GetField("offset", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var smoothTimeField = typeof(CameraFollow2D).GetField("smoothTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (targetField != null) targetField.SetValue(cameraFollow, player.transform);
            if (offsetField != null) offsetField.SetValue(cameraFollow, cameraOffset);
            if (smoothTimeField != null) smoothTimeField.SetValue(cameraFollow, cameraSmoothTime);

            Debug.Log("✅ Camera configured to follow Player");
        }

        // Find and configure CameraBounds
        var cameraBounds = FindFirstObjectByType<BoxCollider2D>();
        if (cameraBounds != null && cameraBounds.isTrigger)
        {
            var worldBoundsField = typeof(CameraFollow2D).GetField("worldBounds", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (worldBoundsField != null)
            {
                worldBoundsField.SetValue(cameraFollow, cameraBounds);
                Debug.Log("✅ Camera bounds configured");
            }
        }
    }

    private void SetupUI()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("⚠️ No Canvas found - UI will not be created");
            return;
        }

        var healthUI = FindFirstObjectByType<HealthUI>();
        if (healthUI == null)
        {
            CreateHealthUI(canvas);
        }
        else
        {
            Debug.Log("✅ HealthUI already exists");
        }
    }

    private void CreateHealthUI(Canvas canvas)
    {
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
        // TextMeshPro doesn't have sortingOrder property, use Canvas sorting instead

        // Add HealthUI script
        var healthUIScript = healthUIGO.AddComponent<HealthUI>();

        // Assign the text component via reflection
        var field = typeof(HealthUI).GetField("healthText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(healthUIScript, textComponent);
        }

        Debug.Log("✅ Created HealthUI in Canvas");
    }

    [ContextMenu("Validate Level 1")]
    public void ValidateLevel()
    {
        Debug.Log("=== LEVEL 1 VALIDATION ===");

        // Check core components
        var player = FindFirstObjectByType<PlayerController2D>();
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        var cameraFollow = FindFirstObjectByType<CameraFollow2D>();
        var healthUI = FindFirstObjectByType<HealthUI>();
        var canvas = FindFirstObjectByType<Canvas>();

        Debug.Log($"✓ PlayerController2D: {(player != null ? "✅ FOUND" : "❌ MISSING")}");
        Debug.Log($"✓ PlayerHealth: {(playerHealth != null ? "✅ FOUND" : "❌ MISSING")}");
        Debug.Log($"✓ CameraFollow2D: {(cameraFollow != null ? "✅ FOUND" : "❌ MISSING")}");
        Debug.Log($"✓ HealthUI: {(healthUI != null ? "✅ FOUND" : "❌ MISSING")}");
        Debug.Log($"✓ Canvas: {(canvas != null ? "✅ FOUND" : "❌ MISSING")}");

        // Check player physics
        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            bool constraintsOK = rb != null && rb.constraints == RigidbodyConstraints2D.FreezeRotation;
            bool physicsOK = rb != null && rb.gravityScale >= 3.5f && rb.interpolation == RigidbodyInterpolation2D.Interpolate;
            
            Debug.Log($"✓ Player Constraints: {(constraintsOK ? "✅ CORRECT" : "❌ WRONG")} - {rb?.constraints}");
            Debug.Log($"✓ Player Physics: {(physicsOK ? "✅ CORRECT" : "❌ WRONG")} - Gravity: {rb?.gravityScale}");
        }

        // Check scene objects
        var checkpoints = FindObjectsByType<CheckpointTrigger>(FindObjectsSortMode.None);
        var hazards = FindObjectsByType<DamageOnTouch>(FindObjectsSortMode.None);
        
        Debug.Log($"✓ Checkpoints: {checkpoints.Length} found");
        Debug.Log($"✓ Hazards: {hazards.Length} found");

        // Test quality gates
        bool canPlayerMove = player != null && player.GetComponent<Rigidbody2D>()?.constraints == RigidbodyConstraints2D.FreezeRotation;
        bool hasHealthSystem = playerHealth != null;
        bool hasCheckpoints = checkpoints.Length > 0;
        bool hasUI = healthUI != null;

        Debug.Log("=== QUALITY GATES ===");
        Debug.Log($"✓ Player can move: {(canPlayerMove ? "✅ PASS" : "❌ FAIL")}");
        Debug.Log($"✓ Health system active: {(hasHealthSystem ? "✅ PASS" : "❌ FAIL")}");
        Debug.Log($"✓ Checkpoints present: {(hasCheckpoints ? "✅ PASS" : "❌ FAIL")}");
        Debug.Log($"✓ UI working: {(hasUI ? "✅ PASS" : "❌ FAIL")}");

        bool allPassed = canPlayerMove && hasHealthSystem && hasCheckpoints && hasUI;
        Debug.Log($"=== OVERALL: {(allPassed ? "✅ LEVEL 1 READY" : "❌ NEEDS FIXES")} ===");
    }

    [ContextMenu("Test Damage System")]
    public void TestDamageSystem()
    {
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            Vector2 testKnockback = new Vector2(5f, 8f);
            playerHealth.ApplyDamage(1, testKnockback, 0.15f, 0.75f);
            Debug.Log("Applied test damage to player");
        }
        else
        {
            Debug.LogError("No PlayerHealth found for testing");
        }
    }
}