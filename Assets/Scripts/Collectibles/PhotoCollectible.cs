using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Collectibles
{
    public class PhotoCollectible : MonoBehaviour
    {
        [Tooltip("Unique ID for this photo (use a GUID or unique string)")]
        public string photoId;

        [Tooltip("Addressable Sprite for this photo")] public AssetReferenceT<Sprite> photoSprite;

        [Tooltip("Optional pickup sound")] public AudioSource pickupAudio;

        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            // Set layer at runtime (avoid doing this in OnValidate/Awake to prevent editor warnings)
            var collectibles = LayerMask.NameToLayer("Collectibles");
            if (collectibles >= 0 && gameObject.layer != collectibles)
            {
                gameObject.layer = collectibles;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only player may collect; detect via IAgeAwareCharacter marker
            if (other == null) return;
            var isPlayer = other.GetComponentInParent<IAgeAwareCharacter>() != null;
            if (!isPlayer) return;

            Collect();
        }

        public async void Collect()
        {
            if (string.IsNullOrEmpty(photoId))
            {
                Debug.LogWarning("PhotoCollectible: photoId is empty");
                return;
            }

            // Disable interactions immediately
            if (_collider != null) _collider.enabled = false;
            if (_spriteRenderer != null) _spriteRenderer.enabled = false;

            if (pickupAudio != null)
            {
                pickupAudio.Play();
            }

            await PhotoAlbumService.Instance.AddPhotoAsync(photoId, photoSprite);

            // Memory stinger + haptics
            WWIII.SideScroller.Audio.BiographicalAudioManager.Instance?.PlayMemoryStinger();

#if NICE_VIBRATIONS
            try 
            { 
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(MoreMountains.NiceVibrations.HapticTypes.Success); 
            } 
            catch (System.Exception e) 
            { 
                Debug.LogWarning($"Nice Vibrations not available: {e.Message}"); 
            }
#endif

            // You may pool instead of destroying for performance
            Destroy(gameObject, pickupAudio != null ? Mathf.Max(0.05f, pickupAudio.clip.length) : 0.05f);
        }
    }
}
