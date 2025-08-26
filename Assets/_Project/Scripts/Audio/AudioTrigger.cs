using UnityEngine;

namespace WWIII.Audio
{
    public class AudioTrigger : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;
        [SerializeField] private bool is3D = false;
        [SerializeField] private bool playOnAwake = false;
        [SerializeField] private bool playOnTriggerEnter = true;
        [SerializeField] private bool playOnTriggerExit = false;
        [SerializeField] private bool playOnCollisionEnter = false;
        
        [Header("Trigger Settings")]
        [SerializeField] private string[] triggerTags = { "Player" };
        [SerializeField] private bool oneTimeUse = false;
        [SerializeField] private float cooldownTime = 0f;
        
        [Header("Random Variation")]
        [SerializeField] private bool randomizePitch = false;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.8f, 1.2f);
        [SerializeField] private bool randomizeVolume = false;
        [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1f);
        
        private bool hasTriggered = false;
        private float lastTriggerTime = 0f;
        
        private void Start()
        {
            if (playOnAwake)
            {
                PlayAudio();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (playOnTriggerEnter && CanTrigger(other.tag))
            {
                PlayAudio();
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (playOnTriggerExit && CanTrigger(other.tag))
            {
                PlayAudio();
            }
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (playOnCollisionEnter && CanTrigger(collision.gameObject.tag))
            {
                PlayAudio();
            }
        }
        
        private bool CanTrigger(string objectTag)
        {
            // Check if already triggered and one-time use
            if (oneTimeUse && hasTriggered)
                return false;
            
            // Check cooldown
            if (Time.time - lastTriggerTime < cooldownTime)
                return false;
            
            // Check if tag matches
            if (triggerTags == null || triggerTags.Length == 0)
                return true;
            
            foreach (string tag in triggerTags)
            {
                if (objectTag == tag)
                    return true;
            }
            
            return false;
        }
        
        public void PlayAudio()
        {
            if (audioClip == null)
            {
                Debug.LogWarning($"No audio clip assigned to AudioTrigger on {gameObject.name}");
                return;
            }
            
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager not found!");
                return;
            }
            
            // Apply random variations
            float finalVolume = volume;
            float finalPitch = pitch;
            
            if (randomizeVolume)
            {
                finalVolume *= Random.Range(volumeRange.x, volumeRange.y);
            }
            
            if (randomizePitch)
            {
                finalPitch = Random.Range(pitchRange.x, pitchRange.y);
            }
            
            // Play audio
            if (is3D)
            {
                AudioManager.Instance.PlaySFX(audioClip, transform.position, finalVolume, finalPitch);
            }
            else
            {
                AudioManager.Instance.PlaySFX(audioClip, finalVolume, finalPitch);
            }
            
            // Update trigger state
            hasTriggered = true;
            lastTriggerTime = Time.time;
        }
        
        public void ResetTrigger()
        {
            hasTriggered = false;
        }
        
        public void SetAudioClip(AudioClip clip)
        {
            audioClip = clip;
        }
        
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
        }
        
        public void SetPitch(float newPitch)
        {
            pitch = Mathf.Clamp(newPitch, 0.1f, 3f);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw trigger area
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.color = Color.yellow;
                if (col is BoxCollider2D boxCol)
                {
                    Gizmos.DrawWireCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
                }
                else if (col is CircleCollider2D circleCol)
                {
                    Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
                }
            }
        }
    }
}