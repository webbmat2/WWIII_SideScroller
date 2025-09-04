using UnityEngine;

namespace WWIII.SideScroller.Level
{
    [CreateAssetMenu(menuName = "WWIII/Level/Level Definition", fileName = "LevelDefinition")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string designId;
        public string displayName;
        [Tooltip("Chronological age in years for this level's narrative context.")]
        public int ageYears;

        [Header("Scene")]
        [Tooltip("Unity scene name for this level (added to Build Settings or Addressables).")]
        public string sceneName;
        [Tooltip("Optional Addressables key to load this scene additively at runtime.")]
        public string addressablesKey;

        [Header("Collectibles")]
        [Tooltip("Number of photo collectibles present in this level (for UI/achievement display only).")]
        public int photoCount = 5;

        [Header("Boss & Audio")]
        public string bossId;
        public string musicCue;
        public string ambientCue;

        [Header("Design Hooks")]
        [Tooltip("Optional spawn spec asset for this level")] public LevelSpawnSpec spawnSpec;

        [Header("Summary")]
        public string summary;
    }
}
