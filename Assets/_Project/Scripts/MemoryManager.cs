using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the collection and display of James's life memories
/// </summary>
public class MemoryManager : MonoBehaviour
{
    [Header("Memory Collection")]
    public List<MemoryCollectable.Memory> collectedMemories = new List<MemoryCollectable.Memory>();
    
    [Header("UI References")]
    public GameObject memoryBookUI;
    public Transform memoryGrid;
    public GameObject memoryEntryPrefab;
    
    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip memoryCollectSound;
    public AudioClip memoryBookOpenSound;

    private static MemoryManager instance;
    public static MemoryManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Add a new memory to the collection
    /// </summary>
    public void AddMemory(MemoryCollectable.Memory memory)
    {
        if (IsMemoryCollected(memory.title)) return;
        
        collectedMemories.Add(memory);
        
        Debug.Log($"📚 Memory added to collection: {memory.title}");
        
        // Audio feedback
        if (uiAudioSource != null && memoryCollectSound != null)
        {
            uiAudioSource.PlayOneShot(memoryCollectSound);
        }
        
        // Update UI if memory book is open
        RefreshMemoryBookUI();
        
        // Check for completion milestones
        CheckMemoryMilestones();
    }

    /// <summary>
    /// Check if a memory has already been collected
    /// </summary>
    public bool IsMemoryCollected(string memoryTitle)
    {
        return collectedMemories.Exists(m => m.title == memoryTitle);
    }

    /// <summary>
    /// Get memories from a specific decade
    /// </summary>
    public List<MemoryCollectable.Memory> GetMemoriesFromDecade(string decade)
    {
        return collectedMemories.FindAll(m => GetDecadeFromYear(m.year) == decade);
    }

    /// <summary>
    /// Get total number of memories collected
    /// </summary>
    public int GetTotalMemoriesCollected() => collectedMemories.Count;

    /// <summary>
    /// Open the memory book UI
    /// </summary>
    public void OpenMemoryBook()
    {
        if (memoryBookUI != null)
        {
            memoryBookUI.SetActive(true);
            RefreshMemoryBookUI();
            
            // Audio feedback
            if (uiAudioSource != null && memoryBookOpenSound != null)
            {
                uiAudioSource.PlayOneShot(memoryBookOpenSound);
            }
        }
    }

    /// <summary>
    /// Close the memory book UI
    /// </summary>
    public void CloseMemoryBook()
    {
        if (memoryBookUI != null)
        {
            memoryBookUI.SetActive(false);
        }
    }

    /// <summary>
    /// Refresh the memory book UI display
    /// </summary>
    private void RefreshMemoryBookUI()
    {
        if (memoryGrid == null || memoryEntryPrefab == null) return;
        
        // Clear existing entries
        foreach (Transform child in memoryGrid)
        {
            Destroy(child.gameObject);
        }
        
        // Create entries for collected memories
        foreach (var memory in collectedMemories)
        {
            CreateMemoryEntry(memory);
        }
    }

    /// <summary>
    /// Create a UI entry for a memory
    /// </summary>
    private void CreateMemoryEntry(MemoryCollectable.Memory memory)
    {
        var entryGO = Instantiate(memoryEntryPrefab, memoryGrid);
        
        // Set up the entry (assuming it has specific child components)
        var titleText = entryGO.transform.Find("Title")?.GetComponent<TMPro.TextMeshProUGUI>();
        var descText = entryGO.transform.Find("Description")?.GetComponent<TMPro.TextMeshProUGUI>();
        var yearText = entryGO.transform.Find("Year")?.GetComponent<TMPro.TextMeshProUGUI>();
        var iconImage = entryGO.transform.Find("Icon")?.GetComponent<UnityEngine.UI.Image>();
        
        if (titleText != null) titleText.text = memory.title;
        if (descText != null) descText.text = memory.description;
        if (yearText != null) yearText.text = memory.year.ToString();
        if (iconImage != null && memory.memoryIcon != null) 
        {
            iconImage.sprite = memory.memoryIcon;
            iconImage.color = memory.memoryColor;
        }
    }

    /// <summary>
    /// Check for memory collection milestones
    /// </summary>
    private void CheckMemoryMilestones()
    {
        int totalMemories = GetTotalMemoriesCollected();
        
        // Define milestone thresholds
        if (totalMemories == 5)
        {
            Debug.Log("🏆 Milestone: First Memories - Collected 5 memories!");
        }
        else if (totalMemories == 15)
        {
            Debug.Log("🏆 Milestone: Memory Keeper - Collected 15 memories!");
        }
        else if (totalMemories == 30)
        {
            Debug.Log("🏆 Milestone: Life Chronicler - Collected 30 memories!");
        }
        else if (totalMemories == 50)
        {
            Debug.Log("🏆 Milestone: Master Archivist - Collected all memories!");
        }
    }

    /// <summary>
    /// Get the decade string from a year
    /// </summary>
    private string GetDecadeFromYear(int year)
    {
        if (year >= 1980 && year < 1990) return "1980s";
        if (year >= 1990 && year < 2000) return "1990s";
        if (year >= 2000 && year < 2010) return "2000s";
        if (year >= 2010 && year < 2020) return "2010s";
        if (year >= 2020 && year < 2030) return "2020s";
        return "Unknown";
    }

    /// <summary>
    /// Save memories to PlayerPrefs (simple save system)
    /// </summary>
    public void SaveMemories()
    {
        PlayerPrefs.SetInt("MemoryCount", collectedMemories.Count);
        
        for (int i = 0; i < collectedMemories.Count; i++)
        {
            var memory = collectedMemories[i];
            PlayerPrefs.SetString($"Memory_{i}_Title", memory.title);
            PlayerPrefs.SetString($"Memory_{i}_Description", memory.description);
            PlayerPrefs.SetInt($"Memory_{i}_Year", memory.year);
        }
        
        PlayerPrefs.Save();
        Debug.Log($"💾 Saved {collectedMemories.Count} memories");
    }

    /// <summary>
    /// Load memories from PlayerPrefs
    /// </summary>
    public void LoadMemories()
    {
        collectedMemories.Clear();
        
        int memoryCount = PlayerPrefs.GetInt("MemoryCount", 0);
        
        for (int i = 0; i < memoryCount; i++)
        {
            var memory = new MemoryCollectable.Memory
            {
                title = PlayerPrefs.GetString($"Memory_{i}_Title", ""),
                description = PlayerPrefs.GetString($"Memory_{i}_Description", ""),
                year = PlayerPrefs.GetInt($"Memory_{i}_Year", 1985)
            };
            
            if (!string.IsNullOrEmpty(memory.title))
            {
                collectedMemories.Add(memory);
            }
        }
        
        Debug.Log($"📖 Loaded {collectedMemories.Count} memories");
    }

#if UNITY_EDITOR
    [Header("Debug")]
    [UnityEngine.Space]
    public bool debugShowMemoryBook;
    
    private void OnValidate()
    {
        if (debugShowMemoryBook)
        {
            debugShowMemoryBook = false;
            if (Application.isPlaying) OpenMemoryBook();
        }
    }
#endif
}