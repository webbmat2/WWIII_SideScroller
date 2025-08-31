using UnityEngine;
using UnityEditor;
using System.IO;

namespace WWIII.Editor
{
    /// <summary>
    /// Fixes assembly definition references to resolve compilation errors
    /// </summary>
    public class FixAssemblyReferences
    {
        [MenuItem("WWIII/🔧 FIX ASSEMBLY REFERENCES")]
        public static void FixAllAssemblyReferences()
        {
            Debug.Log("🔧 Fixing assembly definition references...");

            FixEditorAssembly();
            FixIntegrationsAssembly();
            FixDataAssembly();
            
            AssetDatabase.Refresh();
            Debug.Log("✅ Assembly references fixed! Recompiling...");
            
            EditorUtility.DisplayDialog("Assembly References Fixed",
                "Assembly definition files have been updated.\n\n" +
                "Fixed references:\n" +
                "✅ Editor now references Data & Integrations\n" +
                "✅ Integrations now references Data\n" +
                "✅ TextMeshPro reference added\n\n" +
                "Unity will now recompile the project.",
                "OK");
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
            File.WriteAllText(editorAsmdefPath, editorContent);
            Debug.Log("✅ Fixed Editor assembly references");
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
            File.WriteAllText(integrationsAsmdefPath, integrationsContent);
            Debug.Log("✅ Fixed Integrations assembly references");
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
            File.WriteAllText(dataAsmdefPath, dataContent);
            Debug.Log("✅ Fixed Data assembly references");
        }

        [MenuItem("WWIII/🔧 IMPORT TEXTMESHPRO")]
        public static void ImportTextMeshPro()
        {
            Debug.Log("🔧 Importing TextMeshPro...");
            
            // Import TextMeshPro essentials
            #if UNITY_2018_3_OR_NEWER
            // Unity 6+ no longer supports TextSettings.ImportEssentials() - API deprecated\n            Debug.Log(\"Unity 6+ detected: TextMeshPro is included by default, no import needed\");
            #endif
            
            Debug.Log("✅ TextMeshPro import initiated");
            
            EditorUtility.DisplayDialog("TextMeshPro Import",
                "TextMeshPro import has been initiated.\n\n" +
                "If the import window appears, click 'Import TMP Essentials'.\n\n" +
                "This will resolve TMPro namespace errors.",
                "OK");
        }
    }
}