using UnityEngine;

namespace WWIII.SideScroller.Design
{
    [CreateAssetMenu(menuName = "WWIII/Design/Enemy", fileName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string designId;
        public string displayName;
        [Tooltip("Asset path to prefab under Assets/, or Addressables key")] public string prefabKey;
        [Header("Stats")]
        public float hp = 10f;
        public float damage = 1f;
        public float speed = 3f;
        [Tooltip("AI type key: patrol, swarm, chase, stealth")]
        public string aiType = "patrol";
    }
}
