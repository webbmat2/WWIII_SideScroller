// Editor utility to definitively repair Rigidbody2D constraints serialization in Unity 6000.2
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Integration.Editor
{
    public static class PlayerPhysicsConstraintRepair
    {
        [MenuItem("Tools/Physics2D/Repair Rigidbody2D Constraints (Unity 6000.2)")]
        public static void RepairSelected()
        {
            var objs = Selection.gameObjects;
            if (objs == null || objs.Length == 0)
            {
                Debug.LogWarning("[ConstraintRepair] Select one or more GameObjects with Rigidbody2D.");
                return;
            }

            int fixedCount = 0;
            foreach (var go in objs)
            {
                foreach (var rb in go.GetComponentsInChildren<Rigidbody2D>(true))
                {
                    var so = new SerializedObject(rb);
                    var prop = so.FindProperty("m_Constraints");
                    if (prop == null) continue;

                    prop.intValue = (int)RigidbodyConstraints2D.FreezeRotation;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(rb);
                    fixedCount++;
                    Debug.Log($"[ConstraintRepair] {GetPath(rb.gameObject)} -> m_Constraints={(int)RigidbodyConstraints2D.FreezeRotation}");
                }
            }

            if (fixedCount == 0)
            {
                Debug.Log("[ConstraintRepair] No Rigidbody2D components found in selection.");
            }
            else
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"[ConstraintRepair] Repaired constraints on {fixedCount} Rigidbody2D component(s).");
            }
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

