using UnityEngine;

[AddComponentMenu("Environment/Slip-n-Slide Gate")]
[RequireComponent(typeof(Collider2D))]
public class SlipNSlideGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private bool isWet = false;
    [SerializeField] private float wetDuration = 10f;
    [SerializeField] private float slipForce = 5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer gateRenderer;
    [SerializeField] private Color dryColor = Color.brown;
    [SerializeField] private Color wetColor = Color.blue;
    [SerializeField] private ParticleSystem waterEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioClip wetSound;
    [SerializeField] private AudioClip slipSound;
    
    private Collider2D gateCollider;
    private float wetTimer = 0f;

    private void Awake()
    {
        gateCollider = GetComponent<Collider2D>();
        
        if (gateRenderer == null)
        {
            gateRenderer = GetComponent<SpriteRenderer>();
        }
        
        UpdateVisual();
    }

    private void Update()
    {
        if (isWet && wetTimer > 0f)
        {
            wetTimer -= Time.deltaTime;
            
            if (wetTimer <= 0f)
            {
                DryGate();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (isWet)
        {
            // Player can pass through when wet
            var playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Add slip force
                Vector3 slipDirection = transform.right; // Assume gate faces right
                playerRb.AddForce(slipDirection * slipForce, ForceMode2D.Impulse);
                
                // Play slip sound
                if (slipSound != null)
                {
                    AudioSource.PlayClipAtPoint(slipSound, transform.position);
                }
                
                Debug.Log("Player slipped through gate!");
            }
        }
        else
        {
            // Block player when dry
            Debug.Log("Gate is dry - player cannot pass!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isWet && collision.gameObject.CompareTag("Player"))
        {
            // Solid collision when dry
            Debug.Log("Player blocked by dry gate");
        }
    }

    public void WetGate()
    {
        if (isWet) return;
        
        isWet = true;
        wetTimer = wetDuration;
        
        // Change collider to trigger when wet
        gateCollider.isTrigger = true;
        
        UpdateVisual();
        
        // Play wet sound
        if (wetSound != null)
        {
            AudioSource.PlayClipAtPoint(wetSound, transform.position);
        }
        
        // Start water effect
        if (waterEffect != null)
        {
            waterEffect.Play();
        }
        
        Debug.Log("Gate wetted! Player can now slip through.");
    }

    public void DryGate()
    {
        if (!isWet) return;
        
        isWet = false;
        wetTimer = 0f;
        
        // Change collider back to solid when dry
        gateCollider.isTrigger = false;
        
        UpdateVisual();
        
        // Stop water effect
        if (waterEffect != null)
        {
            waterEffect.Stop();
        }
        
        Debug.Log("Gate dried out - now blocking again.");
    }

    private void UpdateVisual()
    {
        if (gateRenderer != null)
        {
            gateRenderer.color = isWet ? wetColor : dryColor;
        }
    }

    [ContextMenu("Wet Gate")]
    public void TestWetGate()
    {
        WetGate();
    }

    [ContextMenu("Dry Gate")]
    public void TestDryGate()
    {
        DryGate();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = isWet ? Color.blue : Color.brown;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        if (isWet)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.right * slipForce * 0.5f);
        }
    }
#endif
}