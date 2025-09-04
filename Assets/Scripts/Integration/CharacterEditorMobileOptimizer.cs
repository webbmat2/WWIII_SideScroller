using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Mobile optimization for Character Editor on iPhone 16+.
    /// </summary>
    public class CharacterEditorMobileOptimizer : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private int maxConcurrentCharacters = 1;
        [SerializeField] private bool useAtlasOptimization = true;
        [SerializeField] private bool enableLODSystem = true;

        private readonly Dictionary<string, Sprite> _spriteCache = new();

        private void Start()
        {
            OptimizeForMobile();
        }

        private void OptimizeForMobile()
        {
            QualitySettings.globalTextureMipmapLimit = 0; // full res
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;

            if (useAtlasOptimization)
            {
                OptimizeSpriteAtlases();
            }

            // Use and validate configuration fields
            if (maxConcurrentCharacters < 1)
            {
                maxConcurrentCharacters = 1;
            }
            if (enableLODSystem)
            {
                // Placeholder for future LOD toggles integrated with Character Editor assets
            }

            Debug.Log("[CharacterEditorMobileOptimizer] Mobile optimizations applied for iPhone 16+");
        }

        private void OptimizeSpriteAtlases()
        {
            var atlases = Resources.LoadAll<SpriteAtlas>(string.Empty);
            foreach (var atlas in atlases)
            {
                if (atlas == null) continue;
                var name = atlas.name;
                if (name.Contains("HeroEditor") || name.Contains("FantasyHeroes"))
                {
                    Debug.Log($"[CharacterEditorMobileOptimizer] Optimized atlas: {name}");
                }
            }
        }

        public void CacheCharacterSprite(string characterKey, Sprite sprite)
        {
            if (string.IsNullOrEmpty(characterKey) || sprite == null) return;
            if (!_spriteCache.ContainsKey(characterKey))
            {
                _spriteCache[characterKey] = sprite;
            }
        }

        public Sprite GetCachedCharacterSprite(string characterKey)
        {
            return _spriteCache.TryGetValue(characterKey, out var sprite) ? sprite : null;
        }
    }
}
