using UnityEngine;
using System.Collections.Generic;
using WWIII.Player;

namespace WWIII.Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector3 defaultSpawnPoint = Vector3.zero;
        [SerializeField] private bool autoFindPlayer = true;
        
        [Header("Respawn Settings")]
        [SerializeField] private float respawnDelay = 1f;
        [SerializeField] private bool fadeOnRespawn = true;
        [SerializeField] private float fadeInDuration = 0.5f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem respawnEffect;
        [SerializeField] private AudioClip respawnSound;
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float shakeIntensity = 0.2f;
        [SerializeField] private float shakeDuration = 0.3f;
        
        public static CheckpointManager Instance { get; private set; }
        
        private List<Checkpoint> checkpoints = new List<Checkpoint>();
        private Checkpoint activeCheckpoint;
        private Vector3 currentSpawnPoint;
        private PlayerController playerController;
        private AudioSource audioSource;
        
        public System.Action OnPlayerRespawned;
        public System.Action<Checkpoint> OnCheckpointActivated;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupCheckpointManager();
        }
        
        private void InitializeComponents()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && respawnSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        private void SetupCheckpointManager()
        {
            // Auto-find player if not assigned
            if (autoFindPlayer && playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    playerController = player.GetComponent<PlayerController>();
                }
            }
            
            // Set initial spawn point
            if (playerTransform != null)
            {
                currentSpawnPoint = playerTransform.position;
            }
            else
            {
                currentSpawnPoint = defaultSpawnPoint;
            }
            
            // Find all checkpoints in scene
            FindAllCheckpoints();
            
            Debug.Log($"CheckpointManager initialized. Found {checkpoints.Count} checkpoints.");
        }
        
        private void FindAllCheckpoints()
        {
            Checkpoint[] foundCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            
            foreach (Checkpoint checkpoint in foundCheckpoints)
            {
                RegisterCheckpoint(checkpoint);
            }
            
            // Sort checkpoints by order
            checkpoints.Sort((a, b) => a.GetCheckpointOrder().CompareTo(b.GetCheckpointOrder()));
        }
        
        public void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (!checkpoints.Contains(checkpoint))
            {
                checkpoints.Add(checkpoint);
                checkpoint.OnCheckpointTriggered += HandleCheckpointTriggered;
                Debug.Log($"Registered checkpoint: {checkpoint.name}");
            }
        }
        
        public void UnregisterCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoints.Contains(checkpoint))
            {
                checkpoints.Remove(checkpoint);
                checkpoint.OnCheckpointTriggered -= HandleCheckpointTriggered;
                
                // Clear active checkpoint if it's the one being removed
                if (activeCheckpoint == checkpoint)
                {
                    activeCheckpoint = null;
                }
            }
        }
        
        private void HandleCheckpointTriggered(Checkpoint checkpoint)
        {
            ActivateCheckpoint(checkpoint);
        }
        
        public void ActivateCheckpoint(Checkpoint checkpoint)
        {
            if (activeCheckpoint != checkpoint)
            {
                // Deactivate previous checkpoint
                if (activeCheckpoint != null)
                {
                    activeCheckpoint.SetActive(false);
                }
                
                // Activate new checkpoint
                activeCheckpoint = checkpoint;
                activeCheckpoint.SetActive(true);
                currentSpawnPoint = checkpoint.GetSpawnPosition();
                
                Debug.Log($"Checkpoint activated: {checkpoint.name} at {currentSpawnPoint}");
                
                // Trigger effects
                checkpoint.PlayActivationEffects();
                
                // Notify listeners
                OnCheckpointActivated?.Invoke(checkpoint);
            }
        }
        
        public void RespawnPlayer()
        {
            if (playerTransform == null)
            {
                Debug.LogError("Cannot respawn player - playerTransform not assigned!");
                return;
            }
            
            StartCoroutine(RespawnCoroutine());
        }
        
        private System.Collections.IEnumerator RespawnCoroutine()
        {
            Debug.Log($"Respawning player at {currentSpawnPoint}");
            
            // Wait for respawn delay
            yield return new WaitForSeconds(respawnDelay);
            
            // Reset player position and state
            if (playerController != null)
            {
                playerController.ResetPlayer(currentSpawnPoint);
            }
            else
            {
                playerTransform.position = currentSpawnPoint;
                
                // Reset rigidbody if present
                Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
            
            // Play effects
            PlayRespawnEffects();
            
            // Camera shake
            if (enableCameraShake)
            {
                var cameraController = FindFirstObjectByType<WWIII.Camera.CameraFollowController>();
                if (cameraController != null)
                {
                    cameraController.TriggerShake(shakeIntensity, shakeDuration);
                }
            }
            
            // Fade in effect
            if (fadeOnRespawn)
            {
                yield return StartCoroutine(FadeInCoroutine());
            }
            
            // Notify listeners
            OnPlayerRespawned?.Invoke();
        }
        
        private void PlayRespawnEffects()
        {
            // Play particle effect
            if (respawnEffect != null)
            {
                if (respawnEffect.main.prewarm)
                {
                    respawnEffect.transform.position = currentSpawnPoint;
                    respawnEffect.Play();
                }
                else
                {
                    ParticleSystem effect = Instantiate(respawnEffect, currentSpawnPoint, Quaternion.identity);
                    Destroy(effect.gameObject, 3f);
                }
            }
            
            // Play respawn sound
            if (audioSource != null && respawnSound != null)
            {
                audioSource.PlayOneShot(respawnSound);
            }
            
            // Haptic feedback
            if (WWIII.Core.MobileOptimizer.Instance != null)
            {
                WWIII.Core.MobileOptimizer.Instance.TriggerHapticFeedback(WWIII.Core.HapticFeedbackType.MediumImpact);
            }
        }
        
        private System.Collections.IEnumerator FadeInCoroutine()
        {
            // Simple fade in implementation
            // In a full implementation, this would control a UI fade overlay
            yield return new WaitForSeconds(fadeInDuration);
        }
        
        public void SetSpawnPoint(Vector3 position)
        {
            currentSpawnPoint = position;
        }
        
        public Vector3 GetCurrentSpawnPoint()
        {
            return currentSpawnPoint;
        }
        
        public Checkpoint GetActiveCheckpoint()
        {
            return activeCheckpoint;
        }
        
        public List<Checkpoint> GetAllCheckpoints()
        {
            return new List<Checkpoint>(checkpoints);
        }
        
        public void ResetAllCheckpoints()
        {
            foreach (Checkpoint checkpoint in checkpoints)
            {
                checkpoint.SetActive(false);
            }
            
            activeCheckpoint = null;
            currentSpawnPoint = defaultSpawnPoint;
        }
        
        public int GetActiveCheckpointIndex()
        {
            if (activeCheckpoint != null)
            {
                return checkpoints.IndexOf(activeCheckpoint);
            }
            return -1;
        }
        
        public float GetProgressPercentage()
        {
            if (checkpoints.Count == 0) return 0f;
            
            int activeIndex = GetActiveCheckpointIndex();
            if (activeIndex < 0) return 0f;
            
            return (float)(activeIndex + 1) / checkpoints.Count * 100f;
        }
        
        private void OnDestroy()
        {
            // Clean up checkpoint listeners
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (checkpoint != null)
                {
                    checkpoint.OnCheckpointTriggered -= HandleCheckpointTriggered;
                }
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw current spawn point
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentSpawnPoint, 1f);
            Gizmos.DrawLine(currentSpawnPoint, currentSpawnPoint + Vector3.up * 2f);
            
            // Draw default spawn point
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(defaultSpawnPoint, Vector3.one * 0.5f);
        }
    }
}