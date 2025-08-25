using UnityEngine;

[AddComponentMenu("WWIII/Scene Integrator")]
public class SceneIntegrator : MonoBehaviour
{
    [Header("Integration Options")]
    [SerializeField] private bool integrateOnStart = true;
    [SerializeField] private bool updateExistingPlayer = true;
    [SerializeField] private bool addChapterSystem = true;
    [SerializeField] private bool configureAsNorthville = true;

    private void Start()
    {
        if (integrateOnStart)
        {
            IntegrateWWIIIChapterSystem();
        }
    }

    [ContextMenu("Integrate WWIII Chapter System")]
    public void IntegrateWWIIIChapterSystem()
    {
        Debug.Log("🔄 Integrating WWIII Chapter System into existing scene...");

        if (addChapterSystem)
        {
            AddChapterManagerToScene();
        }

        if (updateExistingPlayer)
        {
            UpdatePlayerForChapterSystem();
        }

        if (configureAsNorthville)
        {
            ConfigureAsNorthvilleChapter();
        }

        UpdateUIForChapterSystem();
        ValidateIntegration();

        Debug.Log("✅ WWIII Chapter System integration complete!");
    }

    private void AddChapterManagerToScene()
    {
        var existingChapterManager = FindFirstObjectByType<ChapterManager>();
        if (existingChapterManager == null)
        {
            var chapterManagerGO = new GameObject("ChapterManager");
            var chapterManager = chapterManagerGO.AddComponent<ChapterManager>();
            Debug.Log("✅ Added ChapterManager to scene");
        }
        else
        {
            Debug.Log("ℹ️ ChapterManager already exists");
        }
    }

