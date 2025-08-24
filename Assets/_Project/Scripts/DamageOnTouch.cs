using UnityEngine;

[AddComponentMenu("Gameplay/Damage On Touch")]
[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField, Tooltip("Cooldown between consecutive hits on the same target (seconds)")]
    private float hitCooldown = 0.25f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackUp = 4f;

    private float _lastHitTime = -999f;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.gravityScale = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c == null) return;
        TryHit(c.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void TryHit(Collider2D other)
    {
        if (other == null) return;
        if (Time.time - _lastHitTime < hitCooldown) return;

        var pc = other.GetComponent<PlayerController2D>();
        if (pc == null && !other.CompareTag("Player")) return;
        if (pc == null) pc = other.GetComponentInParent<PlayerController2D>();
        if (pc == null) return;

        var rb = other.attachedRigidbody ?? other.GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = ((Vector2)(other.transform.position - transform.position)).normalized;
            Vector2 impulse = new Vector2(dir.x * knockbackForce, Mathf.Abs(knockbackUp));
            rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        pc.ApplyDamage(damage);
        _lastHitTime = Time.time;
    }
}