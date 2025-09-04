using System;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace WWIII.SideScroller.Characters
{
    [CreateAssetMenu(fileName = "AgeAppearanceSet", menuName = "WWIII/Characters/AgeAppearanceSet")] 
    public class AgeAppearanceSet : ScriptableObject
    {
        [Serializable]
        public class AgeAppearance
        {
            [Header("Age")]
            public int ageYears = 7;

            [Header("Sprite Library (Per-Age Atlas)")]
            public SpriteLibraryAsset spriteLibraryAsset;

            [Header("Labels")]
            public string hairLabel;
            public string outfitLabel;
            public string accessoryLabel;

            [Header("Tints")]
            public Color skinTint = Color.white;
            public Color hairTint = Color.white;

            [Header("Scale")]
            public Vector3 localScale = Vector3.one;

            [Header("Notes")]
            [TextArea(1,3)] public string notes;
        }

        public AgeAppearance[] entries;

        public AgeAppearance GetForAge(int age)
        {
            if (entries == null || entries.Length == 0) return null;
            // exact match first
            foreach (var e in entries)
            {
                if (e != null && e.ageYears == age) return e;
            }
            // fallback: closest lower age
            AgeAppearance best = null;
            int bestAge = int.MinValue;
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (e.ageYears <= age && e.ageYears > bestAge)
                {
                    bestAge = e.ageYears;
                    best = e;
                }
            }
            return best ?? entries[0];
        }
    }
}

