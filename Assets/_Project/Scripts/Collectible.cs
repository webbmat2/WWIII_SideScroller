using UnityEngine;

[AddComponentMenu("Gameplay/Collectible")]
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private bool autoConfigureFromChapter = true;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 2f;
    
    private Vector3 startPosition;
    private ChapterManager chapterManager;
    private SpriteRenderer spriteRenderer;
    private bool collected = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        startPosition = transform.position;
    }

    private void Start()
    {
        chapterManager = ChapterManager.Instance;
        
        if (autoConfigureFromChapter && chapterManager?.CurrentChapter != null)
        {
            ConfigureFromChapter(chapterManager.CurrentChapter);
        }
    }

    private void Update()
    {
        if (!collected)
        {
            // Bob animation
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * bobOffset;
        }
    }

    private void ConfigureFromChapter(ChapterData chapter)
    {
        if (chapter.collectibleSprite != null)
        {
            spriteRenderer.sprite = chapter.collectibleSprite;
        }
        else
        {
            // Create default sprite based on chapter
            spriteRenderer.sprite = CreateDefaultCollectibleSprite(chapter);
        }
        
        Debug.Log($"Collectible configured for chapter: {chapter.title} ({chapter.collectibleName})");
    }

    private Sprite CreateDefaultCollectibleSprite(ChapterData chapter)
    {
        Color collectibleColor = Color.yellow; // Default
        
        // Different colors for different chapters
        switch (chapter.chapterId)
        {
            case "meadowbrook-park":
                collectibleColor = Color.yellow; // Golden Fried ush signs
                break;
            case "torch-lake":
                collectibleColor = Color.cyan; // Lake stones
                break;
            case "notre-dame":
                collectibleColor = Color.blue; // Campus items
                break;
            case "high-school":
                collectibleColor = Color.red; // School memorabilia
                break;
            case "philadelphia":
                collectibleColor = Color.white; // Poker chips
                break;
            case "parsons-chicken":
                collectibleColor = Color.orange; // Restaurant items
                break;
            case "costa-rica":
                collectibleColor = Color.green; // Jungle artifacts
                break;
        }
        
        return CreateSpriteWithColor(collectibleColor);
    }

    private Sprite CreateSpriteWithColor(Color color)
    {
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        
        // Create a simple circle pattern
        Vector2 center = new Vector2(8f, 8f);
        float radius = 6f;
        
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Vector2 pixel = new Vector2(x, y);
                float distance = Vector2.Distance(pixel, center);
                
                if (distance <= radius)
                {
                    pixels[y * 16 + x] = color;
                }
                else
                {
                    pixels[y * 16 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        collected = true;
        
        // Notify chapter manager
        if (chapterManager != null)
        {
            chapterManager.AddCollectible();
        }
        
        // Backward compatibility with CollectibleManager
        try
        {
            CollectibleManager.Add(1);
        }
        catch (System.Exception)
        {
            // CollectibleManager might not exist, ignore
        }
        
        // Play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Spawn effect
        if (collectEffect != null)
        {
            var effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        Debug.Log($"Collectible collected! ({chapterManager?.CollectiblesFound}/{chapterManager?.MaxCollectibles})");
        
        // Destroy collectible
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = collected ? Color.gray : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}