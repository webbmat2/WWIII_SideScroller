using UnityEngine;

[AddComponentMenu("Player/Player Controller")]
[RequireComponent(typeof(PlayerMovement), typeof(PlayerHealth), typeof(PlayerAbilities))]
public class PlayerController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerAbilities abilities;
    
    // Legacy compatibility properties
    public PlayerMovement Movement => movement;
    public PlayerHealth Health => health;
    public PlayerAbilities Abilities => abilities;
    
    public bool IsGrounded => movement?.IsGrounded ?? false;
    public bool IsCrouching => movement?.IsCrouching ?? false;
    public float HorizontalInput => movement?.HorizontalInput ?? 0f;

    private void Awake()
    {
        // Auto-find components if not assigned
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (health == null) health = GetComponent<PlayerHealth>();
        if (abilities == null) abilities = GetComponent<PlayerAbilities>();
        
        // Ensure all required components exist
        if (movement == null) movement = gameObject.AddComponent<PlayerMovement>();
        if (health == null) health = gameObject.AddComponent<PlayerHealth>();
        if (abilities == null) abilities = gameObject.AddComponent<PlayerAbilities>();
        
        ValidateSetup();
    }

    private void ValidateSetup()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Ensure proper physics setup
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3.8f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        var collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            var boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(0.8f, 1.8f);
        }
        
        // Ensure Player tag
        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
        }
        
        Debug.Log("PlayerController setup validated");
    }

    // Legacy compatibility method for DamageOnTouch
    public void ApplyDamage(int damage, Vector2 knockback, float hitStun, float invuln)
    {
        if (health != null)
        {
            health.ApplyDamage(damage, knockback, hitStun, invuln);
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (movement != null)
        {
            movement.SetMovementEnabled(enabled);
        }
    }

    public void SetCurrentAbility(PowerUpType ability)
    {
        if (abilities != null)
        {
            abilities.SetCurrentAbility(ability);
        }
    }

    public void GrantChiliguaro()
    {
        if (abilities != null)
        {
            abilities.GrantChiliguaro();
        }
    }

    public void RemoveChiliguaro()
    {
        if (abilities != null)
        {
            abilities.RemoveChiliguaro();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Validate Player Setup")]
    public void ValidatePlayerSetup()
    {
        ValidateSetup();
        
        Debug.Log("=== PLAYER VALIDATION ===");
        Debug.Log($"Movement: {(movement != null ? "✅" : "❌")}");
        Debug.Log($"Health: {(health != null ? "✅" : "❌")}");
        Debug.Log($"Abilities: {(abilities != null ? "✅" : "❌")}");
        Debug.Log($"Rigidbody2D: {(GetComponent<Rigidbody2D>() != null ? "✅" : "❌")}");
        Debug.Log($"Collider2D: {(GetComponent<Collider2D>() != null ? "✅" : "❌")}");
        Debug.Log($"Player Tag: {(gameObject.CompareTag("Player") ? "✅" : "❌")}");
    }
#endif
}