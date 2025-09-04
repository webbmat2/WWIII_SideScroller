#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WWIII.SideScroller.EditorTools
{
    public static class ConstraintFixTools
    {
        [MenuItem("WWIII/Fix/Force Scene Reserialize (Current)")]
        public static void ForceSceneReserialize()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("[Fix] No active scene.");
                return;
            }

            // Ensure text serialization and visible meta files are set in ProjectSettings manually by user.
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            var path = scene.path;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[Fix] Scene not saved yet; please save it first.");
                return;
            }

            AssetDatabase.ForceReserializeAssets(new List<string> { path });
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Fix common corruption by writing exact constraints mask
            var rbs = Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int fixedCount = 0;
            foreach (var rb in rbs)
            {
                FixConstraints(rb, ref fixedCount);
            }
            if (fixedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
            Debug.Log($"[Fix] Reserialized and normalized RB2D constraints. Fixed={fixedCount}");
        }

        [MenuItem("WWIII/Fix/Scan & Fix Rigidbody2D Constraints (Scene)")]
        public static void ScanAndFixConstraintsScene()
        {
            var rbs = Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int fixedCount = 0;
            foreach (var rb in rbs) FixConstraints(rb, ref fixedCount);
            if (fixedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
            Debug.Log($"[Fix] RB2D constraints normalized in scene: {fixedCount}");
        }

        [MenuItem("WWIII/Diagnostics/Print RB2D Serialized Constraints (Selected)")]
        public static void PrintSelectedConstraints()
        {
            foreach (var go in Selection.gameObjects)
            {
                var rb = go.GetComponent<Rigidbody2D>();
                if (!rb) continue;
                var so = new SerializedObject(rb);
                var prop = so.FindProperty("m_Constraints");
                Debug.Log($"[Diag] {GetPath(go)} runtime={rb.constraints} serialized={(prop != null ? prop.intValue : -1)}");
            }
        }

        [MenuItem("WWIII/Fix/Create Clean BioLevel Scene")] 
        public static void CreateCleanBioLevelScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Player
            var player = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(player, "Create Player");
            player.layer = LayerMask.NameToLayer("Player");
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = player.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.8f);
            col.offset = new Vector2(0, 0.9f);
            // Minimal Corgi stack if available
            TryAdd(player, "MoreMountains.CorgiEngine.CorgiController");
            TryAdd(player, "MoreMountains.CorgiEngine.Character");
            TryAdd(player, "MoreMountains.CorgiEngine.CharacterHorizontalMovement");
            TryAdd(player, "MoreMountains.CorgiEngine.CharacterJump");

            // Ground
            var ground = new GameObject("Ground");
            Undo.RegisterCreatedObjectUndo(ground, "Create Ground");
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = 3;
            ground.layer = groundLayer;
            var grb = ground.AddComponent<Rigidbody2D>();
            grb.bodyType = RigidbodyType2D.Static;
            var gcol = ground.AddComponent<BoxCollider2D>();
            gcol.size = new Vector2(500, 2);
            gcol.offset = new Vector2(0, -1);
            ground.transform.position = new Vector3(0, -1, 0);

            var path = "Assets/WWIII/Scenes/BioLevel_Age7_Clean.unity";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[Fix] Created clean scene at {path}");
        }

        [MenuItem("WWIII/Fix/Reserialize All Assets (Project)")]
        public static void ReserializeAllAssets()
        {
            AssetDatabase.ForceReserializeAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Fix] Forced reserialize of all assets.");
        }

        private static void FixConstraints(Rigidbody2D rb, ref int fixedCount)
        {
            if (!rb) return;
            // Clear any stale bits then set the exact mask
            Undo.RecordObject(rb, "Normalize RB2D Constraints");
            rb.freezeRotation = false; // avoid legacy bool interference
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            // Persist serialized int
            var so = new SerializedObject(rb);
            var prop = so.FindProperty("m_Constraints");
            if (prop != null && prop.intValue != (int)RigidbodyConstraints2D.FreezeRotation)
            {
                prop.intValue = (int)RigidbodyConstraints2D.FreezeRotation;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(rb);
            EditorUtility.SetDirty(rb);
            fixedCount++;
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var t = go.transform;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }

        private static void TryAdd(GameObject go, string typeName)
        {
            var type = System.Type.GetType(typeName + ", MoreMountains.CorgiEngine");
            if (type == null) type = System.Type.GetType(typeName);
            if (type != null) go.AddComponent(type);
        }
    }
}
#endif

