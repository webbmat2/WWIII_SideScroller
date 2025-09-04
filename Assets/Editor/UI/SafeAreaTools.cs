using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.UI;

namespace WWIII.SideScroller.Editor.UI
{
    public static class SafeAreaTools
    {
        [MenuItem("WWIII/Diagnostics/UI/Add Safe Area Overlay")]
        public static void AddOverlay()
        {
            var canv = Object.FindFirstObjectByType<UnityEngine.UI.CanvasScaler>();
            GameObject parent = canv != null ? canv.gameObject : null;
            if (parent == null)
            {
                var go = new GameObject("Canvas");
                go.AddComponent<UnityEngine.UI.CanvasScaler>();
                go.AddComponent<UnityEngine.Canvas>().renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
                go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                parent = go;
            }
            var holder = new GameObject("SafeAreaOverlayHolder");
            holder.transform.SetParent(parent.transform, false);
            holder.AddComponent<SafeAreaOverlay>();
            Selection.activeGameObject = holder;
        }

        [MenuItem("WWIII/Diagnostics/UI/Add Safe Area Overlay", true)]
        private static bool ValidateAddOverlay()
        {
            return WWIII.SideScroller.Editor.Diagnostics.DiagnosticsMenu.IsEnabled;
        }
    }
}
