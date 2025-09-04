using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.Aging
{
    [CreateAssetMenu(menuName = "WWIII/Aging/Age Set", fileName = "AgeSet")]
    public class AgeSet : ScriptableObject
    {
        [Tooltip("Ordered list of age profiles from youngest to oldest.")]
        public List<AgeProfile> ages = new List<AgeProfile>();

        public int Count => ages?.Count ?? 0;

        public AgeProfile Get(int index)
        {
            if (ages == null || index < 0 || index >= ages.Count) return null;
            return ages[index];
        }

        public int IndexOf(AgeProfile profile)
        {
            if (ages == null || profile == null) return -1;
            return ages.IndexOf(profile);
        }

        public AgeProfile GetNext(AgeProfile current)
        {
            var i = IndexOf(current);
            if (i < 0) return null;
            var nextIndex = Mathf.Min(i + 1, ages.Count - 1);
            return ages[nextIndex];
        }

        public AgeProfile GetPrevious(AgeProfile current)
        {
            var i = IndexOf(current);
            if (i < 0) return null;
            var prevIndex = Mathf.Max(i - 1, 0);
            return ages[prevIndex];
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < ages.Count; i++)
            {
                if (ages[i] != null)
                {
                    ages[i].index = i;
                }
            }
        }
#endif
    }
}
