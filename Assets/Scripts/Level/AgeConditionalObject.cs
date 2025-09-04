using System;
using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Level
{
    [DisallowMultipleComponent]
    public class AgeConditionalObject : MonoBehaviour
    {
        public bool useYears = true;
        [Tooltip("Inclusive minimum age in years to show this object.")]
        public int minYears = 7;
        [Tooltip("Inclusive maximum age in years to show this object. Use 999 for no cap.")]
        public int maxYears = 999;

        [Tooltip("Optional: If not assigned, the first AgeManager in scene is used.")]
        public AgeManager ageManager;

        private void OnEnable()
        {
            if (ageManager == null) ageManager = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (ageManager != null)
            {
                ageManager.OnAgeChanged += HandleAgeChanged;
                HandleAgeChanged(ageManager.CurrentAge);
            }
            else
            {
                // No manager found; leave object enabled to avoid hiding content unintentionally
            }
        }

        private void OnDisable()
        {
            if (ageManager != null)
                ageManager.OnAgeChanged -= HandleAgeChanged;
        }

        private void HandleAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;
            bool show = true;
            if (useYears)
            {
                show = profile.ageYears >= minYears && profile.ageYears <= maxYears;
            }
            gameObject.SetActive(show);
        }
    }
}

