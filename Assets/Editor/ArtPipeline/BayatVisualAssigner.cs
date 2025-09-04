using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class BayatVisualAssigner
    {
        private static readonly string BayatRoot = "Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate";

        [MenuItem("WWIII/Art/Assign Bayat Visuals for Age 7")] 
        public static void AssignAge7Visuals()
        {
            var theme = AssetDatabase.LoadAssetAtPath<LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
            if (theme == null) { EditorUtility.DisplayDialog("WWIII","LevelThemeProfile not found at Assets/WWIII/Data/LevelThemeProfile.asset","OK"); return; }
            var ageTheme = theme.GetForYears(7);
            if (ageTheme == null) { EditorUtility.DisplayDialog("WWIII","Age 7 theme not found in LevelThemeProfile","OK"); return; }

            // 1) Build Tile assets from Bayat sprites
            var tilesFolder = "Assets/WWIII/Tiles/Age7";
            EnsureFolder(tilesFolder);

            // Find some ground candidate sprites (dirt/2D tiles)
            var groundSprites = FindSprites(new []{
                BayatRoot + "/Textures/Tiles/2D",
                BayatRoot + "/Textures/Tiles/Dirt",
            }, 24);
            var bgSprites = FindSprites(new []{
                BayatRoot + "/Textures/Background And Environment/Background",
                BayatRoot + "/Textures/Background And Environment/Environment",
            }, 16);
            var fgSprites = FindSprites(new []{
                BayatRoot + "/Textures/Chest",
                BayatRoot + "/Textures/UI/Icons",
            }, 8);

            var groundTiles = CreateTiles(tilesFolder + "/Ground", groundSprites);
            var backgroundTiles = CreateTiles(tilesFolder + "/Background", bgSprites);
            var foregroundTiles = CreateTiles(tilesFolder + "/Foreground", fgSprites);

            // 2) Assign to LevelThemeProfile age 7
            ageTheme.groundTiles = groundTiles.Cast<TileBase>().ToList();
            ageTheme.backgroundTiles = backgroundTiles.Cast<TileBase>().ToList();
            ageTheme.foregroundTiles = foregroundTiles.Cast<TileBase>().ToList();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3) Put some Bayat props into propPrefabs for visual storytelling
            var propCandidates = new []
            {
                BayatRoot + "/Prefabs/Items/Coin/Coin.prefab",
                BayatRoot + "/Prefabs/Items/Key/Key.prefab",
                BayatRoot + "/Prefabs/Particle/Leaf_Particle.prefab",
            };
            ageTheme.propPrefabs = propCandidates
                .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p))
                .Where(p => p != null)
                .ToList();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            // 4) Make PhotoCollectible prefab visible using Bayat coin sprite
            AssignPhotoCollectibleSprite();

            EditorUtility.DisplayDialog("WWIII", "Age 7 visuals assigned: tiles, props, photo sprite.", "OK");
        }

        [MenuItem("WWIII/Art/Assign Bayat Enemy Prefabs (Fallback)")] 
        public static void AssignEnemyPrefabs()
        {
            var enemyFolder = "Assets/WWIII/Data/Definitions/Enemies";
            var guids = AssetDatabase.FindAssets("t:WWIII.SideScroller.Design.EnemyDefinition", new[]{ enemyFolder});
            if (guids.Length == 0) { EditorUtility.DisplayDialog("WWIII","No EnemyDefinition assets found.","OK"); return; }

            // Bayat enemy prefab options
            var options = new []
            {
                BayatRoot + "/Prefabs/Characters/Enemy 1/Enemy 1.prefab",
                BayatRoot + "/Prefabs/Characters/Armored Enemy 1/Armored Enemy 1.prefab",
                BayatRoot + "/Prefabs/Characters/Armored Enemy 2/Armored Enemy 2.prefab",
                BayatRoot + "/Prefabs/Characters/Armored Enemy 3/Armored Enemy 3.prefab",
                BayatRoot + "/Prefabs/Anima2D Characters/Enemies/Enemy_1.prefab",
                BayatRoot + "/Prefabs/Anima2D Characters/Enemies/Enemy_2.prefab",
            }.Where(p => AssetDatabase.LoadAssetAtPath<GameObject>(p) != null).ToArray();
            if (options.Length == 0) { EditorUtility.DisplayDialog("WWIII","No Bayat enemy prefabs found.","OK"); return; }

            int assigned = 0; int skipped = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var so = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
                var idProp = so.FindProperty("designId");
                var prefabProp = so.FindProperty("prefabKey");
                string current = prefabProp != null ? prefabProp.stringValue : null;
                bool missing = string.IsNullOrEmpty(current) || AssetDatabase.LoadAssetAtPath<GameObject>(current) == null;
                if (!missing) { skipped++; continue; }

                // Deterministic mapping by hash of designId
                var id = idProp != null ? idProp.stringValue : string.Empty;
                int index = Mathf.Abs(id.GetHashCode());
                string choice = options[index % options.Length];
                prefabProp.stringValue = choice;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
                assigned++;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", $"Enemy prefab assignment complete. Assigned: {assigned}, Skipped: {skipped}", "OK");
        }

        [MenuItem("WWIII/Art/Assign PhotoCollectible Sprite (Bayat)")] 
        public static void AssignPhotoCollectibleSprite()
        {
            var prefabPath = "Assets/WWIII/Prefabs/Collectibles/PhotoCollectible.prefab";
            if (!File.Exists(prefabPath)) return;
            var coinPrefabPath = BayatRoot + "/Prefabs/Items/Coin/Coin.prefab";
            var coin = AssetDatabase.LoadAssetAtPath<GameObject>(coinPrefabPath);
            Sprite coinSprite = null;
            if (coin != null)
            {
                var srCoin = coin.GetComponentInChildren<SpriteRenderer>();
                if (srCoin != null) coinSprite = srCoin.sprite;
            }
            if (coinSprite == null)
            {
                // fallback: find any sprite under Bayat root
                var any = FindSprites(new[]{BayatRoot}, 1).FirstOrDefault();
                coinSprite = any;
            }
            if (coinSprite == null) return;

            var temp = PrefabUtility.LoadPrefabContents(prefabPath);
            var sr = temp.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite == null)
            {
                sr.sprite = coinSprite;
            }
            PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            PrefabUtility.UnloadPrefabContents(temp);
        }

        // Helpers
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

        private static List<Sprite> FindSprites(string[] folders, int limit)
        {
            var result = new List<Sprite>();
            var guids = AssetDatabase.FindAssets("t:Sprite", folders);
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null) result.Add(s);
                if (result.Count >= limit) break;
            }
            return result;
        }

        private static List<Tile> CreateTiles(string rootFolder, List<Sprite> sprites)
        {
            EnsureFolder(rootFolder);
            var tiles = new List<Tile>();
            foreach (var s in sprites)
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = s;
                tile.name = s.name + "_Tile";
                var path = rootFolder + "/" + Sanitize(s.name) + ".asset";
                AssetDatabase.CreateAsset(tile, path);
                tiles.Add(tile);
            }
            AssetDatabase.SaveAssets();
            return tiles;
        }

        private static string Sanitize(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }
    }
}

