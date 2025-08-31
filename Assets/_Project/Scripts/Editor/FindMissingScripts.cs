using UnityEngine;
using UnityEditor;
using System.Linq;

namespace WWIII.Editor
{
    /// <summary>
    /// Unity AI recommended utility to find and fix missing script references
    /// Addresses console error: "The referenced script (Unknown) on this Behaviour is missing!"
    /// </summary>
    public static class FindMissingScripts
    {
        [MenuItem("WWIII/🔍 FIND MISSING SCRIPTS")]
        public static void FindMissingScriptsInScene()
        {
            Debug.Log("🔍 Scanning for missing script references...");
            
            int missingCount = 0;
            
            // Find all GameObjects in scene
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            
            foreach (GameObject go in allObjects)
            {
                Component[] components = go.GetComponents<Component>();
                
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        missingCount++;
                        string path = GetGameObjectPath(go);
                        Debug.LogError($"❌ Missing script on: {path} (Component {i})", go);
                        
                        // Auto-remove missing scripts
                        var serializedObject = new SerializedObject(go);
                        var componentsProperty = serializedObject.FindProperty("m_Component");
                        
                        for (int j = componentsProperty.arraySize - 1; j >= 0; j--)
                        {
                            var componentRef = componentsProperty.GetArrayElementAtIndex(j);
                            if (componentRef.FindPropertyRelative("component").objectReferenceValue == null)
                            {
                                Debug.Log($"🗑️  Removing missing script component {j} from {path}");
                                componentsProperty.DeleteArrayElementAtIndex(j);
                            }
                        }
                        
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            
            if (missingCount == 0)
            {
                Debug.Log("✅ No missing scripts found in scene!");
            }
            else
            {
                Debug.Log($"🧹 Found and cleaned {missingCount} missing script references");
                EditorUtility.DisplayDialog("Missing Scripts Cleanup", 
                    $"Found and removed {missingCount} missing script references.\\n\\n" +
                    "Check the Console for details.", "OK");
            }
        }
        
        [MenuItem("WWIII/🔍 FIND MISSING SCRIPTS IN PROJECT")]
        public static void FindMissingScriptsInProject()
        {
            Debug.Log("🔍 Scanning project for missing script references...");
            
            // Find all prefabs in project
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
            int totalMissing = 0;
            
            foreach (string guid in prefabGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    int missingInPrefab = CheckPrefabForMissingScripts(prefab, path);
                    totalMissing += missingInPrefab;
                }
            }
            
            if (totalMissing == 0)
            {
                Debug.Log("✅ No missing scripts found in project prefabs!");
            }
            else
            {
                Debug.Log($"⚠️  Found {totalMissing} missing script references in project prefabs");
            }
        }
        
        private static int CheckPrefabForMissingScripts(GameObject prefab, string path)
        {
            int missing = 0;
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            
            foreach (Component component in components)
            {
                if (component == null)
                {
                    missing++;
                    Debug.LogError($"❌ Missing script in prefab: {path}", prefab);
                }
            }
            
            return missing;
        }
        
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return "/" + path;
        }
    }
}