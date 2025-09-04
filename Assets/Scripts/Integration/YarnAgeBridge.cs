using System.Linq;
using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Integration
{
    public class YarnAgeBridge : MonoBehaviour
    {
        public AgeManager ageManager;

        private void Reset()
        {
            if (ageManager == null)
                ageManager = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
        }

#if YARNSPINNER
        [Yarn.Unity.YarnCommand("set_age")] // usage: <<set_age 2>>
#endif
        public void SetAge(int index)
        {
            if (ageManager == null || ageManager.ageSet == null) return;
            index = Mathf.Clamp(index, 0, ageManager.ageSet.Count - 1);
            _ = ageManager.RequestAgeIndexAsync(index, true);
        }

#if YARNSPINNER
        [Yarn.Unity.YarnCommand("set_age_years")] // usage: <<set_age_years 12>>
#endif
        public void SetAgeYears(int years)
        {
            if (ageManager == null || ageManager.ageSet == null) return;
            var idx = ageManager.ageSet.ages.FindIndex(a => a != null && a.ageYears == years);
            if (idx >= 0)
            {
                _ = ageManager.RequestAgeIndexAsync(idx, true);
            }
        }

#if YARNSPINNER
        [Yarn.Unity.YarnCommand("next_age")] // usage: <<next_age>>
#endif
        public void NextAge()
        {
            if (ageManager == null) return;
            _ = ageManager.NextAgeAsync(true);
        }

#if YARNSPINNER
        [Yarn.Unity.YarnCommand("prev_age")] // usage: <<prev_age>>
#endif
        public void PrevAge()
        {
            if (ageManager == null) return;
            _ = ageManager.PreviousAgeAsync(false);
        }
    }
}
