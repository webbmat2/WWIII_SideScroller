// Editor batch normalizer for Rigidbody2D constraints in Unity 6000.2
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace WWIII.EditorTools
{
    public static class Rigidbody2DConstraintNormalizer
    {
        [MenuItem("WWIII/Physics2D/Normalize Rigidbody2D Constraints (Selected)")]
        public static void NormalizeSelected()
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                Debug.LogWarning("[RB2D Normalizer] Select one or more GameObjects.");
                return;
            }
            foreach (var go in selection)
            {
                foreach (var rb in go.GetComponentsInChildren<Rigidbody2D>(true))
                {
                    Normalize(rb);
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("WWIII/Physics2D/Normalize Rigidbody2D Constraints (Scene)")]
        public static void NormalizeScene()
        {
            var rbs = Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var rb in rbs)
            {
                Normalize(rb);
            }
            AssetDatabase.SaveAssets();
        }

        private static void Normalize(Rigidbody2D rb)
        {
            if (!rb) return;
            Undo.RecordObject(rb, "Normalize RB2D Constraints");

            // Clear and assign exact mask. Avoid |= or &= with FreezeAll.
            rb.freezeRotation = false;                       // keep API parity
            rb.constraints = RigidbodyConstraints2D.None;    // clear every bit first
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Persist to serialization and prefab overrides
            EditorUtility.SetDirty(rb);
            PrefabUtility.RecordPrefabInstancePropertyModifications(rb);

            // Verify raw serialized int (4 == FreezeRotation)
            var so = new SerializedObject(rb);
            var p = so.FindProperty("m_Constraints");
            if (p != null && p.intValue != 4)
            {
                p.intValue = 4;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(rb);
            }

            Debug.Log($"[RB2D Normalizer] {GetPath(rb.gameObject)} -> constraints=FreezeRotation (m_Constraints=4)");
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var t = go.transform;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
#endif

