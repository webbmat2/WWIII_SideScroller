using UnityEngine;

/// <summary>
/// Simple quick start script to immediately add WWIII Chapter System to your existing scene
/// </summary>
public class WWIII_QuickStart : MonoBehaviour
{
    [Header("🚀 WWIII Chapter System Quick Start")]
    [SerializeField] private bool setupOnStart = false;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupChapterSystem();
        }
    }
    
    [ContextMenu("🚀 Setup WWIII Chapter System NOW")]
    public void SetupChapterSystem()
    {
        Debug.Log("🚀 WWIII Quick Start - Setting up chapter system...");
        
        // 1. Add ChapterManager
        if (FindFirstObjectByType<ChapterManager>() == null)
        {
            new GameObject("ChapterManager").AddComponent<ChapterManager>();
            Debug.Log("✅ Added ChapterManager");
        }
        
        // 2. Update Player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.GetComponent<PlayerController>() == null)
                player.AddComponent<PlayerController>();
            if (player.GetComponent<PlayerMovement>() == null)
                player.AddComponent<PlayerMovement>();
            if (player.GetComponent<PlayerHealth>() == null)
                player.AddComponent<PlayerHealth>();
            if (player.GetComponent<PlayerAbilities>() == null)
                player.AddComponent<PlayerAbilities>();
            
            Debug.Log("✅ Enhanced Player with WWIII components");
        }
        
        // 3. Add GameHUD
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null && canvas.GetComponentInChildren<GameHUD>() == null)
        {
            var hudGO = new GameObject("GameHUD");
            hudGO.transform.SetParent(canvas.transform, false);
            hudGO.AddComponent<GameHUD>();
            Debug.Log("✅ Added GameHUD to Canvas");
        }
        
        // 4. Setup Camera
        var camera = Camera.main;
        if (camera != null)
        {
            if (!camera.orthographic)
            {
                camera.orthographic = true;
                camera.orthographicSize = 6f;
            }
            if (camera.GetComponent<CameraFollow2D>() == null)
                camera.gameObject.AddComponent<CameraFollow2D>();
            Debug.Log("✅ Configured Camera for 2D side-scrolling");
        }
        
        Debug.Log("🎮 SETUP COMPLETE!");
        Debug.Log("Press Play and test with WASD/Arrows to move, Space to jump, X for abilities!");
    }
    
    [ContextMenu("🔧 Test Hose Ability")]
    public void TestHoseAbility()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var abilities = player?.GetComponent<PlayerAbilities>();
        if (abilities != null)
        {
            abilities.SetCurrentAbility(PowerUpType.Hose);
            Debug.Log("🔧 Granted Hose ability! Press X to use it!");
        }
    }
    
    [ContextMenu("🎯 Create Chapter Data Assets")]
    public void CreateChapterAssets()
    {
        Debug.Log("📋 Creating Chapter Data Assets...");
        
        // This would normally use the editor script, but for now just log
        Debug.Log("Use Window menu: WWIII/Create Chapter Data Assets");
        Debug.Log("Or find ChapterDataCreator script and use its methods");
    }
}