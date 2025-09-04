using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WWIII.SideScroller.Collectibles
{
    public class PhotoAlbumService : MonoBehaviour
    {
        public static PhotoAlbumService Instance { get; private set; }

        private readonly HashSet<string> _collected = new HashSet<string>();
        private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _loadedSprites = new();

        public event Action<string> OnPhotoCollected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool HasPhoto(string id) => _collected.Contains(id);

        public async Task AddPhotoAsync(string id, AssetReferenceT<Sprite> spriteRef)
        {
            if (string.IsNullOrEmpty(id) || _collected.Contains(id)) return;
            _collected.Add(id);

            // Persist progress
            Save.ProgressionSaveService.Instance?.AddPhoto(id);

            // Load sprite only on demand; but here we opportunistically warm it
            if (spriteRef != null && spriteRef.RuntimeKeyIsValid() && !_loadedSprites.ContainsKey(id))
            {
                var handle = spriteRef.LoadAssetAsync();
                await handle.Task;
                _loadedSprites[id] = handle;
            }

            OnPhotoCollected?.Invoke(id);
        }

        public async Task<Sprite> GetPhotoSpriteAsync(string id, AssetReferenceT<Sprite> spriteRef)
        {
            if (_loadedSprites.TryGetValue(id, out var handle) && handle.IsValid())
            {
                return handle.Result;
            }

            if (spriteRef != null && spriteRef.RuntimeKeyIsValid())
            {
                var newHandle = spriteRef.LoadAssetAsync();
                await newHandle.Task;
                _loadedSprites[id] = newHandle;
                return newHandle.Result;
            }

            return null;
        }

        public void UnloadAll()
        {
            foreach (var kvp in _loadedSprites)
            {
                if (kvp.Value.IsValid()) Addressables.Release(kvp.Value);
            }
            _loadedSprites.Clear();
        }

        private void OnDestroy()
        {
            UnloadAll();
        }
    }
}

