using UnityEngine;

[AddComponentMenu("Player/Chiliguaro Fireball")]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ChiliguaroFireball : MonoBehaviour
{
    [Header("Fireball Settings")]
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask bounceOffLayers = -1;
    
    private Rigidbody2D rb;
    private int bouncesRemaining;
    private bool launched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure proper setup
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 velocity, int bounces)
    {
        if (launched) return;
        
        launched = true;
        bouncesRemaining = bounces;
        rb.linearVelocity = velocity;
        
        Debug.Log($"Fireball launched with {bounces} bounces");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the player who fired it
        if (other.CompareTag("Player")) return;
        
        // Check if it's an enemy or boss
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            DestroyFireball();
            return;
        }
        
        // Check for boss components
        var purplePig = other.GetComponent<PurplePigBoss>();
        if (purplePig != null)
        {
            purplePig.TakeDamage(damage);
            DestroyFireball();
            return;
        }
        
        // Check for bounce
        if (ShouldBounceOff(other))
        {
            HandleBounce(other);
        }
    }

    private bool ShouldBounceOff(Collider2D other)
    {
        // Check if the layer is in our bounce layers
        return ((1 << other.gameObject.layer) & bounceOffLayers) != 0;
    }

    private void HandleBounce(Collider2D other)
    {
        if (bouncesRemaining <= 0)
        {
            DestroyFireball();
            return;
        }
        
        bouncesRemaining--;
        
        // Simple bounce logic - reverse appropriate velocity component
        Vector3 velocity = rb.linearVelocity;
        
        // Determine bounce direction based on collision
        Bounds otherBounds = other.bounds;
        Bounds thisBounds = GetComponent<Collider2D>().bounds;
        
        float overlapX = Mathf.Min(thisBounds.max.x - otherBounds.min.x, otherBounds.max.x - thisBounds.min.x);
        float overlapY = Mathf.Min(thisBounds.max.y - otherBounds.min.y, otherBounds.max.y - thisBounds.min.y);
        
        if (overlapX < overlapY)
        {
            // Horizontal bounce
            velocity.x = -velocity.x;
        }
        else
        {
            // Vertical bounce
            velocity.y = -velocity.y;
        }
        
        rb.linearVelocity = velocity;
        
        Debug.Log($"Fireball bounced! {bouncesRemaining} bounces remaining");
        
        if (bouncesRemaining <= 0)
        {
            // Change color to indicate last bounce
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.yellow;
            }
        }
    }

    private void DestroyFireball()
    {
        // Create destruction effect if desired
        Debug.Log("Fireball destroyed");
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        if (rb != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized);
        }
    }
}

public interface IDamageable
{
    void TakeDamage(int damage);
}