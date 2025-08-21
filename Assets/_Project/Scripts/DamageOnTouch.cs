using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    [Header("Knockback")]
    [Tooltip("Horizontal push applied to the player on contact")]
    public float horizontalForce = 7f;

    [Tooltip("Small upward pop so the player doesn't stick inside colliders")]
    public float verticalBoost = 2f;

    [Header("Debug")]
    public string logMessage = "DamageOnTouch triggered";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController2D>();
        if (!pc) return;

        // Knock the player away from the hazard
        float dir = Mathf.Sign(other.transform.position.x - transform.position.x);
        if (dir == 0f) dir = -1f;            // default left if perfectly aligned
        Vector2 kb = new Vector2(dir * horizontalForce, verticalBoost);

        Debug.Log(logMessage);
        pc.ApplyDamage(kb);
    }
}