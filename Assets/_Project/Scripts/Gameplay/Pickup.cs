using UnityEngine;

namespace WWIII.Gameplay
{
    public class Pickup : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [SerializeField] private PickupType pickupType = PickupType.Coin;
        [SerializeField] private int value = 100;
        [SerializeField] private bool destroyOnPickup = true;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem pickupEffect;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private float magnetRange = 2f;
        [SerializeField] private float magnetStrength = 5f;
        
        [Header("Animation")]
        [SerializeField] private bool enableBobAnimation = true;
        [SerializeField] private float bobHeight = 0.3f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private bool enableRotation = true;
        [SerializeField] private float rotationSpeed = 90f;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color originalColor = Color.white;
        [SerializeField] private Color flashColor = Color.yellow;
        [SerializeField] private float flashDuration = 0.1f;
        
        private AudioSource audioSource;
        private Vector3 startPosition;
        private bool isPickedUp = false;
        private Transform playerTransform;
        private bool playerInRange = false;
        
        public System.Action<PickupType, int> OnPickedUp;
        
        private void Awake()
        {
            InitializeComponents();
            SetupAudio();
        }
        
        private void Start()
        {
            startPosition = transform.position;
            FindPlayer();
        }
        
        private void Update()
        {
            if (isPickedUp) return;
            
            HandleAnimations();
            HandleMagnetism();
        }
        
        private void InitializeComponents()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }
        
        private void SetupAudio()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && pickupSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = 0.7f;
            }
        }
        
        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        private void HandleAnimations()
        {
            // Bob animation
            if (enableBobAnimation)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            
            // Rotation animation
            if (enableRotation)
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void HandleMagnetism()
        {
            if (playerTransform == null || magnetRange <= 0) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= magnetRange)
            {
                if (!playerInRange)
                {
                    playerInRange = true;
                    OnPlayerEnterRange();
                }
                
                // Move towards player
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                float magnetForce = Mathf.Lerp(magnetStrength, magnetStrength * 3f, 1f - (distanceToPlayer / magnetRange));
                transform.position += direction * magnetForce * Time.deltaTime;
            }
            else
            {
                if (playerInRange)
                {
                    playerInRange = false;
                    OnPlayerExitRange();
                }
            }
        }
        
        private void OnPlayerEnterRange()
        {
            // Flash effect when player enters magnet range
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashCoroutine());
            }
        }
        
        private void OnPlayerExitRange()
        {
            // Could add effects when player leaves range
        }
        
        private System.Collections.IEnumerator FlashCoroutine()
        {
            if (spriteRenderer == null) yield break;
            
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return;
            
            if (other.CompareTag("Player"))
            {
                CollectPickup(other.gameObject);
            }
        }
        
        private void CollectPickup(GameObject player)
        {
            isPickedUp = true;
            
            Debug.Log($"Player collected {pickupType} worth {value} points!");
            
            // Play effects
            PlayEffects();
            
            // Trigger haptic feedback
            TriggerHapticFeedback();
            
            // Notify game systems
            NotifyPickup();
            
            // Handle destruction
            if (destroyOnPickup)
            {
                StartCoroutine(DestroyAfterEffect());
            }
            else
            {
                // Just hide if not destroying
                gameObject.SetActive(false);
            }
        }
        
        private void PlayEffects()
        {
            // Play particle effect
            if (pickupEffect != null)
            {
                if (pickupEffect.main.prewarm)
                {
                    pickupEffect.Play();
                }
                else
                {
                    ParticleSystem effect = Instantiate(pickupEffect, transform.position, transform.rotation);
                    Destroy(effect.gameObject, 2f);
                }
            }
            
            // Play pickup sound through AudioManager if available
            if (WWIII.Audio.AudioManager.Instance != null)
            {
                WWIII.Audio.AudioManager.Instance.PlayCoinCollect();
            }
            else if (audioSource != null && pickupSound != null)
            {
                // Fallback to local audio source
                audioSource.PlayOneShot(pickupSound);
            }
        }
        
        private void TriggerHapticFeedback()
        {
            if (WWIII.Core.MobileOptimizer.Instance != null)
            {
                WWIII.Core.MobileOptimizer.Instance.TriggerHapticFeedback(WWIII.Core.HapticFeedbackType.LightImpact);
            }
        }
        
        private void NotifyPickup()
        {
            // Notify local listeners
            OnPickedUp?.Invoke(pickupType, value);
            
            // Notify game manager or score system
            var gameManager = WWIII.Core.GameManager.Instance;
            if (gameManager != null)
            {
                // gameManager.AddScore(value);
                Debug.Log($"Added {value} points to score");
            }
            
            // Could also trigger achievements, update UI, etc.
        }
        
        private System.Collections.IEnumerator DestroyAfterEffect()
        {
            // Hide visual components but keep object for audio
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            
            // Disable collider
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // Wait for sound to finish
            float audioLength = pickupSound != null ? pickupSound.length : 0.1f;
            yield return new WaitForSeconds(audioLength);
            
            Destroy(gameObject);
        }
        
        public void ResetPickup()
        {
            isPickedUp = false;
            gameObject.SetActive(true);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = originalColor;
            }
            
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
            
            transform.position = startPosition;
        }
        
        public void SetValue(int newValue)
        {
            value = newValue;
        }
        
        public int GetValue()
        {
            return value;
        }
        
        public PickupType GetPickupType()
        {
            return pickupType;
        }
        
        public bool IsPickedUp()
        {
            return isPickedUp;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw pickup indicator
            Gizmos.color = Color.green;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
            
            // Draw magnet range
            if (magnetRange > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, magnetRange);
            }
        }
    }
    
    public enum PickupType
    {
        Coin,
        Gem,
        PowerUp,
        Health,
        Key,
        Star,
        Bonus,
        Collectible
    }
}