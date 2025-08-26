using UnityEngine;

namespace WWIII.Gameplay
{
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private int checkpointOrder = 0;
        [SerializeField] private bool activateOnTrigger = true;
        [SerializeField] private bool oneTimeUse = false;
        [SerializeField] private Transform spawnPoint;
        
        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer flagRenderer;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color activeColor = Color.green;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string activationTrigger = "Activate";
        [SerializeField] private bool enableFlagWave = true;
        [SerializeField] private float waveSpeed = 2f;
        [SerializeField] private float waveAmount = 0.1f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem activationEffect;
        [SerializeField] private AudioClip activationSound;
        [SerializeField] private GameObject activationGlow;
        [SerializeField] private float glowIntensity = 1.5f;
        
        [Header("Feedback")]
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private bool enableCameraShake = false;
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeDuration = 0.2f;
        
        private bool isActive = false;
        private bool hasBeenUsed = false;
        private AudioSource audioSource;
        private Vector3 originalScale;
        
        public System.Action<Checkpoint> OnCheckpointTriggered;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupCheckpoint();
        }
        
        private void Update()
        {
            if (isActive && enableFlagWave)
            {
                HandleFlagWaveAnimation();
            }
        }
        
        private void InitializeComponents()
        {
            // Setup audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && activationSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            
            // Setup spawn point
            if (spawnPoint == null)
            {
                // Create spawn point if not assigned
                GameObject spawnObj = new GameObject("SpawnPoint");
                spawnObj.transform.SetParent(transform);
                spawnObj.transform.localPosition = Vector3.zero;
                spawnPoint = spawnObj.transform;
            }
            
            // Store original scale for animations
            originalScale = transform.localScale;
        }
        
        private void SetupCheckpoint()
        {
            // Set initial visual state
            UpdateVisualState();
            
            // Register with checkpoint manager
            CheckpointManager checkpointManager = FindFirstObjectByType<CheckpointManager>();
            if (checkpointManager != null)
            {
                checkpointManager.RegisterCheckpoint(this);
            }
            
            Debug.Log($"Checkpoint {name} initialized at order {checkpointOrder}");
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!activateOnTrigger) return;
            if (oneTimeUse && hasBeenUsed) return;
            
            if (other.CompareTag("Player"))
            {
                TriggerCheckpoint();
            }
        }
        
        public void TriggerCheckpoint()
        {
            if (oneTimeUse && hasBeenUsed) return;
            
            Debug.Log($"Checkpoint {name} triggered!");
            
            hasBeenUsed = true;
            
            // Notify checkpoint manager
            OnCheckpointTriggered?.Invoke(this);
        }
        
        public void SetActive(bool active)
        {
            if (isActive == active) return;
            
            isActive = active;
            UpdateVisualState();
            
            if (active)
            {
                PlayActivationEffects();
            }
        }
        
        private void UpdateVisualState()
        {
            // Update sprite
            if (flagRenderer != null)
            {
                if (activeSprite != null && inactiveSprite != null)
                {
                    flagRenderer.sprite = isActive ? activeSprite : inactiveSprite;
                }
                
                flagRenderer.color = isActive ? activeColor : inactiveColor;
            }
            
            // Update glow with intensity
            if (activationGlow != null)
            {
                activationGlow.SetActive(isActive);
                
                if (isActive)
                {
                    // Apply glow intensity to the glow object's components
                    var glowRenderer = activationGlow.GetComponent<SpriteRenderer>();
                    if (glowRenderer != null)
                    {
                        Color glowColor = glowRenderer.color;
                        glowColor.a = glowIntensity;
                        glowRenderer.color = glowColor;
                    }
                    
                    // Also apply to any Light2D components if present
                    var light2D = activationGlow.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
                    if (light2D != null)
                    {
                        light2D.intensity = glowIntensity;
                    }
                }
            }
            
            // Update animator
            if (animator != null)
            {
                animator.SetBool("IsActive", isActive);
            }
        }
        
        public void PlayActivationEffects()
        {
            // Play particle effect
            if (activationEffect != null)
            {
                if (activationEffect.main.prewarm)
                {
                    activationEffect.Play();
                }
                else
                {
                    ParticleSystem effect = Instantiate(activationEffect, transform.position, transform.rotation);
                    Destroy(effect.gameObject, 3f);
                }
            }
            
            // Play activation sound through AudioManager if available
            if (WWIII.Audio.AudioManager.Instance != null)
            {
                WWIII.Audio.AudioManager.Instance.PlayCheckpoint();
            }
            else if (audioSource != null && activationSound != null)
            {
                // Fallback to local audio source
                audioSource.PlayOneShot(activationSound);
            }
            
            // Trigger animation
            if (animator != null && !string.IsNullOrEmpty(activationTrigger))
            {
                animator.SetTrigger(activationTrigger);
            }
            
            // Haptic feedback
            if (enableHapticFeedback && WWIII.Core.MobileOptimizer.Instance != null)
            {
                WWIII.Core.MobileOptimizer.Instance.TriggerHapticFeedback(WWIII.Core.HapticFeedbackType.LightImpact);
            }
            
            // Camera shake
            if (enableCameraShake)
            {
                var cameraController = FindFirstObjectByType<WWIII.Camera.CameraFollowController>();
                if (cameraController != null)
                {
                    cameraController.TriggerShake(shakeIntensity, shakeDuration);
                }
            }
            
            // Scale animation
            StartCoroutine(ActivationScaleAnimation());
        }
        
        private System.Collections.IEnumerator ActivationScaleAnimation()
        {
            Vector3 targetScale = originalScale * 1.2f;
            float duration = 0.3f;
            float elapsed = 0f;
            
            // Scale up
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Scale back down
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        private void HandleFlagWaveAnimation()
        {
            if (flagRenderer == null) return;
            
            // Simple wave animation using sine wave
            float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmount;
            Vector3 scale = originalScale;
            scale.x += wave;
            flagRenderer.transform.localScale = scale;
        }
        
        public Vector3 GetSpawnPosition()
        {
            return spawnPoint != null ? spawnPoint.position : transform.position;
        }
        
        public int GetCheckpointOrder()
        {
            return checkpointOrder;
        }
        
        public bool IsActive()
        {
            return isActive;
        }
        
        public bool HasBeenUsed()
        {
            return hasBeenUsed;
        }
        
        public void ResetCheckpoint()
        {
            hasBeenUsed = false;
            SetActive(false);
        }
        
        public void SetCheckpointOrder(int order)
        {
            checkpointOrder = order;
        }
        
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            spawnPoint = newSpawnPoint;
        }
        
        private void OnDestroy()
        {
            // Unregister from checkpoint manager
            CheckpointManager checkpointManager = FindFirstObjectByType<CheckpointManager>();
            if (checkpointManager != null)
            {
                checkpointManager.UnregisterCheckpoint(this);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw checkpoint indicator
            Gizmos.color = isActive ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            
            // Draw spawn point
            if (spawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(transform.position, spawnPoint.position);
            }
            
            // Draw checkpoint order
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Checkpoint {checkpointOrder}");
            #endif
        }
        
        private void OnValidate()
        {
            // Update visual state in editor
            if (Application.isPlaying)
            {
                UpdateVisualState();
            }
        }
    }
}