using UnityEngine;
using UnityEngine.UI;

namespace WWIII.SideScroller.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class TvOsCanvasScaler : MonoBehaviour
    {
        [Tooltip("Scale factor to apply on tvOS for living-room readability")] 
        public float tvOsScaleFactor = 1.25f;
        [Tooltip("Scale factor on handheld/mobile")] 
        public float mobileScaleFactor = 1.0f;
        public bool applyOnUpdate = false;

        private CanvasScaler _cs;
        private void Awake()
        {
            _cs = GetComponent<CanvasScaler>();
            Apply();
        }

        private void Update()
        {
            if (applyOnUpdate) Apply();
        }

        public void Apply()
        {
            if (_cs == null) return;
#if UNITY_TVOS
            _cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _cs.scaleFactor = tvOsScaleFactor;
#else
            _cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _cs.scaleFactor = mobileScaleFactor;
#endif
        }
    }
}

