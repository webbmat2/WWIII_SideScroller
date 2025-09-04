using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class SortingLayerUtility
    {
        public static void EnsureSortingLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("m_SortingLayers");

            void Ensure(string name, int uniqueId)
            {
                bool exists = false;
                for (int i = 0; i < layersProp.arraySize; i++)
                {
                    var sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp.FindPropertyRelative("name").stringValue == name)
                    {
                        exists = true; break;
                    }
                }
                if (!exists)
                {
                    layersProp.arraySize++;
                    var sp = layersProp.GetArrayElementAtIndex(layersProp.arraySize - 1);
                    sp.FindPropertyRelative("name").stringValue = name;
                    sp.FindPropertyRelative("uniqueID").intValue = uniqueId;
                }
            }

            Ensure("Background", 33201);
            Ensure("Environment", 33202);
            Ensure("Characters", 33203);
            Ensure("Collectibles", 33204);
            Ensure("Foreground", 33205);

            tagManager.ApplyModifiedProperties();
        }
    }
}

