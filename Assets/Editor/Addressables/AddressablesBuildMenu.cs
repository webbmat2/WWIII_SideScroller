using UnityEditor;
#if UNITY_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build.DataBuilders;
#endif

namespace WWIII.SideScroller.Editor.Addressables
{
    public static class AddressablesBuildMenu
    {
        [MenuItem("WWIII/Addressables/Analyze")] 
        public static void Analyze()
        {
#if UNITY_ADDRESSABLES
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) { EditorUtility.DisplayDialog("WWIII","Addressables settings missing.","OK"); return; }
            try
            {
                var asm = typeof(AddressableAssetSettings).Assembly;
                var analyzeType = asm.GetType("UnityEditor.AddressableAssets.Build.AnalyzeSystem.AnalyzeSystem");
                if (analyzeType != null)
                {
                    var analyze = System.Activator.CreateInstance(analyzeType);
                    analyzeType.GetMethod("RefreshAnalysis")?.Invoke(analyze, null);
                    analyzeType.GetMethod("RunAll")?.Invoke(analyze, null);
                    EditorUtility.DisplayDialog("WWIII","Addressables Analyze completed.","OK");
                    return;
                }
            }
            catch { }
            UnityEditor.EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Analyze");
#else
            EditorUtility.DisplayDialog("WWIII","Enable Addressables to use this.","OK");
#endif
        }

        [MenuItem("WWIII/Addressables/Build Player Content")] 
        public static void BuildPlayerContent()
        {
#if UNITY_ADDRESSABLES
            AddressableAssetSettings.BuildPlayerContent();
#else
            EditorUtility.DisplayDialog("WWIII","Enable Addressables to use this.","OK");
#endif
        }

        // Called by CI
        public static void BuildPlayerContentCI()
        {
#if UNITY_ADDRESSABLES
            AddressableAssetSettings.BuildPlayerContent();
#endif
        }
    }
}

