using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WWIII.SideScroller.DevTools;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Editor.Flow
{
    public static class CreateControllerSandboxScene
    {
        [MenuItem("WWIII/Diagnostics/Controller/Create Sandbox Scene")] 
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>(); canvasGO.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            // HUD
            var textGO = new GameObject("Info");
            textGO.transform.SetParent(canvasGO.transform);
            var text = textGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.UpperLeft; text.raycastTarget = false; text.color = Color.white;
            var rt = text.GetComponent<RectTransform>(); rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(16,-16); rt.sizeDelta = new Vector2(600,400);

            // Age system + dummy age set
            var ageRoot = new GameObject("AgeSystem"); var mgr = ageRoot.AddComponent<AgeManager>();

            // HUD controller
            var hudGO = new GameObject("ControllerHUD");
            var hud = hudGO.AddComponent<ControllerSandboxHUD>();
            hud.info = text; hud.ageManager = mgr;

            EditorSceneManager.SaveScene(scene, "Assets/WWIII/Scenes/ControllerSandbox.unity");
            EditorUtility.DisplayDialog("WWIII", "Controller Sandbox scene created.", "OK");
        }

        // Validation to gate Diagnostics menu visibility
        [MenuItem("WWIII/Diagnostics/Controller/Create Sandbox Scene", true)]
        private static bool ValidateCreateScene()
        {
            return WWIII.SideScroller.Editor.Diagnostics.DiagnosticsMenu.IsEnabled;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }
    }
}
