using UnityEngine;

[AddComponentMenu("Player/Player Health")]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHP = 3;
    [SerializeField] private float invulnSeconds = 0.75f;
    [SerializeField] private float hitStunSeconds = 0.15f;
    [SerializeField] private float respawnDelay = 0.75f;
    [SerializeField] private float flashHz = 10f;

    [Header("Respawn")]
    [SerializeField] private Vector3 respawnPoint = Vector3.zero;

    private int currentHP;
    private bool invulnerable = false;
    private bool hitStunActive = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private PlayerMovement playerMovement;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsInvulnerable => invulnerable;
    public Vector3 RespawnPoint => respawnPoint;

    public System.Action<int, int> OnHealthChanged; // currentHP, maxHP
    public System.Action OnPlayerDied;
    public System.Action OnPlayerRespawned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        currentHP = maxHP;
        respawnPoint = transform.position;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void ApplyDamage(int dmg, Vector2 knockback, float hitStun, float invuln)
    {
        if (invulnerable || currentHP <= 0) return;

        // Apply damage
        currentHP = Mathf.Max(0, currentHP - dmg);
        OnHealthChanged?.Invoke(currentHP, maxHP);

        Debug.Log($"Player took {dmg} damage. Health: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        // Apply knockback
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Stop horizontal movement
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        // Start invulnerability and hit stun
        StartInvulnerability(invuln);
        StartHitStun(hitStun);
        StartFlashing();
    }

    private void Die()
    {
        Debug.Log("Player died! Respawning...");
        OnPlayerDied?.Invoke();
        
        // Disable player controls during death
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }

        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        // Reset health
        currentHP = maxHP;
        
        // Reset position
        transform.position = respawnPoint;
        
        // Reset physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Reset visual state
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // Reset states
        invulnerable = false;
        hitStunActive = false;
        
        // Re-enable player
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
        }
        
        OnHealthChanged?.Invoke(currentHP, maxHP);
        OnPlayerRespawned?.Invoke();
        
        Debug.Log($"Player respawned with {currentHP}/{maxHP} health at {respawnPoint}");
    }

    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
        Debug.Log($"Respawn point set to: {respawnPoint}");
    }

    public void Heal(int amount)
    {
        if (currentHP >= maxHP) return;
        
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHealthChanged?.Invoke(currentHP, maxHP);
        
        Debug.Log($"Player healed {amount} HP. Current: {currentHP}/{maxHP}");
    }

    private void StartInvulnerability(float duration)
    {
        invulnerable = true;
        CancelInvoke(nameof(EndInvulnerability));
        Invoke(nameof(EndInvulnerability), duration);
    }

    private void EndInvulnerability()
    {
        invulnerable = false;
    }

    private void StartHitStun(float duration)
    {
        hitStunActive = true;
        CancelInvoke(nameof(EndHitStun));
        Invoke(nameof(EndHitStun), duration);
    }

    private void EndHitStun()
    {
        hitStunActive = false;
    }

    private void StartFlashing()
    {
        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(FlashCoroutine));
            StartCoroutine(nameof(FlashCoroutine));
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        float flashInterval = 1f / flashHz;
        
        while (invulnerable)
        {
            spriteRenderer.color = Color.clear;
            yield return new WaitForSeconds(flashInterval / 2f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval / 2f);
        }
        
        spriteRenderer.color = originalColor;
    }

    public bool CanMove()
    {
        return !hitStunActive && currentHP > 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(respawnPoint, 1f);
        Gizmos.DrawIcon(respawnPoint, "Respawn", true);
    }
#endif
}