    private void UpdatePlayerForChapterSystem()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ No Player found with 'Player' tag");
            return;
        }

        Debug.Log($"🎮 Updating player: {player.name}");

        // Add new player components if they don't exist
        var playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = player.AddComponent<PlayerController>();
            Debug.Log("✅ Added PlayerController");
        }

        var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = player.AddComponent<PlayerMovement>();
            Debug.Log("✅ Added PlayerMovement");
        }

        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.AddComponent<PlayerHealth>();
            Debug.Log("✅ Added PlayerHealth");
        }

        var playerAbilities = player.GetComponent<PlayerAbilities>();
        if (playerAbilities == null)
        {
            playerAbilities = player.AddComponent<PlayerAbilities>();
            Debug.Log("✅ Added PlayerAbilities");
        }

        // Ensure proper physics setup
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3.8f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log("✅ Updated player physics settings");
        }

        // Set layer
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1) playerLayer = 0; // Default layer
        player.layer = playerLayer;
    }

    private void ConfigureAsNorthvilleChapter()
    {
        Debug.Log("🏞️ Configuring scene as Northville (Meadowbrook Park)...");

        // Add layer setup
        var layerSetup = FindFirstObjectByType<LayerSetup>();
        if (layerSetup == null)
        {
            var layerSetupGO = new GameObject("LayerSetup");
            layerSetup = layerSetupGO.AddComponent<LayerSetup>();
            Debug.Log("✅ Added LayerSetup component");
        }

        // Configure existing collectibles for chapter system
        ConfigureExistingCollectibles();

        // Add Purple Pig boss if this is truly Meadowbrook Park
        AddMeadowbrookParkElements();

        // Update camera for 2D side-scrolling
        ConfigureCameraFor2D();
    }

    private void ConfigureExistingCollectibles()
    {
        var existingCollectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        Debug.Log($"📦 Found {existingCollectibles.Length} existing collectibles");

        // If no Collectible components, check for objects that should be collectibles
        if (existingCollectibles.Length == 0)
        {
            // Look for objects named "Collectible" or "Coin" 
            var collectibleObjects = new GameObject[0];
            
            var coinObject = GameObject.Find("Coin");
            if (coinObject != null)
            {
                var collectible = coinObject.GetComponent<Collectible>();
                if (collectible == null)
                {
                    collectible = coinObject.AddComponent<Collectible>();
                    Debug.Log($"✅ Added Collectible component to {coinObject.name}");
                }
            }

            var collectibleObject = GameObject.Find("Collectible");
            if (collectibleObject != null)
            {
                var collectible = collectibleObject.GetComponent<Collectible>();
                if (collectible == null)
                {
                    collectible = collectibleObject.AddComponent<Collectible>();
                    Debug.Log($"✅ Added Collectible component to {collectibleObject.name}");
                }
            }
        }
    }

    private void AddMeadowbrookParkElements()
    {
        // Add Purple Pig boss if it doesn't exist
        var purplePig = FindFirstObjectByType<PurplePigBoss>();
        if (purplePig == null)
        {
            var purplePigGO = new GameObject("PurplePigBoss");
            purplePigGO.transform.position = new Vector3(8f, 2f, 0f);
            purplePigGO.AddComponent<PurplePigBoss>();
            var pigCollider = purplePigGO.AddComponent<BoxCollider2D>();
            pigCollider.size = new Vector2(1f, 1f);
            Debug.Log("✅ Added Purple Pig Boss");
        }

        // Add Slip-n-Slide gate if it doesn't exist
        var slipGate = FindFirstObjectByType<SlipNSlideGate>();
        if (slipGate == null)
        {
            var slipGateGO = new GameObject("SlipNSlideGate");
            slipGateGO.transform.position = new Vector3(0f, 1f, 0f);
            slipGateGO.AddComponent<SlipNSlideGate>();
            var gateCollider = slipGateGO.AddComponent<BoxCollider2D>();
            gateCollider.size = new Vector2(1f, 2f);
            gateCollider.isTrigger = false; // Starts solid
            Debug.Log("✅ Added Slip-n-Slide Gate");
        }
    }

    private void ConfigureCameraFor2D()
    {
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            if (!mainCamera.orthographic)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 6f;
                Debug.Log("✅ Set camera to orthographic");
            }

            var cameraFollow = mainCamera.GetComponent<CameraFollow2D>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow2D>();
                Debug.Log("✅ Added CameraFollow2D to main camera");
            }
        }
    }

    private void UpdateUIForChapterSystem()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            var gameHUD = canvas.GetComponentInChildren<GameHUD>();
            if (gameHUD == null)
            {
                var hudGO = new GameObject("GameHUD");
                hudGO.transform.SetParent(canvas.transform, false);
                gameHUD = hudGO.AddComponent<GameHUD>();
                Debug.Log("✅ Added GameHUD to existing Canvas");
            }
        }
    }

    private void ValidateIntegration()
    {
        Debug.Log("🔍 Validating integration...");

        var chapterManager = FindFirstObjectByType<ChapterManager>();
        var player = GameObject.FindGameObjectWithTag("Player");
        var playerController = player?.GetComponent<PlayerController>();
        var canvas = FindFirstObjectByType<Canvas>();
        var gameHUD = FindFirstObjectByType<GameHUD>();

        bool success = chapterManager != null && 
                      playerController != null && 
                      canvas != null && 
                      gameHUD != null;

        if (success)
        {
            Debug.Log("✅ Integration validation passed!");
            Debug.Log("🎮 Ready to test! Press Play and use WASD/Arrows to move, Space to jump, X for abilities");
        }
        else
        {
            Debug.LogWarning("⚠️ Integration validation found issues. Check individual components.");
        }
    }

    [ContextMenu("Load Meadowbrook Park Chapter")]
    public void LoadMeadowbrookParkChapter()
    {
        var chapterManager = FindFirstObjectByType<ChapterManager>();
        if (chapterManager != null)
        {
            chapterManager.LoadChapter("meadowbrook-park");
            Debug.Log("🏞️ Loaded Meadowbrook Park chapter data");
        }
        else
        {
            Debug.LogError("❌ No ChapterManager found. Run integration first.");
        }
    }

    [ContextMenu("Test Player Abilities")]
    public void TestPlayerAbilities()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var abilities = player?.GetComponent<PlayerAbilities>();
        
        if (abilities != null)
        {
            abilities.SetCurrentAbility(PowerUpType.Hose);
            Debug.Log("🔧 Granted Hose ability - test with X key!");
        }
        else
        {
            Debug.LogError("❌ No PlayerAbilities found on player");
        }
    }
}