using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Audio
{
    public static class FMODValidator
    {
        [MenuItem("WWIII/Audio/Validate FMOD Banks and Config")] 
        public static void ValidateBanks()
        {
            var root = Path.Combine(Application.dataPath, "StreamingAssets/FMOD");
            if (!Directory.Exists(root))
            {
                Debug.LogWarning($"FMODValidator: Folder not found: {root}\nSet FMOD Studio build directory to Assets/StreamingAssets/FMOD and build banks.");
                return;
            }

            var platformDirs = Directory.GetDirectories(root);
            if (platformDirs.Length == 0)
            {
                Debug.LogWarning($"FMODValidator: No platform subfolders under {root}. Expected e.g. 'Desktop', 'iOS'.");
            }

            foreach (var dir in platformDirs)
            {
                var platform = Path.GetFileName(dir);
                var banks = Directory.GetFiles(dir, "*.bank").Select(Path.GetFileName).ToArray();
                bool hasMaster = banks.Any(f => f == "Master.bank");
                bool hasStrings = File.Exists(Path.Combine(dir, "Master.strings.bank"));
                Debug.Log($"FMODValidator: {platform} banks: [ {string.Join(", ", banks)} ]\nMaster.strings.bank present: {hasStrings}");
                if (!hasMaster || !hasStrings)
                {
                    Debug.LogWarning($"FMODValidator: {platform} missing Master or Master.strings bank.");
                }
            }

            // Show quick reminder about names vs paths
            Debug.Log("FMODValidator: In BiographicalAudioConfig, Bank fields are names (e.g., 'Master', 'Music', 'Memory'), not paths. Runtime resolves StreamingAssets/FMOD/<Platform>/ automatically.");
        }
    }
}

