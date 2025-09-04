using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WWIII.SideScroller.Collectibles;

namespace WWIII.SideScroller.UI.PhotoAlbum
{
    public class PhotoAlbumUIController : MonoBehaviour
    {
        [Header("UI Hooks")]
        public Transform gridRoot;
        public PhotoItemUI itemPrefab;

        [Header("Config")]
        [Tooltip("Optional Yarn node prefix; final node will be prefix + photoId.")]
        public string yarnNodePrefix = "Photo_";

        private readonly List<PhotoItemUI> _items = new();

        private void OnEnable()
        {
            if (PhotoAlbumService.Instance != null)
            {
                PhotoAlbumService.Instance.OnPhotoCollected += OnPhotoCollected;
            }
            Rebuild();
        }

        private void OnDisable()
        {
            if (PhotoAlbumService.Instance != null)
            {
                PhotoAlbumService.Instance.OnPhotoCollected -= OnPhotoCollected;
            }
        }

        private void OnPhotoCollected(string id)
        {
            Rebuild();
        }

        public void Rebuild()
        {
            if (gridRoot == null || itemPrefab == null || PhotoAlbumService.Instance == null) return;
            foreach (Transform child in gridRoot) Destroy(child.gameObject);
            _items.Clear();

            // Build from saved photo IDs
            var save = WWIII.SideScroller.Save.ProgressionSaveService.Instance;
            if (save == null) return;
            foreach (var id in save.Data.collectedPhotoIds)
            {
                var item = Instantiate(itemPrefab, gridRoot);
                item.Bind(id, OnPhotoClicked);
                _items.Add(item);
            }
        }

        private void OnPhotoClicked(string id)
        {
            // Try to open Yarn node if present
            var dialogueRunnerType = System.Type.GetType("Yarn.Unity.DialogueRunner, Yarn.Unity");
            if (dialogueRunnerType == null) return;
            var runner = UnityEngine.Object.FindFirstObjectByType(dialogueRunnerType);
            if (runner == null) return;
            var startMethod = dialogueRunnerType.GetMethod("StartDialogue", new[] { typeof(string) });
            if (startMethod == null) return;
            startMethod.Invoke(runner, new object[] { yarnNodePrefix + id });
        }
    }

    public class PhotoItemUI : MonoBehaviour
    {
        public Image image;
        public Button button;
        private string _id;

        public void Bind(string id, System.Action<string> onClicked)
        {
            _id = id;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClicked?.Invoke(_id));
            }
        }
    }
}
