using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WWIII.SideScroller.Editor.Level
{
    public static class BiographicalTilemapBuilder
    {
        public struct Tilemaps
        {
            public Grid grid;
            public Tilemap ground;
            public Tilemap back;
            public Tilemap front;
        }

        public static Tilemaps CreateGridWithTilemaps(string rootName = "TilemapRoot")
        {
            var root = new GameObject(rootName);
            var grid = root.AddComponent<Grid>();

            Tilemap Create(string name, int order)
            {
                var go = new GameObject(name);
                go.transform.SetParent(root.transform);
                var tm = go.AddComponent<Tilemap>();
                var r = go.AddComponent<TilemapRenderer>();
                r.mode = TilemapRenderer.Mode.Individual;
                r.sortingOrder = order;
                return tm;
            }

            var maps = new Tilemaps
            {
                grid = grid,
                ground = Create("Ground", 0),
                back = Create("Background", -10),
                front = Create("Foreground", 10)
            };
            return maps;
        }

        public static void PopulateGround(Tilemap ground, TileBase[] tiles, int width = 96, int height = 32, int baseline = 8, int seed = 0)
        {
            if (ground == null || tiles == null || tiles.Length == 0) return;
            var rng = new System.Random(seed);
            for (int x = 0; x < width; x++)
            {
                int h = baseline + (int)(Mathf.PerlinNoise((x + seed) * 0.1f, 0.1f) * 4f);
                for (int y = 0; y <= h; y++)
                {
                    var t = tiles[rng.Next(tiles.Length)];
                    ground.SetTile(new Vector3Int(x, y, 0), t);
                }
            }
            ground.CompressBounds();
        }
    }
}

