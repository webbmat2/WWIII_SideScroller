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
        if (c.collider) TryHit(c.collider);
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