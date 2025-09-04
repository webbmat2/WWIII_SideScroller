using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Save;
using WWIII.SideScroller.Input;
using WWIII.SideScroller.Utils;
using WWIII.SideScroller.Integration.AssetStreaming;

namespace WWIII.SideScroller.Editor.Bootstrap
{
    public static class CreateBootstrapPrefab
    {
        [MenuItem("WWIII/Bootstrap/Create WWIII_Bootstrap Prefab")] 
        public static void Create()
        {
            var root = new GameObject("WWIII_Bootstrap");

            // Core services
            root.AddComponent<ProgressionSaveService>();
            var h = root.AddComponent<ControllerHapticsService>(); h.enableInEditor = true;
            root.AddComponent<SessionPerformanceMonitor>();
            root.AddComponent<AgeAddressablesLoader>();

            // Save as prefab
            var folder = "Assets/WWIII/Prefabs/Bootstrap";
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, "WWIII_Bootstrap.prefab").Replace('\\','/');
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            EditorGUIUtility.PingObject(prefab);
            EditorUtility.DisplayDialog("WWIII", $"Bootstrap prefab created at {path}", "OK");
        }
    }
}

