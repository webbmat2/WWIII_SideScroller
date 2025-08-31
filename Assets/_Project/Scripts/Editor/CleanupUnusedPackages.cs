using UnityEngine;
using UnityEditor;
using System.IO;

namespace WWIII.Editor
{
    /// <summary>
    /// Unity AI recommended cleanup utility for unused packages
    /// Addresses: "[Adaptive Performance] No Provider was configured for use"
    /// </summary>
    public static class CleanupUnusedPackages
    {
        [MenuItem("WWIII/🧹 REMOVE ADAPTIVE PERFORMANCE")]
        public static void RemoveAdaptivePerformance()
        {
            Debug.Log("🧹 Removing Adaptive Performance package (not needed for 2D mobile game)...");
            
            // Remove Adaptive Performance settings directory
            string adaptivePerformancePath = "Assets/Adaptive Performance";
            if (Directory.Exists(adaptivePerformancePath))
            {
                AssetDatabase.DeleteAsset(adaptivePerformancePath);
                Debug.Log("🗑️  Removed Adaptive Performance settings directory");
            }
            
            // The package should be removed via Package Manager
            EditorUtility.DisplayDialog("Adaptive Performance Cleanup",
                "Settings directory removed.\\n\\n" +
                "To complete the cleanup:\\n" +
                "1. Open Window > Package Manager\\n" +
                "2. Find 'Adaptive Performance'\\n" +
                "3. Click 'Remove'\\n\\n" +
                "This will eliminate the provider warning.",
                "OK");
        }
        
        [MenuItem("WWIII/🧹 REMOVE XR PACKAGES")]
        public static void RemoveXRPackages()
        {
            Debug.Log("🧹 Removing XR packages (not needed for 2D side-scroller)...");
            
            // Remove XR settings directory
            string xrPath = "Assets/XR";
            if (Directory.Exists(xrPath))
            {
                AssetDatabase.DeleteAsset(xrPath);
                Debug.Log("🗑️  Removed XR settings directory");
            }
            
            EditorUtility.DisplayDialog("XR Cleanup",
                "XR settings directory removed.\\n\\n" +
                "To complete cleanup, remove these packages via Package Manager:\\n" +
                "• XR Management\\n" +
                "• AR Foundation (if present)\\n" +
                "• OpenXR (if present)\\n\\n" +
                "This optimizes the project for 2D gameplay.",
                "OK");
        }
    }
}