using UnityEngine;

[AddComponentMenu("Gameplay/Damage On Touch (AI Fix)")]
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

        // Only affect the player
        var pc = other.GetComponent<PlayerController2D>();
        if (pc == null && !other.CompareTag("Player")) return;
        if (pc == null) pc = other.GetComponentInParent<PlayerController2D>();
        if (pc == null) return;

        // Apply knockback if the player has a Rigidbody2D
        var rb = other.attachedRigidbody ?? other.GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = ((Vector2)(other.transform.position - transform.position)).normalized;
            Vector2 impulse = new Vector2(dir.x * knockbackForce, Mathf.Abs(knockbackUp));
            rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        // Apply damage as INT
        pc.ApplyDamage(damage);
        _lastHitTime = Time.time;
    }
}