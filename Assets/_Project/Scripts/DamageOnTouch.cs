using UnityEngine;

[AddComponentMenu("Gameplay/Damage On Touch")]
[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float invulnSeconds = 0.75f;

    [Header("Knockback")]
    [SerializeField] private float knockbackX = 12f;
    [SerializeField] private float knockbackY = 10f;
    [SerializeField] private float hitStunSeconds = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;

    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffect;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
        
        // Set to Hazard layer if available, otherwise Water layer
        int hazardLayer = LayerMask.NameToLayer("Hazard");
        if (hazardLayer == -1) hazardLayer = LayerMask.NameToLayer("Water");
        if (hazardLayer != -1) gameObject.layer = hazardLayer;
        
        // Ensure no Rigidbody2D or set it to Static
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"DamageOnTouch: Trigger entered by {other.name}");
        
        // Try PlayerHealth first (preferred), fallback to PlayerController2D
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        var playerController = other.GetComponentInParent<PlayerController2D>();
        
        if (playerHealth == null && playerController == null) 
        {
            Debug.Log($"DamageOnTouch: No player component found on {other.name}");
            return;
        }

        // Direction: push player away from this hazard, always up a bit
        Vector3 playerPos = playerHealth != null ? playerHealth.transform.position : playerController.transform.position;
        float sign = Mathf.Sign(playerPos.x - transform.position.x);
        Vector2 kb = new Vector2(knockbackX * sign, knockbackY);

        Debug.Log($"DamageOnTouch: Applying knockback {kb} to player");
        
        // Apply damage to preferred component
        if (playerHealth != null)
        {
            playerHealth.ApplyDamage(damage, kb, hitStunSeconds, invulnSeconds);
            
            // Notify abilities system that player took damage (removes Chiliguaro)
            var playerAbilities = playerHealth.GetComponent<PlayerAbilities>();
            if (playerAbilities != null)
            {
                playerAbilities.OnPlayerTookDamage();
            }
        }
        else if (playerController != null)
        {
            playerController.ApplyDamage(damage, kb, hitStunSeconds, invulnSeconds);
        }

        // Play sound effect
        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
        }

        // Spawn hit effect
        if (hitEffect != null)
        {
            var effect = Instantiate(hitEffect, playerPos, Quaternion.identity);
            Destroy(effect, 2f); // Auto-cleanup after 2 seconds
        }
    }
}
