using UnityEngine;

namespace WWIII.SideScroller.Design
{
    [CreateAssetMenu(menuName = "WWIII/Design/Collectible", fileName = "Collectible")]
    public class CollectibleDefinition : ScriptableObject
    {
        public string designId;
        public string displayName;
        [Tooltip("Asset path to prefab under Assets/, or Addressables key if you prefer")] public string prefabKey;
        [Tooltip("Points or weight for reveal progress")] public int points = 1;
        [Tooltip("If true, this is a photo shard/memory item")] public bool isPhoto;
    }
}
