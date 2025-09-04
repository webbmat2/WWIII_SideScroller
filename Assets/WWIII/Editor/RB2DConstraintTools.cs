#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WWIII.SideScroller.EditorTools
{
    public static class RB2DConstraintTools
    {
        private const RigidbodyConstraints2D Desired = RigidbodyConstraints2D.FreezeRotation;

        [MenuItem("WWIII/Physics2D/Fix Constraints In Open Scene")]
        public static void FixOpenScene()
        {
            FixAllBodiesInScene(SceneManager.GetActiveScene());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[RB2DConstraintTools] Fixed constraints in open scene.");
        }

        [MenuItem("WWIII/Physics2D/Fix Constraints In All Prefabs And Scenes")]
        public static void FixProject()
        {
            // Prefabs
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                int fixedCount = FixAllBodiesInHierarchy(root);
                if (fixedCount > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    Debug.Log($"[RB2DConstraintTools] Fixed {fixedCount} RB2D in prefab {path}");
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            // Scenes
            var current = SceneManager.GetActiveScene().path;
            foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int fixedCount = FixAllBodiesInScene(scene);
                if (fixedCount > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[RB2DConstraintTools] Fixed {fixedCount} RB2D in scene {path}");
                }
            }
            if (!string.IsNullOrEmpty(current))
            {
                EditorSceneManager.OpenScene(current);
            }

            // Flush
            AssetDatabase.ForceReserializeAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RB2DConstraintTools] Project-wide constraints fix complete.");
        }

        [MenuItem("WWIII/Physics2D/Verify Selected Rigidbody2D")]
        public static void VerifySelected()
        {
            foreach (var go in Selection.gameObjects)
            {
                var rb = go.GetComponent<Rigidbody2D>();
                if (!rb) continue;
                var so = new SerializedObject(rb);
                var p = so.FindProperty("m_Constraints");
                Debug.Log($"[RB2DConstraintTools] {GetPath(go)} runtime={rb.constraints} serialized={(p != null ? p.intValue.ToString() : "<n/a>")}");
            }
        }

        private static int FixAllBodiesInScene(Scene scene)
        {
            int count = 0;
            foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (Fix(rb)) count++;
            }
            return count;
        }

        private static int FixAllBodiesInHierarchy(GameObject root)
        {
            int count = 0;
            foreach (var rb in root.GetComponentsInChildren<Rigidbody2D>(true))
            {
                if (Fix(rb)) count++;
            }
            return count;
        }

        private static bool Fix(Rigidbody2D rb)
        {
            bool changed = false;
            if (rb == null) return false;

            // Clear then set exact mask; avoid lingering FreezeAll
            if (rb.constraints != Desired)
            {
                Undo.RecordObject(rb, "Fix RB2D Constraints");
                rb.freezeRotation = false;
                rb.constraints = RigidbodyConstraints2D.None;
                rb.constraints = Desired;
                EditorUtility.SetDirty(rb);
                PrefabUtility.RecordPrefabInstancePropertyModifications(rb);
                changed = true;
            }

            // Persist serialized int
            var so = new SerializedObject(rb);
            var p = so.FindProperty("m_Constraints");
            if (p != null && p.intValue != (int)Desired)
            {
                p.intValue = (int)Desired;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(rb);
                PrefabUtility.RecordPrefabInstancePropertyModifications(rb);
                changed = true;
            }

            return changed;
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var t = go.transform;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
    }
}
#endif

