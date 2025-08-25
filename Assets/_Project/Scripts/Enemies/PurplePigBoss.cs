using UnityEngine;

[AddComponentMenu("Enemies/Purple Pig Boss")]
public class PurplePigBoss : MonoBehaviour, IDamageable
{
    [Header("Boss Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float humanVulnerableTime = 2f;
    [SerializeField] private float pigMovementSpeed = 5f;
    [SerializeField] private float scatterForce = 8f;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer bossRenderer;
    [SerializeField] private Color humanColor = Color.white;
    [SerializeField] private Color pigColor = new Color(0.8f, 0.4f, 0.8f); // Purple
    
    [Header("Matt NPC")]
    [SerializeField] private Transform mattNPC;
    [SerializeField] private float mattGrabRange = 1.5f;
    
    public enum BossState
    {
        Pig,        // Invulnerable, mobile
        Human       // Vulnerable when grabbed by Matt
    }
    
    private BossState currentState = BossState.Pig;
    private int currentHealth;
    private float vulnerableTimer = 0f;
    private bool isGrabbedByMatt = false;
    private Rigidbody2D rb;
    private Vector3 originalScale;
    
    public BossState CurrentState => currentState;
    public int CurrentHealth => currentHealth;
    
    public System.Action<BossState> OnStateChanged;
    public System.Action OnBossDefeated;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        if (bossRenderer == null)
        {
            bossRenderer = GetComponent<SpriteRenderer>();
        }
        
        originalScale = transform.localScale;
        currentHealth = maxHealth;
        
        SetupMattNPC();
        SetState(BossState.Pig);
    }

    private void SetupMattNPC()
    {
        if (mattNPC == null)
        {
            // Create Matt NPC at screen edge
            var mattGO = new GameObject("MattNPC");
            mattNPC = mattGO.transform;
            
            // Position at left edge of screen (or wherever appropriate)
            var camera = Camera.main;
            if (camera != null)
            {
                Vector3 screenEdge = camera.ScreenToWorldPoint(new Vector3(100f, Screen.height * 0.5f, camera.nearClipPlane));
                screenEdge.z = 0f;
                mattNPC.position = screenEdge;
            }
            else
            {
                mattNPC.position = transform.position + Vector3.left * 10f;
            }
            
            // Add a simple visual for Matt
            var mattRenderer = mattGO.AddComponent<SpriteRenderer>();
            mattRenderer.sprite = CreateSimpleSprite(Color.green);
            mattRenderer.sortingOrder = 1;
            
            Debug.Log($"Created Matt NPC at {mattNPC.position}");
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case BossState.Pig:
                UpdatePigBehavior();
                break;
            case BossState.Human:
                UpdateHumanBehavior();
                break;
        }
        
        CheckMattGrab();
    }

    private void UpdatePigBehavior()
    {
        // Simple pig movement - move around randomly
        if (Time.time % 3f < 0.1f) // Change direction every 3 seconds
        {
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), 0f).normalized;
            rb.linearVelocity = randomDirection * pigMovementSpeed;
        }
    }

    private void UpdateHumanBehavior()
    {
        vulnerableTimer -= Time.deltaTime;
        
        if (vulnerableTimer <= 0f && !isGrabbedByMatt)
        {
            SetState(BossState.Pig);
            Scatter();
        }
    }

    private void CheckMattGrab()
    {
        if (mattNPC == null) return;
        
        float distanceToMatt = Vector3.Distance(transform.position, mattNPC.position);
        bool wasGrabbed = isGrabbedByMatt;
        isGrabbedByMatt = distanceToMatt <= mattGrabRange && currentState == BossState.Human;
        
        if (isGrabbedByMatt && !wasGrabbed)
        {
            Debug.Log("Matt is grabbing Kristen!");
            rb.linearVelocity = Vector2.zero; // Stop movement when grabbed
        }
    }

    public void OnHoseHit()
    {
        if (currentState == BossState.Pig && isGrabbedByMatt)
        {
            // Only vulnerable when Matt is grabbing and in human form
            if (mattNPC != null)
            {
                float distanceToMatt = Vector3.Distance(transform.position, mattNPC.position);
                if (distanceToMatt <= mattGrabRange)
                {
                    // Flip to human state
                    SetState(BossState.Human);
                    vulnerableTimer = humanVulnerableTime;
                    Debug.Log("Purple Pig hit by hose while grabbed by Matt - now vulnerable!");
                }
            }
        }
        else if (currentState == BossState.Human && isGrabbedByMatt)
        {
            // Take damage in human form
            TakeDamage(1);
        }
        else
        {
            Debug.Log("Hose hit Purple Pig but conditions not met for damage");
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState != BossState.Human || !isGrabbedByMatt)
        {
            Debug.Log("Purple Pig is invulnerable in pig form or when not grabbed!");
            return;
        }
        
        currentHealth -= damage;
        Debug.Log($"Purple Pig takes {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            DefeatBoss();
        }
        else
        {
            // Return to pig form after taking damage
            SetState(BossState.Pig);
            Scatter();
        }
    }

    private void SetState(BossState newState)
    {
        currentState = newState;
        UpdateVisual();
        OnStateChanged?.Invoke(currentState);
        
        Debug.Log($"Purple Pig state changed to: {currentState}");
    }

    private void UpdateVisual()
    {
        if (bossRenderer == null) return;
        
        switch (currentState)
        {
            case BossState.Pig:
                bossRenderer.color = pigColor;
                break;
            case BossState.Human:
                bossRenderer.color = humanColor;
                break;
        }
    }

    private void Scatter()
    {
        // Apply scatter force away from Matt
        if (mattNPC != null)
        {
            Vector3 scatterDirection = (transform.position - mattNPC.position).normalized;
            rb.AddForce(scatterDirection * scatterForce, ForceMode2D.Impulse);
        }
        else
        {
            // Random scatter if no Matt
            Vector2 randomScatter = Random.insideUnitCircle.normalized;
            rb.AddForce(randomScatter * scatterForce, ForceMode2D.Impulse);
        }
        
        Debug.Log("Purple Pig scattered!");
    }

    private void DefeatBoss()
    {
        Debug.Log("Purple Pig Boss defeated!");
        OnBossDefeated?.Invoke();
        
        // Trigger chapter completion
        var chapterManager = ChapterManager.Instance;
        if (chapterManager != null)
        {
            chapterManager.CompleteChapter();
        }
        
        // Destroy boss
        Destroy(gameObject, 1f);
    }

    private Sprite CreateSimpleSprite(Color color)
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw Matt grab range
        if (mattNPC != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(mattNPC.position, mattGrabRange);
            Gizmos.DrawLine(transform.position, mattNPC.position);
        }
        
        // Draw state indicator
        Gizmos.color = currentState == BossState.Human ? Color.white : Color.magenta;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
#endif
}