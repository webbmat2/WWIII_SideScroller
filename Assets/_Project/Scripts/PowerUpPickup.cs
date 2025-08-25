using UnityEngine;

[AddComponentMenu("Gameplay/Power-Up Pickup")]
[RequireComponent(typeof(Collider2D))]
public class PowerUpPickup : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType powerUpType = PowerUpType.Chiliguaro;
    [SerializeField] private bool isTemporary = true;
    [SerializeField] private bool autoConfigureVisual = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;
    
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private bool collected = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        startPosition = transform.position;
    }

    private void Start()
    {
        if (autoConfigureVisual)
        {
            ConfigureVisual();
        }
    }

    private void Update()
    {
        if (!collected)
        {
            // Bob animation
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * bobOffset;
        }
    }

    private void ConfigureVisual()
    {
        Color powerUpColor = Color.white;
        
        switch (powerUpType)
        {
            case PowerUpType.Hose:
                powerUpColor = Color.blue;
                break;
            case PowerUpType.Chiliguaro:
                powerUpColor = Color.red;
                break;
            case PowerUpType.CherryPie:
                powerUpColor = new Color(1f, 0.5f, 0.8f); // Pink
                break;
            case PowerUpType.SmartJim:
                powerUpColor = Color.cyan;
                break;
            case PowerUpType.BeefJerky:
                powerUpColor = new Color(0.5f, 0.3f, 0f); // Brown
                break;
            case PowerUpType.CheeseBall:
                powerUpColor = Color.yellow;
                break;
        }
        
        spriteRenderer.sprite = CreatePowerUpSprite(powerUpColor);
        spriteRenderer.sortingOrder = 1;
    }

    private Sprite CreatePowerUpSprite(Color color)
    {
        Texture2D texture = new Texture2D(24, 24);
        Color[] pixels = new Color[24 * 24];
        
        // Create a star pattern for power-ups
        Vector2 center = new Vector2(12f, 12f);
        
        for (int y = 0; y < 24; y++)
        {
            for (int x = 0; x < 24; x++)
            {
                Vector2 pixel = new Vector2(x, y);
                Vector2 dir = (pixel - center).normalized;
                float distance = Vector2.Distance(pixel, center);
                
                // Create star shape
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float starRadius = 8f + 3f * Mathf.Sin(5f * angle * Mathf.Deg2Rad);
                
                if (distance <= starRadius)
                {
                    pixels[y * 24 + x] = color;
                }
                else
                {
                    pixels[y * 24 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        if (other.CompareTag("Player"))
        {
            PickupPowerUp(other.gameObject);
        }
    }

    private void PickupPowerUp(GameObject player)
    {
        collected = true;
        
        var playerAbilities = player.GetComponent<PlayerAbilities>();
        if (playerAbilities != null)
        {
            ApplyPowerUp(playerAbilities);
        }
        
        // Play sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Spawn effect
        if (pickupEffect != null)
        {
            var effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        Debug.Log($"Power-up collected: {powerUpType}");
        
        // Destroy pickup
        Destroy(gameObject);
    }

    private void ApplyPowerUp(PlayerAbilities playerAbilities)
    {
        switch (powerUpType)
        {
            case PowerUpType.Chiliguaro:
                playerAbilities.GrantChiliguaro();
                break;
            case PowerUpType.Hose:
                playerAbilities.SetCurrentAbility(PowerUpType.Hose);
                break;
            case PowerUpType.CherryPie:
                ApplyStatBuff(playerAbilities, "Cherry Pie: +1 Max HP");
                break;
            case PowerUpType.SmartJim:
                ApplyStatBuff(playerAbilities, "Smart Jim: +Speed/Knowledge");
                break;
            case PowerUpType.BeefJerky:
                ApplyStatBuff(playerAbilities, "Beef Jerky: +Damage");
                break;
            case PowerUpType.CheeseBall:
                ApplyStatBuff(playerAbilities, "Cheese Ball: +Defense");
                break;
        }
    }

    private void ApplyStatBuff(PlayerAbilities playerAbilities, string buffDescription)
    {
        // For now, just log the buff. In full implementation, would modify player stats
        Debug.Log($"Applied buff: {buffDescription}");
        
        // TODO: Implement actual stat modifications when stat system is added
        // Example: playerAbilities.GetComponent<PlayerHealth>().ModifyMaxHP(1);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = collected ? Color.gray : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (!collected)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.8f, Vector3.one * 0.3f);
        }
    }
#endif
}