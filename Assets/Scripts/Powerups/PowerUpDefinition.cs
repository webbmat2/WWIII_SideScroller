using UnityEngine;

namespace WWIII.SideScroller.Powerups
{
    [CreateAssetMenu(menuName = "WWIII/Powerups/Definition", fileName = "PowerUp")]
    public class PowerUpDefinition : ScriptableObject
    {
        public string displayName;
        public float duration = 10f;
        [Header("Effects")]
        public bool invulnerable;
        public float speedMultiplier = 1f;
        public float healAmount = 0f;
    }
}

