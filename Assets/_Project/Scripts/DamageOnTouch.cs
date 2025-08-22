using UnityEngine;

[DisallowMultipleComponent]
public sealed class DamageOnTouch : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] float knockbackX = 6f;
    [SerializeField] float knockbackY = 6f;
    [SerializeField] bool fromAttackerDirection = true;

    [Header("Filter")]
    [SerializeField] string targetTag = "Player";
    [SerializeField] float hitCooldown = 0.25f;

    float nextHitTime;

    void TryHit(Collider2D other, Vector2 contactNormal)
    {
        if (Time.time < nextHitTime) return;
        if (!other) return;
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return;

        var player = other.GetComponentInParent<PlayerController2D>();
        if (!player) return;

        float dir = fromAttackerDirection
            ? Mathf.Sign(other.bounds.center.x - transform.position.x)
            : -Mathf.Sign(contactNormal.x);
        if (dir == 0f) dir = 1f;

        Vector2 kb = new Vector2(knockbackX * dir, knockbackY);
        player.ApplyDamage(kb);

        nextHitTime = Time.time + hitCooldown;
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        Vector2 n = c.contacts.Length > 0 ? c.contacts[0].normal : Vector2.zero;
        TryHit(c.collider, n);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other, Vector2.left);
    }
}