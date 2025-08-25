using UnityEngine;
using DG.Tweening;

/// <summary>
/// Collectible memories that represent important moments in James's life
/// </summary>
public class MemoryCollectable : MonoBehaviour
{
    [System.Serializable]
    public class Memory
    {
        public string title;
        public string description;
        public int year;
        public Sprite memoryIcon;
        public Color memoryColor = Color.white;
        public AudioClip memorySound;
    }

    [Header("Memory Data")]
    public Memory memoryData;
    
    [Header("Visual Effects")]
    public ParticleSystem sparkles;
    public SpriteRenderer memoryRenderer;
    
    [Header("Audio")]
    public AudioSource audioSource;

    private bool collected = false;
    private LifeStageManager lifeStageManager;

    private void Start()
    {
        lifeStageManager = FindFirstObjectByType<LifeStageManager>();
        
        // Set up visual appearance
        if (memoryRenderer != null && memoryData.memoryIcon != null)
        {
            memoryRenderer.sprite = memoryData.memoryIcon;
            memoryRenderer.color = memoryData.memoryColor;
        }

        // Gentle floating animation
        DOTweenHelper.PulseLoop(transform, 1.1f, 2f);
        
        // Add subtle glow effect
        if (sparkles != null)
        {
            var main = sparkles.main;
            main.startColor = memoryData.memoryColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        if (other.CompareTag("Player"))
        {
            CollectMemory();
        }
    }

    private void CollectMemory()
    {
        collected = true;
        
        Debug.Log($"📸 Memory Collected: {memoryData.title} ({memoryData.year})");
        
        // Visual feedback
        transform.DOScale(0f, 0.5f).SetEase(Ease.InBack);
        
        if (sparkles != null)
        {
            sparkles.Stop();
            sparkles.Clear();
        }

        // Audio feedback
        if (audioSource != null && memoryData.memorySound != null)
        {
            audioSource.PlayOneShot(memoryData.memorySound);
        }

        // Trigger milestone in life stage manager
        if (lifeStageManager != null)
        {
            lifeStageManager.TriggerMilestone(memoryData.title);
        }

        // Add to memory collection
        var memoryManager = FindFirstObjectByType<MemoryManager>();
        if (memoryManager != null)
        {
            memoryManager.AddMemory(memoryData);
        }

        // Destroy after animation
        Destroy(gameObject, 0.6f);
    }

    /// <summary>
    /// Create a memory collectable at runtime
    /// </summary>
    public static GameObject CreateMemory(Vector3 position, Memory memoryData)
    {
        var memoryGO = new GameObject($"Memory_{memoryData.title}");
        memoryGO.transform.position = position;
        
        // Add components
        var collider = memoryGO.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        var renderer = memoryGO.AddComponent<SpriteRenderer>();
        renderer.sprite = memoryData.memoryIcon;
        renderer.color = memoryData.memoryColor;
        
        var memoryComponent = memoryGO.AddComponent<MemoryCollectable>();
        memoryComponent.memoryData = memoryData;
        memoryComponent.memoryRenderer = renderer;
        
        return memoryGO;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw memory collection area
        Gizmos.color = memoryData.memoryColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw memory info
        UnityEditor.Handles.Label(transform.position + Vector3.up, 
                                $"{memoryData.title}\n{memoryData.year}");
    }
#endif
}