using UnityEngine;
using UnityEngine.UI;

namespace WWIII.SideScroller.UI
{
    public class SafeAreaOverlay : MonoBehaviour
    {
        [Range(0f,1f)] public float alpha = 0.25f;
        [Tooltip("Overlay color outside the safe area")] public Color color = Color.red;
        private Image _img;
        private Rect _last;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var cgo = new GameObject("Canvas"); cgo.transform.SetParent(transform);
                var c = cgo.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            var go = new GameObject("SafeAreaOverlay"); go.transform.SetParent(transform, false);
            _img = go.AddComponent<Image>();
            _img.raycastTarget = false;
            Apply();
        }

        private void Update()
        {
            Apply();
        }

        private void Apply()
        {
            if (_img == null) return;
            var safe = Screen.safeArea; if (safe == _last) return; _last = safe;
            _img.color = new Color(color.r, color.g, color.b, alpha);
            var rt = _img.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            // Create a mask to hollow the safe area is complex; instead draw a 1px border via Outline-like effect is nontrivial without shaders.
            // Simplification: scale overlay so safe area stays fully visible, leaving margins tinted.
            // We'll overlay four child images for borders.
            EnsureBorders();
        }

        private Image _top,_bottom,_left,_right;
        private void EnsureBorders()
        {
            if (_top == null)
            {
                _top = NewBorder("Top"); _bottom = NewBorder("Bottom"); _left = NewBorder("Left"); _right = NewBorder("Right");
            }
            var safe = Screen.safeArea;
            var w = Screen.width; var h = Screen.height;
            float left = safe.xMin / w; float right = 1f - (safe.xMax / w);
            float bottom = safe.yMin / h; float top = 1f - (safe.yMax / h);
            SetBorder(_top, new Vector2(0,1-top), new Vector2(1,1));
            SetBorder(_bottom, new Vector2(0,0), new Vector2(1,bottom));
            SetBorder(_left, new Vector2(0,bottom), new Vector2(left,1-top));
            SetBorder(_right, new Vector2(1-right,bottom), new Vector2(1,1-top));
        }
        private Image NewBorder(string n)
        {
            var go = new GameObject(n); go.transform.SetParent(_img.transform, false);
            var im = go.AddComponent<Image>(); im.color = _img.color; im.raycastTarget = false; return im;
        }
        private void SetBorder(Image im, Vector2 min, Vector2 max)
        {
            var rt = im.rectTransform; rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
    }
}

