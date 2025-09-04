using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace WWIII.SideScroller.Editor.Characters
{
    public static class Age7CharacterSetup
    {
        [MenuItem("WWIII/Characters/Setup Age7 Player (Corgi)")]
        public static void SetupPlayer()
        {
            var ageMgrType = Type.GetType("WWIII.SideScroller.Aging.AgeManager, WWIII.SideScroller");
            if (ageMgrType == null)
            {
                Debug.LogError("Age7CharacterSetup: AgeManager type not found.");
                return;
            }

            var ageMgr = UnityEngine.Object.FindFirstObjectByType(ageMgrType);
            Transform playerRoot = null;
            GameObject playerGO = null;
            if (ageMgr != null)
            {
                var so = new SerializedObject(ageMgr as UnityEngine.Object);
                var currentPlayerProp = so.FindProperty("currentPlayer");
                var rootProp = so.FindProperty("playerRoot");
                playerGO = currentPlayerProp != null ? currentPlayerProp.objectReferenceValue as GameObject : null;
                playerRoot = rootProp != null ? rootProp.objectReferenceValue as Transform : null;
            }

            if (playerGO == null)
            {
                // Fallbacks
                var ageSystem = GameObject.Find("AgeSystem");
                if (ageSystem != null)
                {
                    playerGO = ageSystem.transform.Find("Player")?.gameObject;
                }
                if (playerGO == null)
                {
                    playerGO = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
                }
            }

            if (playerGO == null)
            {
                Debug.LogError("Age7CharacterSetup: Could not find Player GameObject. Ensure /AgeSystem/Player exists.");
                return;
            }

            // Ensure required components via reflection to avoid hard editor asmdef refs
            AddIfFound(playerGO, "MoreMountains.CorgiEngine.Character, MoreMountains.CorgiEngine");
            AddIfFound(playerGO, "MoreMountains.CorgiEngine.CharacterHorizontalMovement, MoreMountains.CorgiEngine");
            AddIfFound(playerGO, "MoreMountains.CorgiEngine.CharacterJump, MoreMountains.CorgiEngine");
            AddIfFound(playerGO, "MoreMountains.CorgiEngine.InputManager, MoreMountains.CorgiEngine");

            var rb2d = playerGO.GetComponent<Rigidbody2D>() ?? playerGO.AddComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.mass = 1f;
            rb2d.drag = 0f;
            rb2d.angularDrag = 0.05f;
            rb2d.gravityScale = 3f;
            rb2d.freezeRotation = true;

            var capsule = playerGO.GetComponent<CapsuleCollider2D>() ?? playerGO.AddComponent<CapsuleCollider2D>();
            capsule.isTrigger = false;
            capsule.size = new Vector2(0.6f, 1.2f);
            capsule.offset = new Vector2(0f, 0.6f);

            // Tag/layer
            playerGO.tag = "Player";
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) playerLayer = 11; // default to 11
            playerGO.layer = playerLayer;

            // Ensure our Age7Character + input handler
            if (playerGO.GetComponent<WWIII.SideScroller.Characters.Age7Character>() == null)
                playerGO.AddComponent<WWIII.SideScroller.Characters.Age7Character>();

            if (playerGO.GetComponent<WWIII.SideScroller.Characters.Age7InputHandler>() == null)
                playerGO.AddComponent<WWIII.SideScroller.Characters.Age7InputHandler>();

            // Save changes
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("WWIII", "Age 7 Player setup complete (Corgi components + physics + colliders).", "OK");
        }

        private static Component AddIfFound(GameObject go, string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                Debug.LogWarning($"Age7CharacterSetup: Type not found: {typeName}");
                return null;
            }
            return go.GetComponent(type) ?? go.AddComponent(type);
        }
    }
}

