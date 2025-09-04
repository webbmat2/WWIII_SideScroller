using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Cleanup
{
    public class CleanupAutomationWindow : EditorWindow
    {
        public class Reports
        {
            public string CleanupPath;
            public string NamespacePath;
            public string PackagePath;
            public List<string> UnusedAssets = new List<string>();
            public List<string> BadNamespaces = new List<string>();
            public List<string> EditorOutside = new List<string>();
            public List<string> SuspiciousPackages = new List<string>();
        }

        private Reports _reports = new Reports();
        private Vector2 _scroll;

        // Suggested actions toggles
        private bool _actMoveUnused = true;
        private bool _actPruneBuildSettings = true;
        private bool _actMoveCorgiDemos = false;
        private bool _actMoveDevDocs = false;
        private bool _actTrashObsoleteCorgiAdapter = false;

        [MenuItem("WWIII/Cleanup/Automation Dashboard")]
        public static void Open()
        {
            var win = GetWindow<CleanupAutomationWindow>(true, "Cleanup Automation", true);
            win.minSize = new Vector2(700, 420);
            win.Scan();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Rescan Latest Reports"))
            {
                Scan();
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawReportsSummary();
            EditorGUILayout.Space(8);
            DrawActions();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Execute Selected Actions", GUILayout.Height(28), GUILayout.MinWidth(240)))
                {
                    ExecuteSelected();
                }
            }
        }

        private void DrawReportsSummary()
        {
            EditorGUILayout.LabelField("Latest Reports", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("CleanupReport:", string.IsNullOrEmpty(_reports.CleanupPath) ? "(not found)" : _reports.CleanupPath);
            EditorGUILayout.LabelField("NamespaceReport:", string.IsNullOrEmpty(_reports.NamespacePath) ? "(not found)" : _reports.NamespacePath);
            EditorGUILayout.LabelField("PackageValidation:", string.IsNullOrEmpty(_reports.PackagePath) ? "(not found)" : _reports.PackagePath);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Unused candidates (from report): {_reports.UnusedAssets.Count}");
            EditorGUILayout.LabelField($"Namespace issues: {_reports.BadNamespaces.Count}");
            EditorGUILayout.LabelField($"EditorOutside issues: {_reports.EditorOutside.Count}");
            EditorGUILayout.LabelField($"Suspicious package items: {_reports.SuspiciousPackages.Count}");

            if (_reports.UnusedAssets.Count > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Sample unused (first 10):", EditorStyles.miniBoldLabel);
                foreach (var p in _reports.UnusedAssets.Take(10)) EditorGUILayout.LabelField("- " + p);
            }
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Suggested Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("General Cleanup", EditorStyles.miniBoldLabel);
                using (new EditorGUI.DisabledScope(_reports.UnusedAssets.Count == 0))
                {
                    _actMoveUnused = EditorGUILayout.ToggleLeft("Move unused assets to Assets/_Trash (recomputed, safe)", _actMoveUnused);
                }
                _actPruneBuildSettings = EditorGUILayout.ToggleLeft("Prune Build Settings to Assets/WWIII/Scenes", _actPruneBuildSettings);
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Third-Party Demos", EditorStyles.miniBoldLabel);
                var demosExists = AssetDatabase.IsValidFolder("Assets/ThirdParty/CorgiEngine/Demos");
                using (new EditorGUI.DisabledScope(!demosExists))
                {
                    _actMoveCorgiDemos = EditorGUILayout.ToggleLeft("Move CorgiEngine demos to Assets/_Trash", _actMoveCorgiDemos && demosExists);
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Housekeeping", EditorStyles.miniBoldLabel);
                var devDocs = new[]
                {
                    "Assets/BEZI_PROJECT_RULES_UNITY_AI.md",
                    "Assets/CURSOR_RULES.txt",
                    "Assets/UNITY_AI_GITIGNORE.txt",
                    "Assets/VSCODE_SETTINGS.json",
                };
                var anyDocs = devDocs.Any(File.Exists);
                using (new EditorGUI.DisabledScope(!anyDocs))
                {
                    _actMoveDevDocs = EditorGUILayout.ToggleLeft("Move dev docs from Assets/ → Docs/", _actMoveDevDocs && anyDocs);
                }

                var obsoleteAdapter = "Assets/Scripts/Integration/Corgi/AgeCorgiAbilityAdapter.cs";
                var adapterExists = File.Exists(obsoleteAdapter);
                using (new EditorGUI.DisabledScope(!adapterExists))
                {
                    _actTrashObsoleteCorgiAdapter = EditorGUILayout.ToggleLeft("Move obsolete AgeCorgiAbilityAdapter.cs to _Trash", _actTrashObsoleteCorgiAdapter && adapterExists);
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Investigate (manual)", EditorStyles.miniBoldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reveal Reports Folder"))
                    {
                        EditorUtility.RevealInFinder(Path.Combine(Application.dataPath, "WWIII/Reports"));
                    }
                    if (!string.IsNullOrEmpty(_reports.NamespacePath) && GUILayout.Button("Open Namespace Report"))
                    {
                        EditorUtility.OpenWithDefaultApp(_reports.NamespacePath);
                    }
                    if (!string.IsNullOrEmpty(_reports.PackagePath) && GUILayout.Button("Open Package Report"))
                    {
                        EditorUtility.OpenWithDefaultApp(_reports.PackagePath);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(_reports.BadNamespaces.Count == 0))
                    {
                        if (GUILayout.Button($"Select Namespace Issues ({_reports.BadNamespaces.Count})"))
                        {
                            SelectAssets(_reports.BadNamespaces);
                        }
                    }
                    using (new EditorGUI.DisabledScope(_reports.EditorOutside.Count == 0))
                    {
                        if (GUILayout.Button($"Select EditorOutside Issues ({_reports.EditorOutside.Count})"))
                        {
                            SelectAssets(_reports.EditorOutside);
                        }
                    }
                }
            }
        }

        private void ExecuteSelected()
        {
            try
            {
                EditorUtility.DisplayProgressBar("WWIII Cleanup", "Applying selected actions...", 0f);
                int steps = 1;
                steps += _actMoveUnused ? 1 : 0;
                steps += _actPruneBuildSettings ? 1 : 0;
                steps += _actMoveCorgiDemos ? 1 : 0;
                steps += _actMoveDevDocs ? 1 : 0;
                steps += _actTrashObsoleteCorgiAdapter ? 1 : 0;
                int done = 0;

                float Progress() => (float)done / Mathf.Max(1, steps);

                if (_actMoveUnused)
                {
                    EditorUtility.DisplayProgressBar("WWIII Cleanup", "Moving unused assets (safe recompute)", Progress());
                    ProjectCleanupTools.MoveUnusedToTrash();
                    done++;
                }

                if (_actPruneBuildSettings)
                {
                    EditorUtility.DisplayProgressBar("WWIII Cleanup", "Pruning Build Settings", Progress());
                    ProjectCleanupTools.PruneBuildSettings();
                    done++;
                }

                if (_actMoveCorgiDemos)
                {
                    EditorUtility.DisplayProgressBar("WWIII Cleanup", "Moving Corgi demos", Progress());
                    ProjectCleanupTools.MoveCorgiDemosToTrash();
                    done++;
                }

                if (_actMoveDevDocs)
                {
                    EditorUtility.DisplayProgressBar("WWIII Cleanup", "Moving dev docs to Docs/", Progress());
                    ProjectCleanupTools.MoveDevDocsToDocsFolder();
                    done++;
                }

                if (_actTrashObsoleteCorgiAdapter)
                {
                    EditorUtility.DisplayProgressBar("WWIII Cleanup", "Archiving obsolete adapter", Progress());
                    var trash = ProjectCleanupTools.EnsureTrashRoot("CorgiObsolete");
                    string err;
                    ProjectCleanupTools.MoveAssetTo("Assets/Scripts/Integration/Corgi/AgeCorgiAbilityAdapter.cs", trash, out err);
                    if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"Adapter move failed: {err}");
                    done++;
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("WWIII Cleanup", "Selected actions completed.", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Scan(); // refresh counts
            }
        }

        private void Scan()
        {
            _reports = new Reports();
            var dir = Path.Combine(Application.dataPath, "WWIII/Reports");
            if (!Directory.Exists(dir)) return;
            string FindLatest(string pattern)
            {
                var files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                return files.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
            }
            _reports.CleanupPath = FindLatest("CleanupReport_*.txt");
            _reports.NamespacePath = FindLatest("NamespaceReport_*.txt");
            _reports.PackagePath = FindLatest("PackageValidation_*.txt");

            if (!string.IsNullOrEmpty(_reports.CleanupPath)) ParseCleanupReport(_reports.CleanupPath, _reports);
            if (!string.IsNullOrEmpty(_reports.NamespacePath)) ParseNamespaceReport(_reports.NamespacePath, _reports);
            if (!string.IsNullOrEmpty(_reports.PackagePath)) ParsePackageReport(_reports.PackagePath, _reports);

            // Default toggles
            _actMoveUnused = _reports.UnusedAssets.Count > 0;
            _actPruneBuildSettings = true;
            _actMoveCorgiDemos = AssetDatabase.IsValidFolder("Assets/ThirdParty/CorgiEngine/Demos");

            var devDocs = new[]
            {
                "Assets/BEZI_PROJECT_RULES_UNITY_AI.md",
                "Assets/CURSOR_RULES.txt",
                "Assets/UNITY_AI_GITIGNORE.txt",
                "Assets/VSCODE_SETTINGS.json",
            };
            _actMoveDevDocs = devDocs.Any(File.Exists);
            _actTrashObsoleteCorgiAdapter = File.Exists("Assets/Scripts/Integration/Corgi/AgeCorgiAbilityAdapter.cs");
        }

        private static void SelectAssets(List<string> paths)
        {
            var objs = new List<UnityEngine.Object>();
            foreach (var p in paths)
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
                if (obj != null) objs.Add(obj);
            }
            if (objs.Count > 0)
            {
                Selection.objects = objs.ToArray();
                EditorGUIUtility.PingObject(objs[0]);
            }
        }

        public static void ParseCleanupReport(string path, Reports r)
        {
            var lines = File.ReadAllLines(path);
            bool inUnused = false;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.StartsWith("Unused assets (candidates):")) { inUnused = true; continue; }
                if (inUnused)
                {
                    if (string.IsNullOrEmpty(line) || (!line.StartsWith("Assets/") && !line.StartsWith("- "))) { inUnused = false; continue; }
                    var p = line.StartsWith("- ") ? line.Substring(2).Trim() : line;
                    if (p.StartsWith("Assets/")) r.UnusedAssets.Add(p);
                }
            }
        }

        public static void ParseNamespaceReport(string path, Reports r)
        {
            var lines = File.ReadAllLines(path);
            bool inBadNs = false, inEditorOutside = false;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.StartsWith("Files with namespace not starting")) { inBadNs = true; inEditorOutside = false; continue; }
                if (line.StartsWith("Editor scripts outside")) { inBadNs = false; inEditorOutside = true; continue; }
                if (string.IsNullOrWhiteSpace(line)) { inBadNs = inEditorOutside = false; continue; }
                if (line.StartsWith("Assets/"))
                {
                    if (inBadNs) r.BadNamespaces.Add(line);
                    else if (inEditorOutside) r.EditorOutside.Add(line);
                }
            }
        }

        public static void ParsePackageReport(string path, Reports r)
        {
            var lines = File.ReadAllLines(path);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.StartsWith("Assets/")) r.SuspiciousPackages.Add(line);
            }
        }
    }

    public static class CleanupAutomationMenu
    {
        [MenuItem("WWIII/Cleanup/Auto Analyze & Execute (One-Click)")]
        public static void AutoAnalyzeAndExecute()
        {
            var dir = Path.Combine(Application.dataPath, "WWIII/Reports");
            string FindLatest(string pattern)
            {
                if (!Directory.Exists(dir)) return null;
                var files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                return files.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
            }

            var reports = new CleanupAutomationWindow.Reports();
            var cleanup = FindLatest("CleanupReport_*.txt");
            var ns = FindLatest("NamespaceReport_*.txt");
            var pkg = FindLatest("PackageValidation_*.txt");
            if (!string.IsNullOrEmpty(cleanup)) CleanupAutomationWindow.ParseCleanupReport(cleanup, reports);
            if (!string.IsNullOrEmpty(ns)) CleanupAutomationWindow.ParseNamespaceReport(ns, reports);
            if (!string.IsNullOrEmpty(pkg)) CleanupAutomationWindow.ParsePackageReport(pkg, reports);

            bool moveUnused = reports.UnusedAssets.Count > 0;
            bool pruneBuild = true;
            bool moveDemos = AssetDatabase.IsValidFolder("Assets/ThirdParty/CorgiEngine/Demos");
            bool moveDocs = new[]
            {
                "Assets/BEZI_PROJECT_RULES_UNITY_AI.md",
                "Assets/CURSOR_RULES.txt",
                "Assets/UNITY_AI_GITIGNORE.txt",
                "Assets/VSCODE_SETTINGS.json",
            }.Any(File.Exists);
            bool trashAdapter = File.Exists("Assets/Scripts/Integration/Corgi/AgeCorgiAbilityAdapter.cs");

            var summary = "Planned actions:\n" +
                          (moveUnused ? $"- Move unused assets to _Trash (report: {reports.UnusedAssets.Count})\n" : "") +
                          (pruneBuild ? "- Prune Build Settings to Assets/WWIII/Scenes\n" : "") +
                          (moveDemos ? "- Move CorgiEngine demos to _Trash\n" : "") +
                          (moveDocs ? "- Move dev docs from Assets/ to Docs\n" : "") +
                          (trashAdapter ? "- Move obsolete AgeCorgiAbilityAdapter.cs to _Trash\n" : "");

            if (!EditorUtility.DisplayDialog("WWIII One-Click Cleanup", summary, "Execute", "Cancel")) return;

            try
            {
                EditorUtility.DisplayProgressBar("WWIII Cleanup", "Running one-click cleanup...", 0f);
                int steps = (moveUnused?1:0)+(pruneBuild?1:0)+(moveDemos?1:0)+(moveDocs?1:0)+(trashAdapter?1:0);
                int done = 0; float Progress() => (float)done / Mathf.Max(1, steps);

                if (moveUnused) { EditorUtility.DisplayProgressBar("WWIII Cleanup", "Moving unused assets", Progress()); ProjectCleanupTools.MoveUnusedToTrash(); done++; }
                if (pruneBuild) { EditorUtility.DisplayProgressBar("WWIII Cleanup", "Pruning build settings", Progress()); ProjectCleanupTools.PruneBuildSettings(); done++; }
                if (moveDemos) { EditorUtility.DisplayProgressBar("WWIII Cleanup", "Moving Corgi demos", Progress()); ProjectCleanupTools.MoveCorgiDemosToTrash(); done++; }
                if (moveDocs) { EditorUtility.DisplayProgressBar("WWIII Cleanup", "Moving dev docs", Progress()); ProjectCleanupTools.MoveDevDocsToDocsFolder(); done++; }
                if (trashAdapter)
                {
                    EditorUtility.DisplayProgressBar("WWIII Cleanup", "Archiving obsolete adapter", Progress());
                    var trash = ProjectCleanupTools.EnsureTrashRoot("CorgiObsolete");
                    string err; ProjectCleanupTools.MoveAssetTo("Assets/Scripts/Integration/Corgi/AgeCorgiAbilityAdapter.cs", trash, out err);
                    if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"Adapter move failed: {err}");
                    done++;
                }

                EditorUtility.DisplayDialog("WWIII One-Click Cleanup", "Cleanup complete.", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
    }
}
