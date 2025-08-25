using UnityEngine;

[AddComponentMenu("WWIII/Deployment Check")]
public class WWIII_DeploymentCheck : MonoBehaviour
{
    [Header("Quality Gates")]
    [SerializeField] private bool runOnStart = true;
    
    private void Start()
    {
        if (runOnStart)
        {
            RunDeploymentCheck();
        }
    }

    [ContextMenu("Run Full Deployment Check")]
    public void RunDeploymentCheck()
    {
        Debug.Log("=== WWIII SIDEROLLER DEPLOYMENT CHECK ===");
        
        bool allTestsPassed = true;
        
        allTestsPassed &= CheckPlayerMovement();
        allTestsPassed &= CheckHealthSystem();
        allTestsPassed &= CheckCheckpointSystem();
        allTestsPassed &= CheckHazardSystem();
        allTestsPassed &= CheckUISystem();
        allTestsPassed &= CheckCameraSystem();
        allTestsPassed &= CheckSceneWiring();
        
        Debug.Log($"=== DEPLOYMENT RESULT: {(allTestsPassed ? "✅ READY FOR PLAY" : "❌ NEEDS FIXES")} ===");
        
        if (allTestsPassed)
        {
            Debug.Log("🎮 LEVEL 1 IS PLAYABLE! Press Play to test.");
        }
    }

    private bool CheckPlayerMovement()
    {
        Debug.Log("--- Testing Player Movement ---");
        
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player == null)
        {
            Debug.LogError("❌ FAIL: No PlayerController2D found");
            return false;
        }
        
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("❌ FAIL: Player has no Rigidbody2D");
            return false;
        }
        
        if (rb.constraints != RigidbodyConstraints2D.FreezeRotation)
        {
            Debug.LogError($"❌ FAIL: Player constraints wrong: {rb.constraints}");
            return false;
        }
        
        if (rb.gravityScale < 3.0f)
        {
            Debug.LogWarning($"⚠️ WARN: Low gravity scale: {rb.gravityScale}");
        }
        
        Debug.Log("✅ PASS: Player movement system ready");
        return true;
    }

    private bool CheckHealthSystem()
    {
        Debug.Log("--- Testing Health System ---");
        
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("❌ FAIL: No PlayerHealth component found");
            return false;
        }
        
        if (playerHealth.MaxHP <= 0)
        {
            Debug.LogError("❌ FAIL: Player max HP is 0 or negative");
            return false;
        }
        
        if (playerHealth.CurrentHP != playerHealth.MaxHP)
        {
            Debug.LogWarning($"⚠️ WARN: Player not at full health: {playerHealth.CurrentHP}/{playerHealth.MaxHP}");
        }
        
        Debug.Log("✅ PASS: Health system ready");
        return true;
    }

    private bool CheckCheckpointSystem()
    {
        Debug.Log("--- Testing Checkpoint System ---");
        
        var checkpoints = FindObjectsByType<CheckpointTrigger>(FindObjectsSortMode.None);
        if (checkpoints.Length == 0)
        {
            Debug.LogError("❌ FAIL: No checkpoints found in scene");
            return false;
        }
        
        if (checkpoints.Length < 2)
        {
            Debug.LogWarning($"⚠️ WARN: Only {checkpoints.Length} checkpoint(s) found, consider adding more");
        }
        
        foreach (var checkpoint in checkpoints)
        {
            var collider = checkpoint.GetComponent<Collider2D>();
            if (collider == null || !collider.isTrigger)
            {
                Debug.LogError($"❌ FAIL: Checkpoint {checkpoint.name} missing trigger collider");
                return false;
            }
        }
        
        Debug.Log($"✅ PASS: {checkpoints.Length} checkpoint(s) ready");
        return true;
    }

    private bool CheckHazardSystem()
    {
        Debug.Log("--- Testing Hazard System ---");
        
        var hazards = FindObjectsByType<DamageOnTouch>(FindObjectsSortMode.None);
        if (hazards.Length == 0)
        {
            Debug.LogWarning("⚠️ WARN: No hazards found - level might be too easy");
            return true; // Not a failure, just warning
        }
        
        foreach (var hazard in hazards)
        {
            var collider = hazard.GetComponent<Collider2D>();
            if (collider == null || !collider.isTrigger)
            {
                Debug.LogError($"❌ FAIL: Hazard {hazard.name} missing trigger collider");
                return false;
            }
        }
        
        Debug.Log($"✅ PASS: {hazards.Length} hazard(s) ready");
        return true;
    }

    private bool CheckUISystem()
    {
        Debug.Log("--- Testing UI System ---");
        
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ FAIL: No Canvas found");
            return false;
        }
        
        var healthUI = FindFirstObjectByType<HealthUI>();
        if (healthUI == null)
        {
            Debug.LogError("❌ FAIL: No HealthUI found");
            return false;
        }
        
        Debug.Log("✅ PASS: UI system ready");
        return true;
    }

    private bool CheckCameraSystem()
    {
        Debug.Log("--- Testing Camera System ---");
        
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("❌ FAIL: No main camera found");
            return false;
        }
        
        var cameraFollow = mainCamera.GetComponent<CameraFollow2D>();
        if (cameraFollow == null)
        {
            Debug.LogError("❌ FAIL: Main camera missing CameraFollow2D");
            return false;
        }
        
        if (!mainCamera.orthographic)
        {
            Debug.LogWarning("⚠️ WARN: Camera not set to orthographic");
        }
        
        Debug.Log("✅ PASS: Camera system ready");
        return true;
    }

    private bool CheckSceneWiring()
    {
        Debug.Log("--- Testing Scene Wiring ---");
        
        var player = FindFirstObjectByType<PlayerController2D>();
        var cameraFollow = FindFirstObjectByType<CameraFollow2D>();
        
        if (player == null || cameraFollow == null)
        {
            Debug.LogError("❌ FAIL: Missing core components for wiring check");
            return false;
        }
        
        // Check if camera has bounds configured
        var cameraBounds = FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None);
        bool hasCameraBounds = false;
        foreach (var bounds in cameraBounds)
        {
            if (bounds.isTrigger && bounds.gameObject != player.gameObject)
            {
                hasCameraBounds = true;
                break;
            }
        }
        
        if (!hasCameraBounds)
        {
            Debug.LogWarning("⚠️ WARN: No camera bounds found");
        }
        
        Debug.Log("✅ PASS: Scene wiring ready");
        return true;
    }

    [ContextMenu("Quick Play Test")]
    public void QuickPlayTest()
    {
        Debug.Log("=== QUICK PLAY TEST ===");
        Debug.Log("🎮 Controls:");
        Debug.Log("  WASD / Arrow Keys - Move");
        Debug.Log("  Space / W / Up - Jump");
        Debug.Log("🎯 Test checklist:");
        Debug.Log("  1. Player moves left/right ✓");
        Debug.Log("  2. Player jumps ✓");
        Debug.Log("  3. Player takes damage from hazards ✓");
        Debug.Log("  4. Health UI updates ✓");
        Debug.Log("  5. Checkpoints work ✓");
        Debug.Log("  6. Player respawns at checkpoint when died ✓");
        Debug.Log("  7. Camera follows player ✓");
        Debug.Log("Press Play and test these features!");
    }
}