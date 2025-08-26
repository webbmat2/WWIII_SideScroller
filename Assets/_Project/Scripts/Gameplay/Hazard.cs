using UnityEngine;
using WWIII.Core;

namespace WWIII.Gameplay
{
    public class Hazard : MonoBehaviour
    {
        [Header("Hazard Settings")]
        [SerializeField] private HazardType hazardType = HazardType.Spikes;
        [SerializeField] private int damage = 1;
        [SerializeField] private bool destroyOnContact = false;
        [SerializeField] private bool respawnPlayer = true;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem deathEffect;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private float screenShakeIntensity = 0.3f;
        [SerializeField] private float screenShakeDuration = 0.2f;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string triggerAnimationName = "Activate";
        
        private AudioSource audioSource;
        private bool hasTriggered = false;
        
        public System.Action<GameObject> OnHazardTriggered;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && deathSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered && !CanTriggerMultipleTimes()) return;
            
            if (other.CompareTag("Player"))
            {
                TriggerHazard(other.gameObject);
            }
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (hasTriggered && !CanTriggerMultipleTimes()) return;
            
            if (collision.gameObject.CompareTag("Player"))
            {
                TriggerHazard(collision.gameObject);
            }
        }
        
        private void TriggerHazard(GameObject player)
        {
            hasTriggered = true;
            
            Debug.Log($"Player hit {hazardType} hazard!");
            
            // Play effects
            PlayEffects();
            
            // Trigger camera shake
            TriggerCameraShake();
            
            // Trigger haptic feedback
            TriggerHapticFeedback();
            
            // Handle player response
            HandlePlayerHit(player);
            
            // Trigger animation
            TriggerAnimation();
            
            // Notify listeners
            OnHazardTriggered?.Invoke(player);
            
            // Destroy if configured
            if (destroyOnContact)
            {
                Destroy(gameObject, 0.1f);
            }
        }
        
        private void PlayEffects()
        {
            // Play particle effect
            if (deathEffect != null)
            {
                if (deathEffect.main.prewarm)
                {
                    deathEffect.Play();
                }
                else
                {
                    ParticleSystem effect = Instantiate(deathEffect, transform.position, transform.rotation);
                    Destroy(effect.gameObject, 2f);
                }
            }
            
            // Play death sound
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
        }
        
        private void TriggerCameraShake()
        {
            var cameraController = FindFirstObjectByType<WWIII.Camera.CameraFollowController>();
            if (cameraController != null)
            {
                cameraController.TriggerShake(screenShakeIntensity, screenShakeDuration);
            }
        }
        
        private void TriggerHapticFeedback()
        {
            if (MobileOptimizer.Instance != null)
            {
                MobileOptimizer.Instance.TriggerHapticFeedback(HapticFeedbackType.MediumImpact);
            }
        }
        
        private void HandlePlayerHit(GameObject player)
        {
            if (respawnPlayer)
            {
                // Find checkpoint manager and respawn player
                var checkpointManager = FindFirstObjectByType<CheckpointManager>();
                if (checkpointManager != null)
                {
                    checkpointManager.RespawnPlayer();
                }
                else
                {
                    Debug.LogWarning("No CheckpointManager found! Cannot respawn player.");
                }
            }
            
            // Could also handle health system here if implemented
            // var playerHealth = player.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damage);
            // }
        }
        
        private void TriggerAnimation()
        {
            if (animator != null && !string.IsNullOrEmpty(triggerAnimationName))
            {
                animator.SetTrigger(triggerAnimationName);
            }
        }
        
        private bool CanTriggerMultipleTimes()
        {
            return hazardType switch
            {
                HazardType.Spikes => false,
                HazardType.Saw => true,
                HazardType.Fire => true,
                HazardType.Acid => true,
                HazardType.Laser => true,
                _ => false
            };
        }
        
        public void ResetHazard()
        {
            hasTriggered = false;
        }
        
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }
        
        public HazardType GetHazardType()
        {
            return hazardType;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw hazard indicator
            Gizmos.color = Color.red;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
        }
    }
    
    public enum HazardType
    {
        Spikes,
        Saw,
        Fire,
        Acid,
        Laser,
        Pit,
        MovingPlatform,
        Crusher
    }
}