using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WWIII.SideScroller.UI.MainMenu;
using WWIII.SideScroller.UI.LevelSelect;
using WWIII.SideScroller.UI.PhotoAlbum;

namespace WWIII.SideScroller.Editor.Flow
{
    public static class CreateFlowScenes
    {
        [MenuItem("WWIII/Flow/Create Main Menu Scene")]
        public static void CreateMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var canvas = CreateCanvas();
            EnsureEventSystem();
            canvas.gameObject.AddComponent<WWIII.SideScroller.UI.SafeAreaPadding>();
            var menu = new GameObject("MainMenu").AddComponent<MainMenuController>();
            menu.transform.SetParent(canvas.transform);
            CreateButton(canvas.transform, "New Game", new Vector2(0, 80), menu.OnClickNewGame);
            CreateButton(canvas.transform, "Continue", new Vector2(0, 20), menu.OnClickContinue);
            CreateButton(canvas.transform, "Level Select", new Vector2(0, -40), menu.OnClickLevelSelect);
            CreateButton(canvas.transform, "Photo Album", new Vector2(0, -100), menu.OnClickPhotoAlbum);
            EditorSceneManager.SaveScene(scene, "Assets/WWIII/Scenes/MainMenu.unity");
        }

        [MenuItem("WWIII/Flow/Create Level Select Scene")]
        public static void CreateLevelSelect()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var canvas = CreateCanvas();
            EnsureEventSystem();
            canvas.gameObject.AddComponent<WWIII.SideScroller.UI.SafeAreaPadding>();
            var root = new GameObject("ListRoot").AddComponent<RectTransform>();
            root.transform.SetParent(canvas.transform);
            var vlg = root.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f; vlg.childForceExpandHeight = false; vlg.childControlHeight = true;

            var controller = new GameObject("LevelSelect").AddComponent<LevelSelectController>();
            controller.transform.SetParent(canvas.transform);
            controller.listRoot = root.transform;
            var item = new GameObject("LevelItem").AddComponent<LevelSelectItem>();
            item.transform.SetParent(canvas.transform);
            controller.itemPrefab = item;
            EditorSceneManager.SaveScene(scene, "Assets/WWIII/Scenes/LevelSelect.unity");
        }

        [MenuItem("WWIII/Flow/Create Photo Album Scene")]
        public static void CreatePhotoAlbum()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var canvas = CreateCanvas();
            EnsureEventSystem();
            canvas.gameObject.AddComponent<WWIII.SideScroller.UI.SafeAreaPadding>();
            var grid = new GameObject("GridRoot").AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(96, 96);
            grid.spacing = new Vector2(8, 8);
            grid.transform.SetParent(canvas.transform);
            var ctrl = new GameObject("PhotoAlbum").AddComponent<PhotoAlbumUIController>();
            ctrl.transform.SetParent(canvas.transform);
            ctrl.gridRoot = grid.transform;
            var item = new GameObject("PhotoItem").AddComponent<WWIII.SideScroller.UI.PhotoAlbum.PhotoItemUI>();
            item.transform.SetParent(canvas.transform);
            ctrl.itemPrefab = item;
            EditorSceneManager.SaveScene(scene, "Assets/WWIII/Scenes/PhotoAlbum.unity");
        }

        private static Canvas CreateCanvas()
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            return canvas;
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

        private static Button CreateButton(Transform parent, string text, Vector2 anchored, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(text.Replace(" ", "") + "Button");
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220, 40);
            var btn = go.AddComponent<Button>();
            var img = go.AddComponent<Image>(); img.color = new Color(0.2f,0.2f,0.2f,0.8f);

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform);
            var lbl = labelGO.AddComponent<Text>();
            lbl.text = text; lbl.alignment = TextAnchor.MiddleCenter; lbl.color = Color.white; lbl.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var lrt = labelGO.GetComponent<RectTransform>(); lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

            var crt = go.GetComponent<RectTransform>();
            crt.anchoredPosition = anchored; crt.anchorMin = new Vector2(0.5f, 0.5f); crt.anchorMax = new Vector2(0.5f, 0.5f);
            btn.onClick.AddListener(onClick);
            return btn;
        }
    }
}
