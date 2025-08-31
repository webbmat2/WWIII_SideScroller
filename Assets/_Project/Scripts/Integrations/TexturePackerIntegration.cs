using UnityEngine;
using UnityEditor;
// using CodeAndWeb.TexturePacker;

namespace WWIII.Integrations
{
    /// <summary>
    /// Integration for TexturePacker sprite atlas system
    /// Currently disabled until TexturePacker import is properly configured
    /// </summary>
    public class TexturePackerIntegration : MonoBehaviour
    {
        [Header("TexturePacker Atlas Management")]
        [Tooltip("Primary game atlas for UI and characters")]
        public Texture2D gameAtlas;
        
        [Tooltip("Environmental atlas for tiles and backgrounds")]
        public Texture2D environmentAtlas;

        [Tooltip("Effects atlas for particles and VFX")]
        public Texture2D effectsAtlas;

        /// <summary>
        /// Get sprite from the appropriate atlas by name (placeholder)
        /// </summary>
        public Sprite GetSprite(string spriteName, AtlasType atlasType = AtlasType.Game)
        {
            Debug.Log($"⚠️ TexturePacker integration pending: {spriteName} from {atlasType}");
            return null;
        }

        /// <summary>
        /// Batch load sprites for a level (placeholder)
        /// </summary>
        public void PreloadLevelSprites(string[] spriteNames, AtlasType atlasType = AtlasType.Environment)
        {
            Debug.Log($"⚠️ TexturePacker preload pending: {spriteNames.Length} sprites from {atlasType}");
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor tool to refresh all TexturePacker atlases (placeholder)
        /// </summary>
        [MenuItem("WWIII/🎨 REFRESH TEXTURE PACKER ATLASES")]
        public static void RefreshAllAtlases()
        {
            Debug.Log("⚠️ TexturePacker refresh pending - install TexturePacker Importer package");
            
            EditorUtility.DisplayDialog("TexturePacker Integration",
                "TexturePacker Importer package needs to be installed.\n\n" +
                "1. Download from Unity Asset Store\n" +
                "2. Import into project\n" +
                "3. Configure assembly references\n\n" +
                "This will enable sprite atlas management.",
                "OK");
        }

        /// <summary>
        /// Create atlas configuration for WWIII project structure (placeholder)
        /// </summary>
        [MenuItem("WWIII/🎨 SETUP ATLAS CONFIGURATION")]
        public static void SetupAtlasConfiguration()
        {
            Debug.Log("⚠️ TexturePacker setup pending - install package first");
            
            EditorUtility.DisplayDialog("Atlas Configuration",
                "TexturePacker integration is ready for setup.\n\n" +
                "Install TexturePacker Importer first, then:\n" +
                "1. Create .tpsheet files\n" +
                "2. Configure sprite sources\n" +
                "3. Import atlases",
                "OK");
        }
#endif
    }

    public enum AtlasType
    {
        Game,        // UI, Characters, Items
        Environment, // Tiles, Backgrounds, Props
        Effects      // Particles, VFX, Weapons
    }
}