using UnityEngine;

[AddComponentMenu("Level Design/Level Setup Manager")]
public class LevelSetupManager : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool fixPlayerConstraintsOnStart = true;
    [SerializeField] private bool setupAudioManager = true;
    [SerializeField] private bool setupGameStateManager = true;
    [SerializeField] private bool initializeCoinSystem = true;

    [Header("References")]
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject gameStateManagerPrefab;
    [SerializeField] private GameObject cameraManagerPrefab;

    private void Start()
    {
        Debug.Log("=== LEVEL SETUP STARTING ===");
        
        if (fixPlayerConstraintsOnStart)
        {
            FixAllPlayerConstraints();
        }

        if (setupAudioManager)
        {
            EnsureAudioManager();
        }

        if (setupGameStateManager)
        {
            EnsureGameStateManager();
        }

        if (initializeCoinSystem)
        {
            InitializeCoinSystem();
        }

        SetupCameraSystem();
        
        Debug.Log("=== LEVEL SETUP COMPLETE ===");
    }

    [ContextMenu("Fix Player Constraints Now")]
    public void FixAllPlayerConstraints()
    {
        var players = FindObjectsByType<PlayerController2D>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                var oldConstraints = rb.constraints;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                Debug.Log($"Fixed Player {player.name}: {oldConstraints} → {rb.constraints}");
                
                // Also add the constraint fix component
                if (player.GetComponent<PlayerConstraintsFix>() == null)
                {
                    player.gameObject.AddComponent<PlayerConstraintsFix>();
                    Debug.Log($"Added PlayerConstraintsFix to {player.name}");
                }
            }
        }
    }

    private void EnsureAudioManager()
    {
        if (FindFirstObjectByType<AudioFXManager>() == null)
        {
            if (audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
                Debug.Log("Audio Manager created from prefab");
            }
            else
            {
                var audioManagerGO = new GameObject("Audio FX Manager");
                audioManagerGO.AddComponent<AudioFXManager>();
                Debug.Log("Audio Manager created manually");
            }
        }
    }

    private void EnsureGameStateManager()
    {
        if (FindFirstObjectByType<GameStateManager>() == null)
        {
            if (gameStateManagerPrefab != null)
            {
                Instantiate(gameStateManagerPrefab);
                Debug.Log("Game State Manager created from prefab");
            }
            else
            {
                var gameStateManagerGO = new GameObject("Game State Manager");
                gameStateManagerGO.AddComponent<GameStateManager>();
                Debug.Log("Game State Manager created manually");
            }
        }
    }

    private void InitializeCoinSystem()
    {
        CoinManager.Reset(0);
        Debug.Log("Coin system initialized");
    }

    private void SetupCameraSystem()
    {
        var cameraManager = FindFirstObjectByType<CameraManager>();
        if (cameraManager == null)
        {
            var player = FindFirstObjectByType<PlayerController2D>();
            if (player != null)
            {
                var cameraGO = new GameObject("Camera Manager");
                var cm = cameraGO.AddComponent<CameraManager>();
                Debug.Log("Camera Manager created and configured");
            }
        }
    }

    [ContextMenu("Validate Level Setup")]
    public void ValidateLevelSetup()
    {
        Debug.Log("=== LEVEL VALIDATION ===");

        // Check Player
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            bool playerOK = rb != null && rb.constraints == RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"✓ Player: {(playerOK ? "OK" : "NEEDS FIX")} - Constraints: {rb?.constraints}");
        }
        else
        {
            Debug.LogError("✗ No PlayerController2D found in scene!");
        }

        // Check Systems
        bool audioOK = FindFirstObjectByType<AudioFXManager>() != null;
        bool gameStateOK = FindFirstObjectByType<GameStateManager>() != null;
        bool coinSystemOK = true; // CoinManager is static

        Debug.Log($"✓ Audio Manager: {(audioOK ? "OK" : "MISSING")}");
        Debug.Log($"✓ Game State Manager: {(gameStateOK ? "OK" : "MISSING")}");
        Debug.Log($"✓ Coin System: {(coinSystemOK ? "OK" : "MISSING")}");

        // Check Level Elements
        var checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        var exits = FindObjectsByType<LevelExit>(FindObjectsSortMode.None);
        var coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);

        Debug.Log($"✓ Checkpoints: {checkpoints.Length}");
        Debug.Log($"✓ Level Exits: {exits.Length}");
        Debug.Log($"✓ Coins: {coins.Length}");

        Debug.Log("=== VALIDATION COMPLETE ===");
    }

    [ContextMenu("Setup Example Level")]
    public void SetupExampleLevel()
    {
        var levelBuilder = GetComponent<LevelBuilder>();
        if (levelBuilder == null)
        {
            levelBuilder = gameObject.AddComponent<LevelBuilder>();
        }

        Vector3 startPos = Vector3.zero;

        // Beat 1: Safe spawn
        levelBuilder.CreateGround(startPos);
        
        // Beat 2: Jump primer
        Vector3 jumpPos = startPos + Vector3.right * 5f;
        levelBuilder.CreateGround(jumpPos);
        levelBuilder.CreateGround(jumpPos + Vector3.right * 3f); // 3-tile gap
        
        // Beat 3: First spikes
        Vector3 spikePos = jumpPos + Vector3.right * 8f;
        levelBuilder.CreateHazardLane(spikePos, 2, true);
        
        Debug.Log("Example level layout created!");
    }
}