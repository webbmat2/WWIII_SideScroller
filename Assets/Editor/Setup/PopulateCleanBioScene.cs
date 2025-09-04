#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WWIII.SideScroller.EditorTools
{
    public static class PopulateCleanBioScene
    {
        [MenuItem("WWIII/Setup/Populate Clean BioLevel Scene")] 
        public static void Populate()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid()) { Debug.LogWarning("[Setup] Open a scene first."); return; }

            // Player
            var player = GameObject.Find("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                Undo.RegisterCreatedObjectUndo(player, "Create Player");
            }
            EnsureLayer(player, "Player");
            EnsureComponent<Rigidbody2D>(player, rb => {
                rb.gravityScale = 3;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            });
            EnsureComponent<BoxCollider2D>(player, bc => {
                if (bc.size == Vector2.zero) bc.size = new Vector2(0.8f, 1.8f);
                bc.offset = new Vector2(0, bc.size.y * 0.5f);
                bc.isTrigger = false;
            });
            EnsureComponent(player, "MoreMountains.CorgiEngine.CorgiController");
            EnsureComponent(player, "MoreMountains.CorgiEngine.Character");
            EnsureComponent(player, "MoreMountains.CorgiEngine.CharacterHorizontalMovement");
            EnsureComponent(player, "MoreMountains.CorgiEngine.CharacterJump");
            EnsureComponent(player, "WWIII.SideScroller.Integration.PlayerPhysicsAutoFixer");
            EnsureComponent(player, "WWIII.SideScroller.Integration.Rigidbody2DConstraintGuard");
            EnsureComponent(player, "WWIII.SideScroller.Integration.ConstraintCorruptionFixer");

            // Ground
            var ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = new GameObject("Ground");
                Undo.RegisterCreatedObjectUndo(ground, "Create Ground");
                ground.transform.position = new Vector3(0, -1, 0);
            }
            EnsureLayer(ground, "Ground", 3);
            EnsureComponent<Rigidbody2D>(ground, rb => rb.bodyType = RigidbodyType2D.Static);
            EnsureComponent<BoxCollider2D>(ground, bc => { bc.size = new Vector2(500, 2); bc.offset = new Vector2(0, -1); });

            // Camera
            var cam = Camera.main != null ? Camera.main.gameObject : null;
            if (cam == null)
            {
                var cameraGO = new GameObject("Main Camera");
                Undo.RegisterCreatedObjectUndo(cameraGO, "Create Camera");
                cam = cameraGO;
                var camera = cameraGO.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 6;
                cameraGO.tag = "MainCamera";
            }
            // Cinemachine if available
            var vcamType = Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
            if (vcamType != null)
            {
                var vcamGO = GameObject.Find("CM vcam1") ?? new GameObject("CM vcam1");
                if (vcamGO.transform.parent == null) Undo.RegisterCreatedObjectUndo(vcamGO, "Create VCam");
                var vcam = vcamGO.GetComponent(vcamType) ?? vcamGO.AddComponent(vcamType);
                // Assign Follow via serialized property
                var so = new SerializedObject(vcam as UnityEngine.Object);
                var followProp = so.FindProperty("m_Follow");
                if (followProp != null) { followProp.objectReferenceValue = player.transform; so.ApplyModifiedPropertiesWithoutUndo(); }
            }
            else
            {
                var follow = cam.GetComponent<WWIII.SideScroller.Integration.SimpleCameraFollow>() ?? cam.AddComponent<WWIII.SideScroller.Integration.SimpleCameraFollow>();
                follow.target = player.transform;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Setup] Populated clean bio scene: Player, Ground, Camera wired. Press Play to test movement.");
        }

        [MenuItem("WWIII/Setup/Place Player Above Ground")]
        public static void PlacePlayerAboveGround()
        {
            var player = GameObject.Find("Player");
            if (player == null) { Debug.LogWarning("[Setup] No Player."); return; }
            float groundTopY = 0f;

            var groundGO = GameObject.Find("Ground");
            if (groundGO != null)
            {
                var bc = groundGO.GetComponent<BoxCollider2D>();
                if (bc != null)
                {
                    groundTopY = groundGO.transform.position.y + bc.offset.y + bc.size.y * 0.5f;
                }
                else
                {
                    var cc = groundGO.GetComponent<UnityEngine.CompositeCollider2D>();
                    if (cc != null) groundTopY = cc.bounds.max.y;
                }
            }
            player.transform.position = new Vector3(0f, groundTopY + 2f, 0f);
            Debug.Log($"[Setup] Placed Player at y={player.transform.position.y} above ground (top y={groundTopY}).");
        }

        private static void EnsureComponent(GameObject go, string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                // try infer assembly-qualified names commonly used
                type = Type.GetType(typeName + ", WWIII.SideScroller") ?? Type.GetType(typeName + ", MoreMountains.CorgiEngine");
            }
            if (type != null && go.GetComponent(type) == null)
            {
                Undo.AddComponent(go, type);
            }
        }

        private static void EnsureComponent<T>(GameObject go, Action<T> configure) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp == null) comp = Undo.AddComponent<T>(go);
            configure?.Invoke(comp);
        }

        private static void EnsureLayer(GameObject go, string layerName, int fallbackIndex = -1)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0) layer = fallbackIndex;
            if (layer >= 0 && go.layer != layer) go.layer = layer;
        }
    }
}
#endif
