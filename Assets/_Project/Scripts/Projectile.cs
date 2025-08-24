using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[AddComponentMenu("Gameplay/Projectile")]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSound;

    private Rigidbody2D _rb;
    private bool _hasHit = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        // Setup physics
        _rb.gravityScale = 0f; // Projectiles don't fall by default
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector3 direction, float speed)
    {
        _rb.linearVelocity = direction * speed;
        
        // Rotate to face direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;

        // Check if hit player
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player != null)
        {
            HitPlayer(player);
            return;
        }

        // Check if hit wall/ground (not player layer)
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            HitWall();
        }
    }

    private void HitPlayer(PlayerController2D player)
    {
        _hasHit = true;

        // Calculate knockback
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Vector2 knockback = new Vector2(direction.x * knockbackForce, Mathf.Abs(direction.y) * knockbackForce + 2f);

        player.ApplyDamage(damage, knockback, 0.15f, 0.75f);

        CreateHitEffect();
        
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    private void HitWall()
    {
        _hasHit = true;
        CreateHitEffect();
        
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    private void CreateHitEffect()
    {
        // Visual effect
        if (hitEffect != null)
        {
            var effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Audio effect
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}