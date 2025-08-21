using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] float horizontalForce = 24f;
    [SerializeField, Tooltip("Small upward pop so the player doesn't stick inside colliders")]
    float verticalBoost = 8f;

    void OnCollisionEnter2D(Collision2D c) => TryHurtFromCollision(c);
    void OnCollisionStay2D(Collision2D c)  => TryHurtFromCollision(c);
    void OnTriggerEnter2D(Collider2D o)    => TryHurtFromTrigger(o);
    void OnTriggerStay2D(Collider2D o)     => TryHurtFromTrigger(o);

    void TryHurtFromCollision(Collision2D collision)
    {
        if (collision == null) return;
        var other = collision.collider;
        if (!other || !other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerController2D pc)) return;

        // robust direction from the contact
        Vector2 normal = collision.GetContact(0).normal; // from spikes toward player
        Vector2 kb = new Vector2(-normal.x * horizontalForce, verticalBoost);

        // perfectly-on-top case gets a fallback horizontal push
        if (Mathf.Abs(kb.x) < 0.01f)
        {
            float dir = Mathf.Sign(other.transform.position.x - transform.position.x);
            if (Mathf.Approximately(dir, 0f)) dir = -1f;
            kb.x = dir * horizontalForce;
        }
        pc.ApplyDamage(kb);
    }

    void TryHurtFromTrigger(Collider2D col)
    {
        if (!col || !col.CompareTag("Player")) return;
        if (!col.TryGetComponent(out PlayerController2D pc)) return;

        float dir = Mathf.Sign(col.transform.position.x - transform.position.x);
        if (Mathf.Approximately(dir, 0f)) dir = -1f;
        Vector2 kb = new Vector2(dir * horizontalForce, verticalBoost);
        pc.ApplyDamage(kb);
    }
}
