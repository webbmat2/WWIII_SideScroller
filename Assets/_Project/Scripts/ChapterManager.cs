using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

[AddComponentMenu("WWIII/Chapter Manager")]
public class ChapterManager : MonoBehaviour
{
    [Header("Chapter Configuration")]
    [SerializeField] private ChapterData[] allChapters;
    [SerializeField] private bool loadFirstChapterOnStart = true;
    
    [Header("Current State")]
    [SerializeField] private ChapterData currentChapter;
    [SerializeField] private int collectiblesFound = 0;
    
    private static ChapterManager instance;
    public static ChapterManager Instance 
    { 
        get 
        { 
            if (instance == null)
                instance = FindFirstObjectByType<ChapterManager>();
            return instance;
        } 
    }

    public ChapterData CurrentChapter => currentChapter;
    public int CollectiblesFound => collectiblesFound;
    public int MaxCollectibles => currentChapter ? currentChapter.maxCollectibles : 5;

    public System.Action<ChapterData> OnChapterLoaded;
    public System.Action<int, int> OnCollectibleChanged; // found, max
    public System.Action OnChapterCompleted;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ValidateChapterData();
    }

    private void Start()
    {
        if (loadFirstChapterOnStart && allChapters.Length > 0)
        {
            LoadChapter(allChapters[0].chapterId);
        }
    }

    private void ValidateChapterData()
    {
        if (allChapters == null || allChapters.Length == 0)
        {
            Debug.LogError("ChapterManager: No chapters configured!");
            return;
        }

        // Check for duplicate IDs
        var ids = allChapters.Select(c => c.chapterId).ToList();
        var duplicates = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
        
        foreach (var duplicate in duplicates)
        {
            Debug.LogError($"ChapterManager: Duplicate chapter ID found: {duplicate}");
        }

        Debug.Log($"ChapterManager: Loaded {allChapters.Length} chapters");
    }

    public bool LoadChapter(string chapterId)
    {
        var chapter = GetChapterData(chapterId);
        if (chapter == null)
        {
            Debug.LogError($"ChapterManager: Chapter '{chapterId}' not found!");
            return false;
        }

        currentChapter = chapter;
        collectiblesFound = 0;
        
        Debug.Log($"Loading Chapter: {chapter.title} ({chapter.location})");
        
        OnChapterLoaded?.Invoke(chapter);
        OnCollectibleChanged?.Invoke(collectiblesFound, MaxCollectibles);

        // Load the scene if specified
        if (!string.IsNullOrEmpty(chapter.sceneName))
        {
            SceneManager.LoadScene(chapter.sceneName);
        }

        return true;
    }

    public ChapterData GetChapterData(string chapterId)
    {
        return allChapters.FirstOrDefault(c => c.chapterId == chapterId);
    }

    public ChapterData[] GetAllChapters()
    {
        return allChapters;
    }

    public void AddCollectible()
    {
        if (collectiblesFound < MaxCollectibles)
        {
            collectiblesFound++;
            OnCollectibleChanged?.Invoke(collectiblesFound, MaxCollectibles);
            
            Debug.Log($"Collectible found! {collectiblesFound}/{MaxCollectibles}");
            
            if (collectiblesFound >= MaxCollectibles)
            {
                Debug.Log("All collectibles found in chapter!");
            }
        }
    }

    public void CompleteChapter()
    {
        if (currentChapter == null) return;

        Debug.Log($"Chapter completed: {currentChapter.title}");
        currentChapter.isCompleted = true;
        
        OnChapterCompleted?.Invoke();

        // Auto-load next chapter if specified
        if (!string.IsNullOrEmpty(currentChapter.nextChapterId))
        {
            LoadChapter(currentChapter.nextChapterId);
        }
    }

    public bool IsChapterUnlocked(string chapterId)
    {
        var chapter = GetChapterData(chapterId);
        return chapter != null && chapter.isUnlocked;
    }

    public void UnlockChapter(string chapterId)
    {
        var chapter = GetChapterData(chapterId);
        if (chapter != null)
        {
            chapter.isUnlocked = true;
            Debug.Log($"Chapter unlocked: {chapter.title}");
        }
    }

    [ContextMenu("Load Next Chapter")]
    public void LoadNextChapter()
    {
        if (currentChapter != null && !string.IsNullOrEmpty(currentChapter.nextChapterId))
        {
            LoadChapter(currentChapter.nextChapterId);
        }
    }

    [ContextMenu("Restart Current Chapter")]
    public void RestartCurrentChapter()
    {
        if (currentChapter != null)
        {
            LoadChapter(currentChapter.chapterId);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Create Missing Chapter Assets")]
    public void CreateMissingChapterAssets()
    {
        string[] chapterIds = {
            "meadowbrook-park", "torch-lake", "notre-dame", 
            "high-school", "philadelphia", "parsons-chicken", "costa-rica"
        };

        foreach (var id in chapterIds)
        {
            if (GetChapterData(id) == null)
            {
                Debug.Log($"Need to create chapter asset for: {id}");
            }
        }
    }
#endif
}