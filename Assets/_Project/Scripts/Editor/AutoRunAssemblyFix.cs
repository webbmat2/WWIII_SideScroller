using UnityEngine;
using UnityEditor;
using System.IO;

namespace WWIII.Editor
{
    /// <summary>
    /// Automatically runs assembly fixes on script compilation
    /// </summary>
    [InitializeOnLoad]
    public class AutoRunAssemblyFix
    {
        private static bool hasRunFix = false;

        static AutoRunAssemblyFix()
        {
            if (!hasRunFix)
            {
                EditorApplication.delayCall += RunFix;
            }
        }

        private static void RunFix()
        {
            if (hasRunFix) return;

            Debug.Log("🔧 Auto-running assembly definition fixes...");
            
            // Fix Editor assembly to reference Data and Integrations
            FixEditorAssembly();
            FixIntegrationsAssembly();
            FixDataAssembly();
            
            hasRunFix = true;
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Assembly definitions auto-fixed!");
        }

        private static void FixEditorAssembly()
        {
            string editorAsmdefPath = "Assets/_Project/Scripts/Editor/Editor.asmdef";
            string editorContent = @"{
    ""name"": ""WWIII.Editor"",
    ""rootNamespace"": ""WWIII.Editor"",
    ""references"": [
        ""WWIII.Data"",
        ""WWIII.Integrations"",
        ""WWIII.Core"",
        ""Unity.TextMeshPro""
    ],
    ""includePlatforms"": [
        ""Editor""
    ],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            try
            {
                File.WriteAllText(editorAsmdefPath, editorContent);
                Debug.Log("✅ Fixed Editor assembly references");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to fix Editor assembly: {e.Message}");
            }
        }

        private static void FixIntegrationsAssembly()
        {
            string integrationsAsmdefPath = "Assets/_Project/Scripts/Integrations/Integrations.asmdef";
            string integrationsContent = @"{
    ""name"": ""WWIII.Integrations"",
    ""rootNamespace"": ""WWIII.Integrations"",
    ""references"": [
        ""WWIII.Data"",
        ""WWIII.Core"",
        ""Unity.TextMeshPro""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            try
            {
                File.WriteAllText(integrationsAsmdefPath, integrationsContent);
                Debug.Log("✅ Fixed Integrations assembly references");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to fix Integrations assembly: {e.Message}");
            }
        }

        private static void FixDataAssembly()
        {
            string dataAsmdefPath = "Assets/_Project/Scripts/Data/Data.asmdef";
            string dataContent = @"{
    ""name"": ""WWIII.Data"",
    ""rootNamespace"": ""WWIII.Data"",
    ""references"": [
        ""Unity.TextMeshPro""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            try
            {
                File.WriteAllText(dataAsmdefPath, dataContent);
                Debug.Log("✅ Fixed Data assembly references");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to fix Data assembly: {e.Message}");
            }
        }
    }
}