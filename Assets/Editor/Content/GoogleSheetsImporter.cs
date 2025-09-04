#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace WWIII.SideScroller.EditorTools
{
    public static class GoogleSheetsImporter
    {
        private const string DocId = "1sSJDYawNzcLGUg5aW2tR9qvc_n3nGbj_SdQqHtzi67s"; // WWIII_Biographical_Content
        private const string Folder = "Assets/StreamingAssets/Biographical";

        private static string Url(string gid) =>
            $"https://docs.google.com/spreadsheets/d/{DocId}/export?format=csv&gid={gid}";

        [MenuItem("WWIII/Content/Import Sheets/All (Levels, PowerUps, Collectibles, Enemies, Spawns)")]
        public static void ImportCoreTabs()
        {
            EnsureFolder();
            DownloadTo(Url("0"), Path.Combine(Folder, "Levels.csv"));
            DownloadTo(Url("40516398"), Path.Combine(Folder, "PowerUps.csv"));
            DownloadTo(Url("333497557"), Path.Combine(Folder, "Collectibles.csv"));
            DownloadTo(Url("37496131"), Path.Combine(Folder, "Enemies.csv"));
            DownloadTo(Url("219516311"), Path.Combine(Folder, "Spawns.csv"));
            AssetDatabase.Refresh();
        }

        [MenuItem("WWIII/Content/Import Sheets/Character_Appearance (set GID in code)")]
        public static void ImportCharacterAppearance()
        {
            EnsureFolder();
            var gid = EditorUtility.DisplayDialogComplex("Character_Appearance GID",
                "Enter the GID for the Character_Appearance tab in the code (GoogleSheetsImporter.cs) or paste here.",
                "Paste GID", "Cancel", "Use Placeholder");
            var target = Path.Combine(Folder, "Character_Appearance.csv");
            string useGid = "REPLACE_WITH_GID";
            if (gid == 0)
            {
                var input = EditorUtility.DisplayDialogComplex("Paste not supported in dialog", "Please edit the script constant or use placeholder.", "OK", "", "");
            }
            DownloadTo(Url(useGid), target);
            AssetDatabase.Refresh();
        }

        [MenuItem("WWIII/Content/Import Sheets/Narrative_Content (set GID in code)")]
        public static void ImportNarrative()
        {
            EnsureFolder();
            DownloadTo(Url("REPLACE_WITH_GID"), Path.Combine(Folder, "Narrative_Content.csv"));
            AssetDatabase.Refresh();
        }

        [MenuItem("WWIII/Content/Import Sheets/Audio_Integration (set GID in code)")]
        public static void ImportAudio()
        {
            EnsureFolder();
            DownloadTo(Url("REPLACE_WITH_GID"), Path.Combine(Folder, "Audio_Integration.csv"));
            AssetDatabase.Refresh();
        }

        [MenuItem("WWIII/Content/Import Sheets/Photo_References (set GID in code)")]
        public static void ImportPhotos()
        {
            EnsureFolder();
            DownloadTo(Url("REPLACE_WITH_GID"), Path.Combine(Folder, "Photo_References.csv"));
            AssetDatabase.Refresh();
        }

        [MenuItem("WWIII/Content/Import Sheets/Spine_Configuration (set GID in code)")]
        public static void ImportSpineCfg()
        {
            EnsureFolder();
            DownloadTo(Url("REPLACE_WITH_GID"), Path.Combine(Folder, "Spine_Configuration.csv"));
            AssetDatabase.Refresh();
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/Biographical"))
                AssetDatabase.CreateFolder("Assets/StreamingAssets", "Biographical");
        }

        private static void DownloadTo(string url, string dest)
        {
            try
            {
                using (var req = UnityWebRequest.Get(url))
                {
                    var op = req.SendWebRequest();
                    while (!op.isDone) { }
#if UNITY_2020_3_OR_NEWER
                    if (req.result != UnityWebRequest.Result.Success)
#else
                    if (req.isNetworkError || req.isHttpError)
#endif
                    {
                        Debug.LogWarning($"[Sheets] Download failed: {url} => {req.error}");
                        return;
                    }
                    File.WriteAllBytes(dest, req.downloadHandler.data);
                    Debug.Log($"[Sheets] Saved {Path.GetFileName(dest)}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Sheets] Exception: {e.Message}");
            }
        }
    }
}
#endif

