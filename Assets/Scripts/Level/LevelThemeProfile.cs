using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WWIII.SideScroller.Level
{
    [CreateAssetMenu(menuName = "WWIII/Level/Level Theme Profile", fileName = "LevelThemeProfile")]
    public class LevelThemeProfile : ScriptableObject
    {
        [Serializable]
        public class AgeTheme
        {
            public int ageYears = 7;
            [Header("Tiles")]
            public List<TileBase> groundTiles = new();
            public List<TileBase> backgroundTiles = new();
            public List<TileBase> foregroundTiles = new();

            [Header("Prefabs")] public List<GameObject> platformPrefabs = new();
            public List<GameObject> propPrefabs = new();

            [Header("Colors")] public Color ambientTint = Color.white;
        }

        public List<AgeTheme> themes = new();

        public AgeTheme GetForYears(int years)
        {
            AgeTheme best = null; int bestDelta = int.MaxValue;
            foreach (var t in themes)
            {
                var d = Mathf.Abs(t.ageYears - years);
                if (d < bestDelta) { bestDelta = d; best = t; }
            }
            return best;
        }
    }
}

