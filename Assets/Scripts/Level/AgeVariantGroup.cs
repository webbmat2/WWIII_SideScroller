using System;
using System.Collections.Generic;
using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Level
{
    public class AgeVariantGroup : MonoBehaviour
    {
        [Serializable]
        public class Variant
        {
            public string name;
            public int minYears = 7;
            public int maxYears = 999;
            public GameObject root;
        }

        public List<Variant> variants = new();
        private AgeManager _ageManager;

        private void OnEnable()
        {
            _ageManager = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (_ageManager != null)
            {
                _ageManager.OnAgeChanged += Apply;
                if (_ageManager.CurrentAge != null) Apply(_ageManager.CurrentAge);
            }
        }

        private void OnDisable()
        {
            if (_ageManager != null) _ageManager.OnAgeChanged -= Apply;
        }

        private void Apply(AgeProfile profile)
        {
            if (profile == null) return;
            foreach (var v in variants)
            {
                if (v?.root == null) continue;
                bool active = profile.ageYears >= v.minYears && profile.ageYears <= v.maxYears;
                if (v.root.activeSelf != active) v.root.SetActive(active);
            }
        }
    }
}

