using UnityEngine;

namespace WWIII.SideScroller.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPadding : MonoBehaviour
    {
        [Tooltip("Extra margin percent for tvOS overscan (0..0.2)")]
        [Range(0f,0.2f)] public float tvOSMargin = 0.06f;
        public bool applyOnUpdate = false;

        private RectTransform _rt;
        private Rect _last;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (applyOnUpdate) Apply();
        }

        public void Apply()
        {
            if (_rt == null) return;
            Rect safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) return;
            if (safe == _last && !applyOnUpdate) return;
            _last = safe;

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

#if UNITY_TVOS
            // tvOS often reports full safe area; add manual overscan margin
            anchorMin += new Vector2(tvOSMargin, tvOSMargin);
            anchorMax -= new Vector2(tvOSMargin, tvOSMargin);
            anchorMin = Vector2.Max(anchorMin, Vector2.zero);
            anchorMax = Vector2.Min(anchorMax, Vector2.one);
#endif
            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}

