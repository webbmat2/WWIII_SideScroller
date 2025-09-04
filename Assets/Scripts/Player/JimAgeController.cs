using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Player
{
    [DisallowMultipleComponent]
    public class JimAgeController : MonoBehaviour, IAgeAwareCharacter
    {
        [Header("Renderers")]
        [Tooltip("Primary SpriteRenderer to swap when using static sprite per age.")]
        public SpriteRenderer mainRenderer;

        [Tooltip("Optional additional SpriteRenderers to update together (e.g., shadow/outline layers).")]
        public List<SpriteRenderer> extraRenderers = new List<SpriteRenderer>();

        [Header("Animator Integration")] 
        [Tooltip("If true and an Animator with a controller is present, sprite swapping is skipped.")]
        public bool preferAnimatorOverrides = true;
        private Animator _animator;

        [Header("Movement Runtime Values")]
        public float maxRunSpeed = 5f;
        public float acceleration = 50f;
        public float deceleration = 60f;
        public float jumpForce = 8f;
        public float gravityScale = 3f;
        private Rigidbody2D _rb;

        [Header("Sprite Selection")]
        [Tooltip("Fallback key searched in atlas when no explicit sprite override is found.")]
        public string defaultIdleKey = "Idle";

        [Serializable]
        public struct AgeSpriteOverride
        {
            public int ageYears;
            public string spriteName;
        }
        [Tooltip("Per-age explicit sprite names to select from the atlas.")]
        public List<AgeSpriteOverride> spriteOverrides = new List<AgeSpriteOverride>();

        private AsyncOperationHandle<SpriteAtlas>? _localAtlasHandle; // used only if we load atlas ourselves

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponentInChildren<Animator>();
            if (mainRenderer == null) mainRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnDestroy()
        {
            if (_localAtlasHandle.HasValue && _localAtlasHandle.Value.IsValid())
            {
                Addressables.Release(_localAtlasHandle.Value);
                _localAtlasHandle = null;
            }
        }

        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            maxRunSpeed = config.maxRunSpeed;
            acceleration = config.acceleration;
            deceleration = config.deceleration;
            jumpForce = config.jumpForce;
            gravityScale = config.gravityScale;
            if (_rb != null) _rb.gravityScale = gravityScale;
        }

        public async void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;

            // Optionally skip sprite swapping if animator is in control
            if (preferAnimatorOverrides && _animator != null && _animator.runtimeAnimatorController != null)
            {
                return;
            }

            // Try to reuse the atlas already loaded by AgeManager to avoid duplicate loads
            SpriteAtlas atlas = null;
            var mgr = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (mgr != null) atlas = mgr.CurrentSpriteAtlas;

            if (atlas == null && profile.spriteAtlas != null && profile.spriteAtlas.RuntimeKeyIsValid())
            {
                // Load locally; remember handle so we can release on next age
                if (_localAtlasHandle.HasValue && _localAtlasHandle.Value.IsValid())
                {
                    Addressables.Release(_localAtlasHandle.Value);
                    _localAtlasHandle = null;
                }
                var handle = profile.spriteAtlas.LoadAssetAsync();
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    atlas = handle.Result;
                    _localAtlasHandle = handle;
                }
            }

            if (atlas == null)
                return;

            // Determine target sprite name: explicit override by years > name heuristic by Idle key
            string targetName = ResolveSpriteName(profile, atlas);
            if (string.IsNullOrEmpty(targetName))
                return;

            var sprite = atlas.GetSprite(targetName);
            if (sprite == null)
            {
                // Try case-insensitive search across atlas
                var all = new Sprite[atlas.spriteCount];
                atlas.GetSprites(all);
                sprite = all.FirstOrDefault(s => string.Equals(s.name, targetName, StringComparison.OrdinalIgnoreCase));
            }

            if (sprite == null || mainRenderer == null)
                return;

            // Apply to primary and extras
            mainRenderer.sprite = sprite;
            foreach (var r in extraRenderers)
            {
                if (r != null) r.sprite = sprite;
            }
        }

        private string ResolveSpriteName(AgeProfile profile, SpriteAtlas atlas)
        {
            // 1) Explicit override by age years
            var ov = spriteOverrides.FirstOrDefault(o => o.ageYears == profile.ageYears);
            if (!string.IsNullOrEmpty(ov.spriteName)) return ov.spriteName;

            // 2) Heuristic: find a sprite that contains the idle key and either age years or display name
            var sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);

            string yearsToken = profile.ageYears.ToString();
            string displayToken = profile.displayName ?? string.Empty;
            string idleKey = defaultIdleKey ?? "Idle";

            // Prefer exact match with years
            var match = sprites.FirstOrDefault(s =>
                NameContains(s.name, idleKey) && NameContains(s.name, yearsToken));
            if (match != null) return match.name;

            // Then with display name
            if (!string.IsNullOrEmpty(displayToken))
            {
                match = sprites.FirstOrDefault(s =>
                    NameContains(s.name, idleKey) && NameContains(s.name, displayToken));
                if (match != null) return match.name;
            }

            // Lastly, any idle key
            match = sprites.FirstOrDefault(s => NameContains(s.name, idleKey));
            return match != null ? match.name : null;
        }

        private static bool NameContains(string haystack, string needle)
        {
            if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(needle)) return false;
            return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
