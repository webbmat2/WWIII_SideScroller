using UnityEngine;

/// <summary>
/// Quick setup utility to integrate WWIII Chapter System into existing scenes
/// </summary>
[AddComponentMenu("WWIII/Quick Setup")]
public class QuickSetup : MonoBehaviour
{
    [ContextMenu("🚀 Quick Setup - Add WWIII Chapter System")]
    public void SetupWWIIIChapterSystem()
    {
        Debug.Log("🚀 Setting up WWIII Chapter System...");
        
        // Step 1: Add ChapterManager
        AddChapterManager();
        
        // Step 2: Update Player
        UpdatePlayer();
        
        // Step 3: Add GameHUD
        AddGameHUD();
        
        // Step 4: Configure as Meadowbrook Park
        ConfigureMeadowbrookPark();
        
        // Step 5: Final validation
        RunValidation();
        
        Debug.Log("✅ WWIII Chapter System setup complete!");
        Debug.Log("🎮 Press Play to test! Use WASD/Arrows to move, Space to jump, X for abilities");
    }
    
    private void AddChapterManager()
    {
        var existing = FindFirstObjectByType<ChapterManager>();
        if (existing == null)
        {
            var go = new GameObject("ChapterManager");
            go.AddComponent<ChapterManager>();
            Debug.Log("✅ Added ChapterManager");
        }
    }
    
    private void UpdatePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Add new components alongside existing PlayerController2D
            if (player.GetComponent<PlayerController>() == null)
                player.AddComponent<PlayerController>();
            if (player.GetComponent<PlayerMovement>() == null)
                player.AddComponent<PlayerMovement>();
            if (player.GetComponent<PlayerHealth>() == null)
                player.AddComponent<PlayerHealth>();
            if (player.GetComponent<PlayerAbilities>() == null)
                player.AddComponent<PlayerAbilities>();
            
            Debug.Log("✅ Updated Player with WWIII components");
        }
    }
    
    private void AddGameHUD()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            var existing = canvas.GetComponentInChildren<GameHUD>();
            if (existing == null)
            {
                var hudGO = new GameObject("GameHUD");
                hudGO.transform.SetParent(canvas.transform, false);
                hudGO.AddComponent<GameHUD>();
                Debug.Log("✅ Added GameHUD");
            }
        }
    }
    
    private void ConfigureMeadowbrookPark()
    {
        // Configure as first chapter (Meadowbrook Park)
        var chapterManager = FindFirstObjectByType<ChapterManager>();
        if (chapterManager != null)
        {
            chapterManager.LoadChapter("meadowbrook-park");
            Debug.Log("✅ Configured as Meadowbrook Park chapter");
        }
    }
    
    private void RunValidation()
    {
        // Add validation component
        var validator = FindFirstObjectByType<WWIII_Validator>();
        if (validator == null)
        {
            var go = new GameObject("WWIII_Validator");
            validator = go.AddComponent<WWIII_Validator>();
        }
        
        validator.RunFullValidation();
    }
}