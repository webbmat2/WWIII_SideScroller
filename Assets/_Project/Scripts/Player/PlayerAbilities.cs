using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[AddComponentMenu("Player/Player Abilities")]
public class PlayerAbilities : MonoBehaviour
{
    [Header("Hose")]
    [SerializeField] private GameObject hosePrefab;
    [SerializeField] private Transform hoseSpawnPoint;
    [SerializeField] private float hoseRange = 3f;
    [SerializeField] private float hoseDuration = 0.5f;
    
    [Header("Chiliguaro Fireballs")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private int fireballBounces = 3;
    
    [Header("Input")]
    [SerializeField] private InputActionReference abilityAction;
    [SerializeField] private KeyCode abilityKey = KeyCode.X;
    
    private ChapterManager chapterManager;
    private List<PowerUpType> availableAbilities = new List<PowerUpType>();
    private PowerUpType currentActiveAbility = PowerUpType.None;
    private bool hasChiliguaro = false;
    
    public PowerUpType CurrentAbility => currentActiveAbility;
    public bool HasChiliguaro => hasChiliguaro;
    
    public System.Action<PowerUpType> OnAbilityChanged;
    public System.Action OnChiliguaroLost;

    private void Awake()
    {
        chapterManager = ChapterManager.Instance;
        SetupSpawnPoints();
    }

    private void Start()
    {
        if (chapterManager != null)
        {
            chapterManager.OnChapterLoaded += OnChapterLoaded;
            UpdateAvailableAbilities();
        }
    }

    private void OnDestroy()
    {
        if (chapterManager != null)
        {
            chapterManager.OnChapterLoaded -= OnChapterLoaded;
        }
    }

    private void SetupSpawnPoints()
    {
        if (hoseSpawnPoint == null)
        {
            var hoseGO = new GameObject("HoseSpawnPoint");
            hoseGO.transform.SetParent(transform);
            hoseGO.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            hoseSpawnPoint = hoseGO.transform;
        }
        
        if (fireballSpawnPoint == null)
        {
            var fireballGO = new GameObject("FireballSpawnPoint");
            fireballGO.transform.SetParent(transform);
            fireballGO.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            fireballSpawnPoint = fireballGO.transform;
        }
    }

    private void Update()
    {
        HandleAbilityInput();
    }

    private void HandleAbilityInput()
    {
        bool abilityPressed = false;
        
        if (abilityAction != null && abilityAction.action != null)
        {
            abilityPressed = abilityAction.action.WasPressedThisFrame();
        }
        else
        {
            abilityPressed = Input.GetKeyDown(abilityKey);
        }

        if (abilityPressed)
        {
            UseCurrentAbility();
        }
    }

    private void OnChapterLoaded(ChapterData chapter)
    {
        UpdateAvailableAbilities();
    }

    private void UpdateAvailableAbilities()
    {
        availableAbilities.Clear();
        
        if (chapterManager?.CurrentChapter?.availablePowerUps != null)
        {
            availableAbilities.AddRange(chapterManager.CurrentChapter.availablePowerUps);
        }
        
        // Set default ability
        if (availableAbilities.Count > 0)
        {
            SetCurrentAbility(availableAbilities[0]);
        }
        else
        {
            SetCurrentAbility(PowerUpType.None);
        }
    }

    public void SetCurrentAbility(PowerUpType ability)
    {
        if (availableAbilities.Contains(ability) || ability == PowerUpType.None)
        {
            currentActiveAbility = ability;
            OnAbilityChanged?.Invoke(currentActiveAbility);
            Debug.Log($"Ability changed to: {currentActiveAbility}");
        }
    }

    public void GrantChiliguaro()
    {
        hasChiliguaro = true;
        SetCurrentAbility(PowerUpType.Chiliguaro);
        Debug.Log("Chiliguaro power-up granted! Bouncing fireballs activated.");
    }

    public void RemoveChiliguaro()
    {
        if (hasChiliguaro)
        {
            hasChiliguaro = false;
            OnChiliguaroLost?.Invoke();
            
            // Revert to hose if available
            if (availableAbilities.Contains(PowerUpType.Hose))
            {
                SetCurrentAbility(PowerUpType.Hose);
            }
            else
            {
                SetCurrentAbility(PowerUpType.None);
            }
            
            Debug.Log("Chiliguaro power lost!");
        }
    }

    private void UseCurrentAbility()
    {
        switch (currentActiveAbility)
        {
            case PowerUpType.Hose:
                UseHose();
                break;
            case PowerUpType.Chiliguaro:
                UseChiliguaro();
                break;
            default:
                Debug.Log("No ability equipped");
                break;
        }
    }

    private void UseHose()
    {
        Debug.Log("Using Hose!");
        
        // Create hose stream
        if (hosePrefab != null)
        {
            var hoseStream = Instantiate(hosePrefab, hoseSpawnPoint.position, hoseSpawnPoint.rotation);
            Destroy(hoseStream, hoseDuration);
        }
        else
        {
            // Simple raycast version for now
            Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            RaycastHit2D hit = Physics2D.Raycast(hoseSpawnPoint.position, direction, hoseRange);
            
            if (hit.collider != null)
            {
                // Check for slip-n-slide gates
                var slipGate = hit.collider.GetComponent<SlipNSlideGate>();
                if (slipGate != null)
                {
                    slipGate.WetGate();
                }
                
                // Check for Purple Pig boss
                var purplePig = hit.collider.GetComponent<PurplePigBoss>();
                if (purplePig != null)
                {
                    purplePig.OnHoseHit();
                }
            }
        }
    }

    private void UseChiliguaro()
    {
        if (!hasChiliguaro) return;
        
        Debug.Log("Firing Chiliguaro fireball!");
        
        if (fireballPrefab != null)
        {
            var fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);
            var fireballScript = fireball.GetComponent<ChiliguaroFireball>();
            if (fireballScript != null)
            {
                Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
                fireballScript.Launch(direction * fireballSpeed, fireballBounces);
            }
        }
        else
        {
            // Create a simple fireball GameObject
            CreateSimpleFireball();
        }
    }

    private void CreateSimpleFireball()
    {
        var fireballGO = new GameObject("ChiliguaroFireball");
        fireballGO.transform.position = fireballSpawnPoint.position;
        
        // Add visual
        var spriteRenderer = fireballGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateFireballSprite();
        spriteRenderer.color = Color.red;
        
        // Add physics
        var rb = fireballGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.5f;
        
        // Add collider
        var collider = fireballGO.AddComponent<CircleCollider2D>();
        collider.radius = 0.2f;
        collider.isTrigger = true;
        
        // Add fireball script
        var fireballScript = fireballGO.AddComponent<ChiliguaroFireball>();
        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        fireballScript.Launch(direction * fireballSpeed, fireballBounces);
    }

    private Sprite CreateFireballSprite()
    {
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.red;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
    }

    // Called by PlayerHealth when taking damage
    public void OnPlayerTookDamage()
    {
        if (hasChiliguaro)
        {
            RemoveChiliguaro();
        }
    }
}