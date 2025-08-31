using UnityEngine;
using Sirenix.OdinInspector;

namespace WWIII.Data
{
    /// <summary>
    /// Data-driven level configuration following project rules
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDef_", menuName = "WWIII/Level Definition")]
    public class LevelDef : ScriptableObject
    {
        [Title("Level Information")]
        public string levelName = "Level Name";
        public string levelDescription = "Level description";
        
        [Title("Level Settings")]
        public string sceneName = "L1_Tutorial";
        public string biome = "Forest";
        public string tilesetName = "Forest Tileset";
        
        [Title("Level Budget")]
        [Range(3, 5)]
        public int hazardCount = 3;
        
        [Range(1, 2)]
        public int enemyArchetypes = 1;
        
        [Range(1, 3)]
        public int checkpointCount = 2;
        
        public bool hasPowerUp = true;
        public bool hasSecret = true;
        
        [Title("Level Bounds")]
        public Vector2 startPosition = Vector2.zero;
        public Vector2 endPosition = new Vector2(100, 0);
        public Vector2 boundsMin = new Vector2(-10, -10);
        public Vector2 boundsMax = new Vector2(110, 20);
        
        [Title("Narrative")]
        [TextArea(3, 6)]
        public string levelIntro = "Level introduction text";
        
        [TextArea(3, 6)]
        public string levelOutro = "Level completion text";
        
        [Title("Collectibles")]
        public int collectibleCount = 5;
        public string collectibleTheme = "War Medals";
        
        [Title("Performance")]
        public int maxParallaxLayers = 2;
        public bool enableDynamicLighting = false;
        
        [Button("Validate Level")]
        private void ValidateLevel()
        {
            Debug.Log($"Validating {levelName}: {hazardCount} hazards, {enemyArchetypes} enemy types, {checkpointCount} checkpoints");
        }
    }
}