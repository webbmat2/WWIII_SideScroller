using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.Level
{
    [CreateAssetMenu(menuName = "WWIII/Level/Level Registry", fileName = "LevelRegistry")]
    public class LevelRegistry : ScriptableObject
    {
        public List<LevelDefinition> levels = new();

        public LevelDefinition GetByAgeYears(int years)
        {
            LevelDefinition best = null; int bestDelta = int.MaxValue;
            foreach (var l in levels)
            {
                if (l == null) continue;
                int d = Mathf.Abs(l.ageYears - years);
                if (d < bestDelta) { bestDelta = d; best = l; }
            }
            return best;
        }
    }
}

