using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; // Addressables API (runtime)
using UnityEngine.ResourceManagement.AsyncOperations; // AsyncOperationHandle
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Integration.AssetStreaming
{
    public class AgeAddressablesLoader : MonoBehaviour
    {
        [Tooltip("AgeManager to listen to for label loads.")]
        public AgeManager ageManager;

        private readonly List<AsyncOperationHandle> _handles = new();

        private void Reset()
        {
            ageManager = FindFirstObjectByType<AgeManager>();
        }

        private void OnEnable()
        {
            if (ageManager == null) ageManager = FindFirstObjectByType<AgeManager>();
            if (ageManager != null)
            {
                ageManager.OnAgeChanged += OnAgeChanged;
                if (ageManager.CurrentAge != null) OnAgeChanged(ageManager.CurrentAge);
            }
        }

        private void OnDisable()
        {
            if (ageManager != null) ageManager.OnAgeChanged -= OnAgeChanged;
            foreach (var h in _handles) if (h.IsValid()) Addressables.Release(h);
            _handles.Clear();
        }

        private async void OnAgeChanged(AgeProfile profile)
        {
            // clear old
            foreach (var h in _handles)
            {
                if (h.IsValid()) Addressables.Release(h);
            }
            _handles.Clear();

            if (profile.audioLabel != null && !string.IsNullOrEmpty(profile.audioLabel.labelString))
            {
                // Load all audio assets for this age label
                AsyncOperationHandle<System.Collections.Generic.IList<UnityEngine.Object>> handle =
                    Addressables.LoadAssetsAsync<UnityEngine.Object>(profile.audioLabel, _ => { /* warm only */ });
                await handle.Task;
                if (handle.IsValid())
                {
                    _handles.Add(handle);
                }
            }
        }
    }
}
