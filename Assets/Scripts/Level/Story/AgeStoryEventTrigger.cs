using UnityEngine;
using UnityEngine.Events;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Level.Story
{
    [RequireComponent(typeof(Collider2D))]
    public class AgeStoryEventTrigger : MonoBehaviour
    {
        public int minYears = 7;
        public int maxYears = 999;
        public UnityEvent onEnterAgeRange;

        private void Reset()
        {
            var c = GetComponent<Collider2D>();
            c.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<IAgeAwareCharacter>() == null) return;
            var ageMgr = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (ageMgr == null || ageMgr.CurrentAge == null) return;
            var y = ageMgr.CurrentAge.ageYears;
            if (y >= minYears && y <= maxYears)
                onEnterAgeRange?.Invoke();
        }
    }
}

