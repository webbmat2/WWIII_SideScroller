using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MoreMountains.CorgiEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Editor.Characters
{
    public static class Age7CharacterSetup
    {
        [MenuItem("WWIII/Characters/Setup Age7 Player (Corgi)")]
        public static void SetupAge7Player()
        {
            GameObject player = FindPlayerGameObject();
            if (player == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find Player GameObject. Please ensure /AgeSystem/Player exists or a GameObject with Player tag is present.", "OK");
                return;
            }

            AddCorgiComponents(player);
            AddUnityPhysicsComponents(player);
            AddAge7Components(player);
            SetupTagsAndLayers(player);

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(player.scene);

            Debug.Log($"[Age7CharacterSetup] Successfully set up Age 7 character on: {player.name}");
            EditorUtility.DisplayDialog("Success", $"Age 7 character setup completed on: {player.name}", "OK");
        }

        private static GameObject FindPlayerGameObject()
        {
            var ageManager = Object.FindFirstObjectByType<AgeManager>();
            if (ageManager != null && ageManager.currentPlayer != null)
            {
                return ageManager.currentPlayer;
            }

            var ageSystem = GameObject.Find("AgeSystem");
            if (ageSystem != null)
            {
                var playerTransform = ageSystem.transform.Find("Player");
                if (playerTransform != null)
                {
                    return playerTransform.gameObject;
                }
            }

            return GameObject.FindWithTag("Player");
        }

        private static void AddCorgiComponents(GameObject player)
        {
            // Character
            var character = player.GetComponent<Character>();
            if (character == null)
            {
                character = player.AddComponent<Character>();
                Debug.Log("[Age7CharacterSetup] Added Character component");
            }
            character.CharacterType = Character.CharacterTypes.Player;

            // Walk movement
            var movement = player.GetComponent<CharacterHorizontalMovement>();
            if (movement == null)
            {
                movement = player.AddComponent<CharacterHorizontalMovement>();
                Debug.Log("[Age7CharacterSetup] Added CharacterHorizontalMovement component");
            }
            movement.WalkSpeed = 8f;

            // Jump ability
            var jump = player.GetComponent<CharacterJump>();
            if (jump == null)
            {
                jump = player.AddComponent<CharacterJump>();
                Debug.Log("[Age7CharacterSetup] Added CharacterJump component");
            }
            jump.JumpHeight = 12f;
            jump.CoyoteTime = 0.2f;
            jump.NumberOfJumps = 1;

            // Run ability
            var run = player.GetComponent<CharacterRun>();
            if (run == null)
            {
                run = player.AddComponent<CharacterRun>();
                Debug.Log("[Age7CharacterSetup] Added CharacterRun component");
            }
            run.RunSpeed = 12f;

            // InputManager singleton (create if missing)
            if (InputManager.Instance == null)
            {
                player.AddComponent<InputManager>();
                Debug.Log("[Age7CharacterSetup] Added InputManager component");
            }
        }

        private static void AddUnityPhysicsComponents(GameObject player)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = player.AddComponent<Rigidbody2D>();
                Debug.Log("[Age7CharacterSetup] Added Rigidbody2D component");
            }
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var collider = player.GetComponent<CapsuleCollider2D>();
            if (collider == null)
            {
                collider = player.AddComponent<CapsuleCollider2D>();
                Debug.Log("[Age7CharacterSetup] Added CapsuleCollider2D component");
            }
            collider.size = new Vector2(0.6f, 1.2f);
            collider.offset = new Vector2(0f, 0.6f);
            collider.isTrigger = false;
        }

        private static void AddAge7Components(GameObject player)
        {
            if (player.GetComponent<WWIII.SideScroller.Characters.Age7Character>() == null)
            {
                player.AddComponent<WWIII.SideScroller.Characters.Age7Character>();
                Debug.Log("[Age7CharacterSetup] Added Age7Character component");
            }

            if (player.GetComponent<WWIII.SideScroller.Characters.Age7InputHandler>() == null)
            {
                player.AddComponent<WWIII.SideScroller.Characters.Age7InputHandler>();
                Debug.Log("[Age7CharacterSetup] Added Age7InputHandler component");
            }
        }

        private static void SetupTagsAndLayers(GameObject player)
        {
            player.tag = "Player";
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer == -1) playerLayer = 11;
            player.layer = playerLayer;

            Debug.Log($"[Age7CharacterSetup] Set tag to 'Player' and layer to {playerLayer}");
        }
    }
}

