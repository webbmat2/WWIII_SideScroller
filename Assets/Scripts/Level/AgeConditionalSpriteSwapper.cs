using System;
using System.Collections.Generic;
using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Level
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AgeConditionalSpriteSwapper : MonoBehaviour
    {
        [Serializable]
        public struct AgeSprite
        {
            public int ageYears;
            public Sprite sprite;
        }

        public List<AgeSprite> sprites = new List<AgeSprite>();
        public bool inheritSortingLayer = true;

        private SpriteRenderer _renderer;
        private AgeManager _ageManager;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _ageManager = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (_ageManager != null)
            {
                _ageManager.OnAgeChanged += OnAgeChanged;
                OnAgeChanged(_ageManager.CurrentAge);
            }
        }

        private void OnDisable()
        {
            if (_ageManager != null)
            {
                _ageManager.OnAgeChanged -= OnAgeChanged;
                _ageManager = null;
            }
        }

        private void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null || _renderer == null) return;
            var sprite = sprites.Find(s => s.ageYears == profile.ageYears).sprite;
            if (sprite != null)
            {
                _renderer.sprite = sprite;
            }
        }
    }
}

