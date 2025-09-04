using System.IO;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Audio
{
    public static class FMODMaintenance
    {
        [MenuItem("WWIII/Audio/Tools/Purge FMOD Editor Cache (safe)")]
        public static void PurgeEditorCache()
        {
            var cachePath = "Assets/Plugins/FMOD/Cache";
            if (AssetDatabase.IsValidFolder(cachePath))
            {
                if (AssetDatabase.DeleteAsset(cachePath))
                {
                    Debug.Log($"Purged FMOD editor cache folder: {cachePath}");
                }
                else
                {
                    Debug.LogWarning($"Couldn't delete {cachePath}. Close any file explorer / apps locking files and try again.");
                }
            }
            else
            {
                Debug.Log($"FMOD editor cache folder already missing: {cachePath}");
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("WWIII/Audio/Tools/Delete FMOD Bank Wrappers (Assets/FMOD*Banks)")]
        public static void DeleteWrapperAssets()
        {
            // FMOD generates bank wrapper assets under a configurable folder; by default it's Assets/FMODBanks
            string[] candidates =
            {
                "Assets/FMODBanks",
                "Assets/FMOD_Banks",
                "Assets/FMOBanks",
                "Assets/FMODOBanks"
            };
            int removed = 0;
            foreach (var path in candidates)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    if (AssetDatabase.DeleteAsset(path))
                    {
                        removed++;
                        Debug.Log($"Deleted FMOD bank wrapper folder: {path}");
                    }
                }
            }
            if (removed == 0)
            {
                Debug.Log("No FMOD bank wrapper folders found under Assets/.");
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("WWIII/Audio/Tools/Reimport StreamingAssets/FMOD (force)")]
        public static void ReimportStreamingAssets()
        {
            var root = "Assets/StreamingAssets/FMOD";
            if (!AssetDatabase.IsValidFolder(root))
            {
                Debug.LogWarning($"Folder not found: {root}. Set FMOD Studio build path to Assets/StreamingAssets/FMOD and Build All.");
                return;
            }
            AssetDatabase.ImportAsset(root, ImportAssetOptions.ForceUpdate);
            Debug.Log("Forced reimport of StreamingAssets/FMOD");
        }
    }
}

