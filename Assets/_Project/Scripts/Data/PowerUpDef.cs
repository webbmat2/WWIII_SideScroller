using UnityEngine;

namespace WWIII.Data
{
    /// <summary>
    /// ScriptableObject definition for power-ups and weapon upgrades
    /// Maps to Corgi Engine abilities and weapon systems
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUp_", menuName = "WWIII/Data/Power-Up Definition")]
    public class PowerUpDef : ScriptableObject
    {
        [Header("Power-Up Info")]
        public string displayName = "New Power-Up";
        public string description = "A powerful upgrade for the player";
        public Sprite icon;
        public PowerUpType effectType = PowerUpType.SpeedBoost;
        
        [Header("Effect Configuration")]
        [Range(1.1f, 3f)]
        public float multiplier = 1.5f;
        
        [Range(5f, 60f)]
        public float duration = 15f; // Duration in seconds
        
        public bool isPermanent = false;
        
        [Header("Visual Effects")]
        public Color effectColor = Color.yellow;
        public GameObject pickupEffect;
        public GameObject activeEffect;
        
        [Header("Audio")]
        public AudioClip pickupSound;
        public AudioClip activeSound;
        
        [Header("Rarity & Cost")]
        public PowerUpRarity rarity = PowerUpRarity.Common;
        public int cost = 100; // For shop or upgrade system
        
        [Header("Requirements")]
        public string[] requiredAbilities; // Prerequisites
        public int minLevelRequired = 1;
    }

    public enum PowerUpType
    {
        SpeedBoost,      // Increases movement speed
        JumpBoost,       // Increases jump height
        WeaponUpgrade,   // Improves weapon damage/rate
        HealthBoost,     // Increases max health
        ArmorBoost,      // Temporary invincibility
        DoubleJump,      // Adds double jump ability
        WallClimb,       // Adds wall climbing
        Dash,            // Adds dash ability
        SlowMotion,      // Slows time temporarily
        MagnetCoins      // Attracts collectibles
    }

    public enum PowerUpRarity
    {
        Common,    // White
        Uncommon,  // Green  
        Rare,      // Blue
        Epic,      // Purple
        Legendary  // Orange
    }
}