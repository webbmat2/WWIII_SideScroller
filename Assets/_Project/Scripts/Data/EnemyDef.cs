using UnityEngine;

namespace WWIII.Data
{
    /// <summary>
    /// ScriptableObject definition for enemy characters
    /// Maps to Corgi Engine AI and enemy systems
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_", menuName = "WWIII/Data/Enemy Definition")]
    public class EnemyDef : ScriptableObject
    {
        [Header("Enemy Info")]
        public string displayName = "New Enemy";
        public string description = "A dangerous foe";
        public Sprite icon;
        public EnemyType enemyType = EnemyType.Soldier;
        
        [Header("Combat Stats")]
        [Range(1, 200)]
        public int health = 50;
        
        [Range(1, 50)]
        public int damage = 10;
        
        [Range(0.5f, 10f)]
        public float attackRate = 2f; // Attacks per second
        
        [Range(1f, 15f)]
        public float detectionRange = 8f; // Units
        
        [Header("Movement")]
        [Range(1f, 12f)]
        public float moveSpeed = 4f;
        
        [Range(0f, 8f)]
        public float jumpHeight = 0f; // 0 = can't jump
        
        public bool canFly = false;
        public bool canClimb = false;
        
        [Header("AI Behavior")]
        public AIBehaviorType behavior = AIBehaviorType.Patrol;
        
        [Range(2f, 20f)]
        public float patrolDistance = 8f;
        
        [Range(0.5f, 5f)]
        public float patrolPauseDuration = 1f;
        
        public bool fleesWhenLowHealth = false;
        
        [Range(0.1f, 0.9f)]
        public float fleeHealthPercentage = 0.3f;
        
        [Header("Visual & Audio")]
        public RuntimeAnimatorController animatorController;
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        
        [Header("Drops & Rewards")]
        public int scoreValue = 100;
        public DropItem[] possibleDrops;
        
        [Header("Special Abilities")]
        public bool canShoot = false;
        public GameObject projectilePrefab;
        
        [Range(0.5f, 5f)]
        public float shootRate = 1f;
        
        public bool hasShield = false;
        public bool canTeleport = false;
        
        [Header("Spawn Configuration")]
        public float spawnWeight = 1f; // Relative spawn chance
        public int minLevelToSpawn = 1;
        public BiomeType[] allowedBiomes;
    }

    public enum EnemyType
    {
        Soldier,      // Basic infantry
        Heavy,        // Tank-like enemy
        Scout,        // Fast, low health
        Sniper,       // Long range attacker
        Drone,        // Flying enemy
        Mech,         // Large mechanical
        Elite,        // Mini-boss type
        Boss          // Level boss
    }

    public enum AIBehaviorType
    {
        Patrol,       // Walks back and forth
        Guard,        // Stands still, attacks when player near
        Chase,        // Actively hunts player
        Flee,         // Runs away from player
        Ambush,       // Hides until player approaches
        Swarm,        // Groups with other enemies
        Ranged        // Keeps distance, shoots
    }

    public enum BiomeType
    {
        Urban,        // City environments
        Industrial,   // Factory areas
        Underground,  // Sewer/metro
        Military,     // Bases and compounds
        Wasteland,    // Destroyed areas
        Any           // Can spawn anywhere
    }

    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;
        
        [Range(0f, 1f)]
        public float dropChance = 0.3f;
        
        [Range(1, 10)]
        public int quantity = 1;
    }
}