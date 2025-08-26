// Assets/_Project/Scripts/DamageOnTouch.cs
using UnityEngine;

[AddComponentMenu("Combat/Damage On Touch")]
public class DamageOnTouch : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackX = 8f;
    [SerializeField] private float knockbackY = 10f;
    [Tooltip("If true, push the player away from this attacker based on relative position.")]
    [SerializeField] private bool fromAttackerDirection = true;

    [Header("Targeting")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField, Min(0f)] private float hitCooldown = 0.2f;

    private float _nextHitTime = 0f;

    void OnCollisionEnter2D(Collision2D c)
    {
        // IMPORTANT: use the OTHER collider (the player), not our own
        if (c.otherCollider) TryHit(c.otherCollider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other) TryHit(other);
    }

    void TryHit(Collider2D other)
    {
        if (Time.time < _nextHitTime) return;
        if (!other.CompareTag(targetTag)) return;

        var player = other.GetComponentInParent<PlayerController2D>();
        if (!player) return;

        float dir = 1f;
        if (fromAttackerDirection)
            dir = Mathf.Sign(other.bounds.center.x - transform.position.x);

        Vector2 kb = new Vector2(dir * knockbackX, knockbackY);
        player.ApplyDamage(kb);
        _nextHitTime = Time.time + hitCooldown;
    }
}
// Assets/_Project/Scripts/DamageOnTouch.cs
using UnityEngine;

[AddComponentMenu("Combat/Damage On Touch")]
public class DamageOnTouch : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Knockback (impulse)")]
    [SerializeField] private float knockbackX = 8f;
    [SerializeField] private float knockbackY = 10f;
    [Tooltip("If true, push the player away from this attacker based on relative position.")]
    [SerializeField] private bool fromAttackerDirection = true;

    [Header("Targeting")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField, Min(0f)] private float hitCooldown = 0.2f;

    private float _nextHitTime = 0f;

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.otherCollider != null) TryHit(c.otherCollider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void TryHit(Collider2D other)
    {
        if (other == null) return;
        if (Time.time < _nextHitTime) return;
        if (!other.CompareTag(targetTag)) return;

        // Knockback
        var rb = other.attachedRigidbody ?? other.GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            float dir = 1f;
            if (fromAttackerDirection)
                dir = Mathf.Sign(other.bounds.center.x - transform.position.x);

            Vector2 impulse = new Vector2(dir * knockbackX, knockbackY);
            rb.velocity = new Vector2(rb.velocity.x, 0f); // stabilize vertical before impulse
            rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        // Damage: call ApplyDamage(int) if present. Avoid compile-time dependency.
        other.gameObject.SendMessageUpwards("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);

        _nextHitTime = Time.time + hitCooldown;
    }
}