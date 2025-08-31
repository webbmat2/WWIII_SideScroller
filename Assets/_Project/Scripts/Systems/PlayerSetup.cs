using UnityEngine;
using WWIII.Core;

namespace WWIII.Systems
{
    /// <summary>
    /// Runtime script for creating player GameObjects with all necessary components
    /// Note: PlayerInput component creation temporarily disabled due to assembly isolation
    /// </summary>
    public static class PlayerSetup
    {
        public static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(-10, 2, 0);
            
            // Add visual representation with Bezi character sprite
            var playerSprite = player.AddComponent<SpriteRenderer>();
            var beziSprite = Resources.Load<Sprite>("Bezi_Character");
            if (beziSprite != null)
            {
                playerSprite.sprite = beziSprite;
                Debug.Log("✅ Bezi character sprite loaded successfully!");
            }
            else
            {
                // Fallback to blue color if sprite not found
                playerSprite.color = Color.blue;
                Debug.LogWarning("⚠️ Bezi character sprite not found in Resources folder! Using blue placeholder.");
            }
            playerSprite.sortingOrder = 1;
            
            // Add physics components with Unity AI optimized settings
            var rb = player.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"✅ Rigidbody2D Constraints Set in PlayerSetup: {rb.constraints}");
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            var playerCollider = player.AddComponent<CapsuleCollider2D>();
            playerCollider.size = new Vector2(0.6f, 1.8f);
            playerCollider.offset = new Vector2(0, 0.9f);
            
            // Add Unity AI optimized PlayerController
            player.AddComponent<PlayerController>();
            
            // Note: PlayerInput should be added manually in the Editor for now
            Debug.Log("💡 Remember to add PlayerInput component manually in the Editor");
            
            // Create GroundCheck child object
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform);
            groundCheck.transform.localPosition = Vector3.down * 0.9f;
            
            // Connect ground check to PlayerController
            var controller = player.GetComponent<PlayerController>();
            controller.groundCheck = groundCheck.transform;
            controller.groundLayer = 1 << LayerMask.NameToLayer("Ground");
            
            Debug.Log("✅ Player created with Unity AI PlayerController setup!");
            
            return player;
        }
    }
}