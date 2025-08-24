using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Health Pickup")]
public class HealthPickup : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private bool healToFull = false;
    [SerializeField] private bool oneTimeUse = true;

    [Header("Audio & Effects")]
    [SerializeField] private AudioClip healSound;
    [SerializeField] private GameObject healEffect;
    [SerializeField] private float effectDuration = 2f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;

    private bool _hasBeenUsed = false;
    private Vector3 _originalScale;
    private Color _originalColor;

    private void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (iconRenderer != null)
        {
            _originalScale = iconRenderer.transform.localScale;
            _originalColor = iconRenderer.color;
        }
    }

    private void Update()
    {
        if (_hasBeenUsed) return;

        // Pulse animation
        if (iconRenderer != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            iconRenderer.transform.localScale = _originalScale * pulse;

            // Color pulse (slight green tint)
            float colorPulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * 2f);
            Color pulseColor = Color.Lerp(_originalColor, Color.green, colorPulse * 0.3f);
            iconRenderer.color = pulseColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasBeenUsed) return;

        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        // Check if player needs healing
        if (player.CurrentHealth >= player.MaxHealth)
        {
            Debug.Log("Player health is already full!");
            return;
        }

        HealPlayer(player);
    }

    private void HealPlayer(PlayerController2D player)
    {
        if (healToFull)
        {
            int healingNeeded = player.MaxHealth - player.CurrentHealth;
            player.Heal(healingNeeded);
            Debug.Log($"Medkit used: Player healed to full health ({player.MaxHealth})");
        }
        else
        {
            player.Heal(healAmount);
            Debug.Log($"Medkit used: Player healed {healAmount} HP (Current: {player.CurrentHealth}/{player.MaxHealth})");
        }

        // Audio feedback
        if (healSound != null)
        {
            AudioSource.PlayClipAtPoint(healSound, transform.position);
        }

        // Visual effect
        if (healEffect != null)
        {
            var effect = Instantiate(healEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // Mark as used
        if (oneTimeUse)
        {
            _hasBeenUsed = true;
            
            // Hide or destroy the pickup
            if (iconRenderer != null)
            {
                iconRenderer.enabled = false;
            }
            
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Destroy(gameObject, effectDuration);
        }
    }

    // Add heal method to PlayerController2D if it doesn't exist
    private void Reset()
    {
        // Auto-setup for medkit appearance
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
        }

        gameObject.name = "Health Pickup";
        gameObject.tag = "Respawn"; // Use existing tag for pickups
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position, "Health", true);
    }
#endif
}