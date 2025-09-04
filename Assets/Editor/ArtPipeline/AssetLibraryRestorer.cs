using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class AssetLibraryRestorer
    {
        private const string ReportsDir = "Assets/WWIII/Reports";
        private const string BayatRoot = "Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate";

        [MenuItem("WWIII/Art/Restore/Verify Visual Asset Library")] 
        public static void VerifyLibrary()
        {
            Directory.CreateDirectory(ReportsDir);
            var reportPath = Path.Combine(ReportsDir, $"AssetVerificationReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            int ok = 0, warn = 0, err = 0;
            using (var sw = new StreamWriter(reportPath))
            {
                sw.WriteLine("Visual Asset Library Verification");
                sw.WriteLine($"Generated: {DateTime.Now}");
                sw.WriteLine();

                // Bayat pack checks
                CheckFolder(sw, BayatRoot + "/Textures", ref ok, ref warn, ref err);
                CheckFolder(sw, BayatRoot + "/Prefabs", ref ok, ref warn, ref err);
                CheckFolder(sw, BayatRoot + "/Animations", ref ok, ref warn, ref err);

                int spriteCount = CountAssets("t:Sprite", new[] { BayatRoot + "/Textures" });
                int prefabCount = CountAssets("t:Prefab", new[] { BayatRoot + "/Prefabs" });
                int animCount = CountAssets("t:AnimationClip", new[] { BayatRoot + "/Animations" });
                sw.WriteLine($"Bayat sprites: {spriteCount}, prefabs: {prefabCount}, animations: {animCount}");
                if (spriteCount == 0 || prefabCount == 0) { sw.WriteLine("[ERR] Bayat assets missing or not imported"); err++; }
                else ok++;

                // Corgi Engine core check (common folders)
                var corgiRoot = "Assets/ThirdParty/CorgiEngine";
                CheckFolder(sw, corgiRoot + "/Common", ref ok, ref warn, ref err);
                CheckFolder(sw, corgiRoot + "/ThirdParty", ref ok, ref warn, ref err);

                // Demo presence (may be in _Trash but offer restore)
                bool demosLive = AssetDatabase.IsValidFolder(corgiRoot + "/Demos");
                sw.WriteLine(demosLive ? "[OK] Corgi demos present" : "[WARN] Corgi demos not present (can restore from _Trash as reference)");
                if (demosLive) ok++; else warn++;

                // Enemy/Character/Environment prefabs availability
                int enemyDefs = CountAssets("t:WWIII.SideScroller.Design.EnemyDefinition", new[] { "Assets/WWIII/Data/Definitions/Enemies" });
                int envProps = CountAssets("t:Prefab", new[] { BayatRoot + "/Prefabs/Items", BayatRoot + "/Prefabs/Particle" });
                sw.WriteLine($"EnemyDefinition assets: {enemyDefs}, Env props/particles: {envProps}");
                if (enemyDefs == 0) { sw.WriteLine("[WARN] No EnemyDefinitions found"); warn++; }
                if (envProps == 0) { sw.WriteLine("[WARN] No Bayat props/particles found"); warn++; }

                // UI elements
                int uiPrefabs = CountAssets("t:Prefab", new[] { BayatRoot + "/Prefabs/Ui" });
                sw.WriteLine($"UI prefabs: {uiPrefabs}");
                ok++;
            }

            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(reportPath);
            EditorUtility.DisplayDialog("WWIII", "Asset verification report generated.", "OK");
        }

        [MenuItem("WWIII/Art/Restore/Restore Corgi Demos As Reference (copy)")]
        public static void RestoreCorgiDemosAsReference()
        {
            // Find the latest CorgiEngine_Demos folder inside Assets/_Trash
            var trash = "Assets/_Trash";
            if (!AssetDatabase.IsValidFolder(trash)) { EditorUtility.DisplayDialog("WWIII", "No _Trash folder found.", "OK"); return; }
            string source = FindFolderRecursive(trash, "CorgiEngine_Demos");
            if (string.IsNullOrEmpty(source)) { EditorUtility.DisplayDialog("WWIII", "No CorgiEngine_Demos folder found in _Trash.", "OK"); return; }
            var destRoot = "Assets/WWIII/Reference/CorgiDemos";
            EnsureFolder(destRoot);

            // Copy all assets under source to destRoot preserving structure
            var guids = AssetDatabase.FindAssets(string.Empty, new[] { source });
            int copied = 0, failed = 0;
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (AssetDatabase.IsValidFolder(p)) continue; // skip folders here
                if (p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue; // skip scripts to avoid compile collisions
                var rel = p.Substring(source.Length).TrimStart('/');
                var dest = Path.Combine(destRoot, rel).Replace('\\','/');
                var destDir = Path.GetDirectoryName(dest).Replace('\\','/');
                EnsureFolder(destDir);
                if (AssetDatabase.CopyAsset(p, dest)) copied++; else failed++;
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", $"Copied Corgi demos to {destRoot}. Copied: {copied}, Failed: {failed}", "OK");
        }

        [MenuItem("WWIII/Art/Restore/Restore Corgi Demos To Original (move)")]
        public static void RestoreCorgiDemosToOriginal()
        {
            var trash = "Assets/_Trash";
            if (!AssetDatabase.IsValidFolder(trash)) { EditorUtility.DisplayDialog("WWIII", "No _Trash folder found.", "OK"); return; }
            string source = FindFolderRecursive(trash, "CorgiEngine_Demos");
            if (string.IsNullOrEmpty(source)) { EditorUtility.DisplayDialog("WWIII", "No CorgiEngine_Demos folder found in _Trash.", "OK"); return; }
            var original = "Assets/ThirdParty/CorgiEngine/Demos";
            EnsureFolder("Assets/ThirdParty/CorgiEngine");
            // Move the entire folder
            var error = AssetDatabase.MoveAsset(source, original);
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("WWIII", $"Move failed: {error}\nTry the 'As Reference (copy)' option instead.", "OK");
                return;
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", "Corgi demos restored to original location.", "OK");
        }

        [MenuItem("WWIII/Art/Restore/Create Tile Palette From Tiles (Age 7)")]
        public static void CreateAge7TilePalette()
        {
            // Create a prefab palette using current Age7 tiles under Assets/WWIII/Tiles/Age7/Ground
            var tilesFolder = "Assets/WWIII/Tiles/Age7/Ground";
            var guids = AssetDatabase.FindAssets("t:Tile", new[] { tilesFolder });
            if (guids.Length == 0) { EditorUtility.DisplayDialog("WWIII", "No tiles found for Age7 ground. Run 'Assign Bayat Visuals for Age 7' first.", "OK"); return; }

            var tiles = guids.Select(AssetDatabase.GUIDToAssetPath).Select(p => AssetDatabase.LoadAssetAtPath<TileBase>(p)).Where(t => t != null).ToList();
            var paletteFolder = "Assets/WWIII/TilePalettes";
            EnsureFolder(paletteFolder);
            var palettePath = Path.Combine(paletteFolder, "Age7_Palette.prefab").Replace('\\','/');

            var go = new GameObject("Age7_Palette");
            go.AddComponent<Grid>();
            var tmGo = new GameObject("PaletteTilemap");
            tmGo.transform.SetParent(go.transform);
            var tm = tmGo.AddComponent<Tilemap>();
            tmGo.AddComponent<TilemapRenderer>();

            // Lay tiles in a grid
            int cols = Mathf.CeilToInt(Mathf.Sqrt(tiles.Count));
            for (int i = 0; i < tiles.Count; i++)
            {
                int x = i % cols;
                int y = i / cols;
                tm.SetTile(new Vector3Int(x, -y, 0), tiles[i]);
            }

            PrefabUtility.SaveAsPrefabAsset(go, palettePath);
            UnityEngine.Object.DestroyImmediate(go);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", $"Tile Palette prefab created at {palettePath}. Open the Tile Palette window and select it as a palette.", "OK");
        }

        [MenuItem("WWIII/Art/Restore/Restore Everything (Verify → Copy Demos → Create Palette)")]
        public static void RestoreEverything()
        {
            if (EditorUtility.DisplayDialog("WWIII", "Run Verify, copy Corgi demos as reference, and create Age7 palette?", "Run", "Cancel"))
            {
                VerifyLibrary();
                RestoreCorgiDemosAsReference();
                CreateAge7TilePalette();
                // Also neutralize any scripts lingering under _Trash
                WWIII.SideScroller.Editor.Cleanup.ProjectCleanupTools.NeutralizeScriptsUnder("Assets/_Trash");
                AssetDatabase.Refresh();
            }
        }

        // Helpers
        private static void CheckFolder(StreamWriter sw, string path, ref int ok, ref int warn, ref int err)
        {
            if (AssetDatabase.IsValidFolder(path)) { sw.WriteLine($"[OK] {path}"); ok++; }
            else { sw.WriteLine($"[ERR] Missing folder: {path}"); err++; }
        }

        private static int CountAssets(string filter, string[] folders)
        {
            try { return AssetDatabase.FindAssets(filter, folders).Length; }
            catch { return 0; }
        }

        private static string FindFolderRecursive(string root, string folderName)
        {
            var stack = new Stack<string>();
            stack.Push(root);
            string best = null;
            DateTime bestTime = DateTime.MinValue;
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                foreach (var guid in AssetDatabase.FindAssets("", new[] { cur }))
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    if (!AssetDatabase.IsValidFolder(p)) continue;
                    if (string.Equals(Path.GetFileName(p), folderName, StringComparison.OrdinalIgnoreCase))
                    {
                        var t = File.GetLastWriteTimeUtc(p);
                        if (t > bestTime) { bestTime = t; best = p; }
                    }
                    stack.Push(p);
                }
            }
            return best;
        }

        private static void EnsureFolder(string folder)
        {
            if (!folder.StartsWith("Assets/")) return;
            var parts = folder.Split('/');
            var path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = path + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(path, parts[i]);
                path = next;
            }
        }
    }
}
