using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_ADDRESSABLES || ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace WWIII.SideScroller.Editor.Cleanup
{
    public static class ProjectCleanupTools
    {
        private const string ReportsDir = "Assets/WWIII/Reports";
        private const string TrashRootBase = "Assets/_Trash";

        [MenuItem("WWIII/Cleanup/Analyze Asset Usage (Dry Run)")] 
        public static void AnalyzeAssetUsage()
        {
            Directory.CreateDirectory(ReportsDir);
            var reportPath = Path.Combine(ReportsDir, $"CleanupReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            var roots = CollectRootAssets(out var info);
            var used = CollectDependencies(roots);
            MarkAlwaysUsed(used);
            MarkAddressablesUsed(used);
            MarkProjectSettingsReferencedAssets(used);

            var allAssets = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/") && !p.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var unused = new List<string>();
            foreach (var p in allAssets)
            {
                if (ShouldSkipFromConsideration(p)) continue;
                if (!used.Contains(p)) unused.Add(p);
            }

            using (var sw = new StreamWriter(reportPath))
            {
                sw.WriteLine("WWIII Cleanup Report");
                sw.WriteLine($"Generated: {DateTime.Now}");
                sw.WriteLine();
                sw.WriteLine("Root scenes considered:");
                foreach (var s in info.RootScenes) sw.WriteLine($"  - {s}");
                sw.WriteLine();
                sw.WriteLine($"All assets: {allAssets.Count}");
                sw.WriteLine($"Used assets: {used.Count}");
                sw.WriteLine($"Candidates to remove (unused): {unused.Count}");
                sw.WriteLine();
                sw.WriteLine("Unused assets (candidates):");
                foreach (var p in unused) sw.WriteLine(p);

                // Namespace report
                sw.WriteLine();
                sw.WriteLine("Namespace consistency (non-matching):");
                foreach (var file in FindCSharpFiles())
                {
                    if (!IsNamespaceOk(file)) sw.WriteLine($"  - {file}");
                }

                // Package validation
                sw.WriteLine();
                sw.WriteLine("Package content validation (suspicious):");
                foreach (var p in FindSuspiciousPackageContent()) sw.WriteLine($"  - {p}");
            }

            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(reportPath);
            Debug.Log($"Cleanup analysis written to {reportPath}");
        }

        [MenuItem("WWIII/Cleanup/Move Unused Assets To _Trash (Safe)")]
        public static void MoveUnusedToTrash()
        {
            if (!EditorUtility.DisplayDialog("WWIII", "This will move unused assets (per analyzer heuristic) into Assets/_Trash. Continue?", "Move", "Cancel")) return;

            var roots = CollectRootAssets(out _);
            var used = CollectDependencies(roots);
            MarkAlwaysUsed(used);
            MarkAddressablesUsed(used);
            MarkProjectSettingsReferencedAssets(used);

            var allAssets = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/") && !p.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var trashRoot = EnsureTrashRoot("Unused");

            int moved = 0;
            foreach (var p in allAssets)
            {
                if (ShouldSkipFromConsideration(p)) continue;
                if (used.Contains(p)) continue;
                var dest = trashRoot + "/" + Path.GetFileName(p);
                var error = AssetDatabase.MoveAsset(p, dest);
                if (string.IsNullOrEmpty(error)) moved++;
                else Debug.LogWarning($"Couldn't move {p} → {dest}: {error}");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", $"Moved {moved} assets to {trashRoot}", "OK");
        }

        [MenuItem("WWIII/Cleanup/Prune Build Settings (Keep WWIII Scenes Only)")]
        public static void PruneBuildSettings()
        {
            var wwiiiScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/WWIII/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p)
                .ToArray();
            if (wwiiiScenes.Length == 0)
            {
                EditorUtility.DisplayDialog("WWIII", "No scenes found under Assets/WWIII/Scenes.", "OK");
                return;
            }
            var sceneSettings = wwiiiScenes.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            EditorBuildSettings.scenes = sceneSettings;
            EditorUtility.DisplayDialog("WWIII", $"Build Settings updated with {sceneSettings.Length} scenes from Assets/WWIII/Scenes.", "OK");
        }

        [MenuItem("WWIII/Cleanup/Move Corgi Demo Content to _Trash (Safe)")]
        public static void MoveCorgiDemosToTrash()
        {
            var demosPath = "Assets/ThirdParty/CorgiEngine/Demos";
            if (!AssetDatabase.IsValidFolder(demosPath))
            {
                EditorUtility.DisplayDialog("WWIII", "Corgi demo folder not found.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("WWIII", "Move CorgiEngine demo content to Assets/_Trash? This reduces project bloat. You can restore if needed.", "Move", "Cancel")) return;
            var trashRoot = EnsureTrashRoot("CorgiDemos");
            var dest = trashRoot + "/CorgiEngine_Demos";
            var error = AssetDatabase.MoveAsset(demosPath, dest);
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("WWIII", $"Move failed: {error}", "OK");
                return;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // Neutralize any C# scripts under _Trash to avoid compile collisions
            try { NeutralizeScriptsUnder(dest); } catch { }
            EditorUtility.DisplayDialog("WWIII", $"Moved Corgi demos to {dest}", "OK");
        }

        [MenuItem("WWIII/Cleanup/Report Namespace Consistency")] 
        public static void ReportNamespaces()
        {
            Directory.CreateDirectory(ReportsDir);
            var reportPath = Path.Combine(ReportsDir, $"NamespaceReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            using (var sw = new StreamWriter(reportPath))
            {
                sw.WriteLine("Files with namespace not starting with WWIII.SideScroller:");
                foreach (var file in FindCSharpFiles())
                {
                    if (!IsNamespaceOk(file)) sw.WriteLine(file);
                }

                sw.WriteLine();
                sw.WriteLine("Editor scripts outside Assets/Editor (using UnityEditor):");
                foreach (var file in FindCSharpFiles())
                {
                    if (file.StartsWith("Assets/Editor/")) continue;
                    try
                    {
                        var text = File.ReadAllText(file);
                        if (text.Contains("using UnityEditor") && !text.Contains("#if UNITY_EDITOR"))
                        {
                            sw.WriteLine(file);
                        }
                    }
                    catch { }
                }
            }
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(reportPath);
        }

        [MenuItem("WWIII/Cleanup/Package Content Validation Report")] 
        public static void PackageContentReport()
        {
            Directory.CreateDirectory(ReportsDir);
            var reportPath = Path.Combine(ReportsDir, $"PackageValidation_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            using (var sw = new StreamWriter(reportPath))
            {
                sw.WriteLine("Suspicious package content in Assets (copies or edits):");
                foreach (var p in FindSuspiciousPackageContent()) sw.WriteLine(p);
            }
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(reportPath);
        }

        // Helpers
        private static HashSet<string> CollectDependencies(List<string> roots)
        {
            var deps = new HashSet<string>();
            foreach (var root in roots)
            {
                foreach (var d in AssetDatabase.GetDependencies(root, true)) deps.Add(d);
            }
            return deps;
        }

        private class RootInfo
        {
            public List<string> RootScenes = new List<string>();
        }

        private static List<string> CollectRootAssets(out RootInfo info)
        {
            info = new RootInfo();
            var roots = new List<string>();

            // 1) WWIII scenes folder (authoritative)
            var wwiiiScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/WWIII/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();
            roots.AddRange(wwiiiScenes);
            info.RootScenes.AddRange(wwiiiScenes);

            // 2) Also include enabled scenes from Build Settings if they live under Assets/WWIII
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled && s.path.StartsWith("Assets/WWIII/"))
                {
                    roots.Add(s.path);
                    if (!info.RootScenes.Contains(s.path)) info.RootScenes.Add(s.path);
                }
            }

            return roots.Distinct().ToList();
        }

        private static bool ShouldSkipFromConsideration(string path)
        {
            // Never move or flag these (safe defaults)
            if (path.StartsWith("Assets/WWIII/Reports")) return true;
            if (path.StartsWith("Assets/_Trash")) return true;
            if (path.StartsWith("Assets/Plugins")) return true; // typically third-party plugins
            if (path.StartsWith("Assets/ThirdParty")) return true; // leave third-party alone in this pass
            if (path.StartsWith("Assets/TextMesh Pro")) return true;
            if (path.StartsWith("Assets/WWIII/Ages")) return true; // contains Addressables-driven content

            // Skip folders (we only want to act on concrete assets)
            if (AssetDatabase.IsValidFolder(path)) return true;

            // Keep Editor & Tests content by default (tools and verification are not scene dependencies)
            if (path.StartsWith("Assets/Editor/")) return true;
            if (path.StartsWith("Assets/Tests/")) return true;

            // Keep essential configs and scripts
            if (path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.EndsWith(".asmref", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) return true; // scripts won't show up as scene deps
            if (path.EndsWith(".inputactions", StringComparison.OrdinalIgnoreCase)) return true;

            // Preserve design notes in scenes folder
            if (path.StartsWith("Assets/WWIII/Scenes/") && path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private static void MarkAlwaysUsed(HashSet<string> used)
        {
            // Everything under Resources is considered used (string-based lookups)
            foreach (var guid in AssetDatabase.FindAssets(string.Empty, new[] { "Assets/Resources" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(p)) used.Add(p);
            }
            // WWIII Data definitions are likely referenced via code/strings
            foreach (var guid in AssetDatabase.FindAssets(string.Empty, new[] { "Assets/WWIII/Data" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(p)) used.Add(p);
            }
        }

        private static void MarkAddressablesUsed(HashSet<string> used)
        {
#if UNITY_ADDRESSABLES || ADDRESSABLES
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;
            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                foreach (var e in group.entries)
                {
                    var path = AssetDatabase.GUIDToAssetPath(e.guid);
                    if (!string.IsNullOrEmpty(path)) used.Add(path);
                    foreach (var d in AssetDatabase.GetDependencies(path, true)) used.Add(d);
                }
            }
#endif
        }

        private static void MarkProjectSettingsReferencedAssets(HashSet<string> used)
        {
            // Parse GUID references from GraphicsSettings and EditorBuildSettings and include their assets + dependencies
            var projectSettingsFiles = new[]
            {
                "ProjectSettings/GraphicsSettings.asset",
                "ProjectSettings/EditorBuildSettings.asset",
            };

            foreach (var ps in projectSettingsFiles)
            {
                try
                {
                    if (!File.Exists(ps)) continue;
                    foreach (var guid in ExtractGuidsFromText(File.ReadAllText(ps)))
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        if (string.IsNullOrEmpty(path)) continue;
                        used.Add(path);
                        foreach (var d in AssetDatabase.GetDependencies(path, true)) used.Add(d);
                    }
                }
                catch { }
            }
        }

        private static IEnumerable<string> ExtractGuidsFromText(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;
            // naive scan for lines containing 'guid: <32 hex>'
            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var idx = line.IndexOf("guid:");
                    if (idx < 0) continue;
                    var parts = line.Substring(idx + 5).Trim().Split(new[] {',',' ','\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;
                    var g = parts[0];
                    if (g == "0000000000000000f000000000000000") continue; // unity placeholder
                    if (g.Length == 32) yield return g;
                }
            }
        }

        private static IEnumerable<string> FindCSharpFiles()
        {
            return AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Scripts", "Assets/WWIII" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsNamespaceOk(string csPath)
        {
            try
            {
                var text = File.ReadAllText(csPath);
                // Accept any file that contains our root namespace
                return text.Contains("namespace WWIII.SideScroller");
            }
            catch { return true; }
        }

        private static IEnumerable<string> FindSuspiciousPackageContent()
        {
            // Look for obvious package names under Assets that indicate copied/edited package content
            var suspiciousTokens = new[]
            {
                "Cinemachine", // package content should live under Packages/
                "Unity.2D.",
                "Unity.XR.",
                "Unity.Timeline",
                "Unity.PostProcessing",
            };

            var results = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets(string.Empty, new[] { "Assets" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(p)) continue;
                if (p.StartsWith("Assets/ThirdParty") || p.StartsWith("Assets/Plugins")) continue;
                if (suspiciousTokens.Any(t => Path.GetFileName(p).IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    results.Add(p);
                }
            }
            return results.Distinct().OrderBy(p => p);
        }

        // Utilities that can be reused by automation window
        public static string EnsureTrashRoot(string group = null)
        {
            // Ensure Assets/_Trash exists as an AssetDatabase folder (with GUID)
            if (!AssetDatabase.IsValidFolder("Assets/_Trash"))
            {
                AssetDatabase.CreateFolder("Assets", "_Trash");
            }
            var leaf = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (!string.IsNullOrEmpty(group)) leaf += "_" + Sanitize(group);
            var parent = "Assets/_Trash";
            var target = parent + "/" + leaf;
            if (!AssetDatabase.IsValidFolder(target))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
            return target;
        }

        public static bool MoveAssetTo(string assetPath, string destFolder, out string error)
        {
            // Ensure destination folder exists in AssetDatabase
            if (!destFolder.StartsWith("Assets/"))
            {
                error = "Destination must be under Assets/ for AssetDatabase.MoveAsset";
                return false;
            }
            // Create each segment if needed
            var parts = destFolder.Split('/');
            var path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = path + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(path, parts[i]);
                }
                path = next;
            }
            var dest = destFolder + "/" + Path.GetFileName(assetPath);
            error = AssetDatabase.MoveAsset(assetPath, dest);
            return string.IsNullOrEmpty(error);
        }

        public static void MoveDevDocsToDocsFolder()
        {
            // Move dev docs out of Assets (to project root Docs/) using IO, then Refresh
            var docsFolderAbs = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs"));
            Directory.CreateDirectory(docsFolderAbs);
            string[] candidates =
            {
                "Assets/BEZI_PROJECT_RULES_UNITY_AI.md",
                "Assets/CURSOR_RULES.txt",
                "Assets/UNITY_AI_GITIGNORE.txt",
                "Assets/VSCODE_SETTINGS.json",
            };
            int moved = 0;
            foreach (var p in candidates)
            {
                if (!File.Exists(p)) continue;
                var srcAbs = Path.GetFullPath(p);
                var destAbs = Path.Combine(docsFolderAbs, Path.GetFileName(p));
                try
                {
                    if (File.Exists(destAbs)) File.Delete(destAbs);
                    File.Move(srcAbs, destAbs);
                    var meta = p + ".meta";
                    if (File.Exists(meta)) File.Delete(meta);
                    moved++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Couldn't move {p} → {destAbs}: {ex.Message}");
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", $"Moved {moved} dev docs to {docsFolderAbs}", "OK");
        }

        [MenuItem("WWIII/Cleanup/Neutralize Scripts in _Trash (rename .cs → .cs.txt)")]
        public static void NeutralizeTrashScriptsMenu()
        {
            int renamed = NeutralizeScriptsUnder("Assets/_Trash");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", $"Renamed {renamed} script(s) to .cs.txt under Assets/_Trash to prevent compilation.", "OK");
        }

        // Alias with the exact menu label requested
        [MenuItem("WWIII/Cleanup/Neutralize Scripts in _Trash")]
        public static void NeutralizeTrashScriptsMenuAlias()
        {
            NeutralizeTrashScriptsMenu();
        }

        public static int NeutralizeScriptsUnder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return 0;
            int renamed = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                // Prefer MonoScript filter to target C# scripts directly
                var guids = AssetDatabase.FindAssets("t:MonoScript", new[] { folder });
                foreach (var g in guids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    if (!p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;
                    var newPath = p + ".txt";
                    var err = AssetDatabase.MoveAsset(p, newPath);
                    if (!string.IsNullOrEmpty(err))
                    {
                        Debug.LogWarning($"Couldn't rename {p}: {err}");
                    }
                    else
                    {
                        renamed++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            Debug.Log($"NeutralizeScriptsUnder: renamed {renamed} script(s) under {folder}");
            return renamed;
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "group";
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }
    }
}
