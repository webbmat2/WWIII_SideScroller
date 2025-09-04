using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace WWIII.SideScroller.Aging
{
    public interface IAgeAwareCharacter
    {
        void ApplyAgeMovement(AgeProfile.MovementConfig config);
        void OnAgeChanged(AgeProfile profile);
    }

    public class AgeManager : MonoBehaviour
    {
        [Header("Config")] public AgeSet ageSet;
        [Tooltip("Index to start at when the game begins.")]
        public int initialAgeIndex = 0;

        [Header("Player Swapping")]
        [Tooltip("Parent transform for player prefab instances (if profile provides one). If not provided, current scene player will be used.")]
        public Transform playerRoot;

        [Tooltip("Existing player instance to update if not swapping prefabs.")]
        public GameObject currentPlayer;

        [Header("Cutscenes & Dialogue (Optional)")]
        [Tooltip("PlayableDirector used to play age transition cutscenes.")]
        public PlayableDirector director;

        [Tooltip("Yarn variable name that mirrors current age in years. Set empty to disable.")]
        public string yarnAgeVariable = "jim_age";
        [Tooltip("If true and an AgeProfile provides a Yarn node name, it will auto-start that node on entering the age.")]
        public bool autoStartYarnNode = false;

        public event Action<AgeProfile> OnAgeWillChange;
        public event Action<AgeProfile> OnAgeChanged;

        public AgeProfile CurrentAge { get; private set; }
        public int CurrentIndex { get; private set; } = -1;

        public SpriteAtlas CurrentSpriteAtlas
        {
            get
            {
                if (_atlasHandle.HasValue && _atlasHandle.Value.IsValid())
                    return _atlasHandle.Value.Result;
                return null;
            }
        }

        // Addressable handles to release when switching
        private AsyncOperationHandle<RuntimeAnimatorController>? _animHandle;
        private AsyncOperationHandle<SpriteAtlas>? _atlasHandle;
        private AsyncOperationHandle<GameObject>? _prefabHandle;
        private AsyncOperationHandle<GameObject>? _instanceHandle;
        private AsyncOperationHandle<PlayableAsset>? _cutsceneHandle;

        // Guard against concurrent transitions
        private bool _isTransitioning;
        private string _lastCutsceneId;

        private void Awake()
        {
            if (currentPlayer == null)
            {
                // Attempt to find an existing player in children
                if (playerRoot != null)
                {
                    currentPlayer = playerRoot.GetComponentInChildren<IAgeAwareCharacter>() is Component c
                        ? c.gameObject
                        : null;
                }
            }
        }

        private async void Start()
        {
            // Initialize first age
            if (ageSet != null && ageSet.Count > 0)
            {
                // If a save exists, prefer it
                int savedIndex = -1;
                try
                {
                    var svcType = typeof(WWIII.SideScroller.Save.ProgressionSaveService);
                    var svc = UnityEngine.Object.FindFirstObjectByType(svcType);
                    if (svc != null)
                    {
                        var dataProp = svcType.GetProperty("Data");
                        var data = dataProp?.GetValue(svc);
                        var idxField = data?.GetType().GetField("currentAgeIndex");
                        if (idxField != null)
                        {
                            savedIndex = (int)idxField.GetValue(data);
                        }
                    }
                }
                catch { /* ignore */ }

                var i = Mathf.Clamp(savedIndex >= 0 ? savedIndex : initialAgeIndex, 0, ageSet.Count - 1);
                await RequestAgeIndexAsync(i, false);
            }
        }

        public async Task RequestAgeIndexAsync(int index, bool playCutscene = true)
        {
            if (_isTransitioning || ageSet == null || ageSet.Count == 0) return;
            index = Mathf.Clamp(index, 0, ageSet.Count - 1);
            var next = ageSet.Get(index);
            if (next == null) return;

            _isTransitioning = true;
            try
            {
                OnAgeWillChange?.Invoke(next);

                // Load assets for the upcoming age
                var tasks = new List<Task>();
                AsyncOperationHandle<GameObject> prefabHandle = default;
                AsyncOperationHandle<RuntimeAnimatorController> animHandle = default;
                AsyncOperationHandle<SpriteAtlas> atlasHandle = default;
                AsyncOperationHandle<PlayableAsset> cutsceneHandle = default;

                if (next.playerPrefab.RuntimeKeyIsValid())
                {
                    prefabHandle = next.playerPrefab.LoadAssetAsync();
                    tasks.Add(prefabHandle.Task);
                }
                else if (next.animatorController.RuntimeKeyIsValid())
                {
                    animHandle = next.animatorController.LoadAssetAsync();
                    tasks.Add(animHandle.Task);
                }

                if (next.spriteAtlas.RuntimeKeyIsValid())
                {
                    atlasHandle = next.spriteAtlas.LoadAssetAsync();
                    tasks.Add(atlasHandle.Task);
                }

                if (playCutscene && director != null && next.transitionCutscene.RuntimeKeyIsValid())
                {
                    cutsceneHandle = next.transitionCutscene.LoadAssetAsync();
                    tasks.Add(cutsceneHandle.Task);
                }

                // Await all loads
                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                }

                // Swap or update player
                GameObject newPlayer = currentPlayer;
                if (next.playerPrefab.RuntimeKeyIsValid())
                {
                    if (playerRoot == null)
                        playerRoot = transform;

                    // Instantiate asynchronously to avoid hitches on mobile
                    var instHandle = next.playerPrefab.InstantiateAsync(playerRoot);
                    await instHandle.Task;

                    // Release old Addressables instance, if any
                    if (_instanceHandle.HasValue)
                    {
                        Addressables.ReleaseInstance(_instanceHandle.Value);
                        _instanceHandle = null;
                    }

                    newPlayer = instHandle.Result;
                    _instanceHandle = instHandle;
                    _prefabHandle = prefabHandle;
                }
                else if (next.animatorController.RuntimeKeyIsValid())
                {
                    var controller = animHandle.Result;
                    if (currentPlayer == null)
                    {
                        Debug.LogWarning("AgeManager: No current player; cannot apply animator-only age.");
                    }
                    else
                    {
                        var animator = currentPlayer.GetComponentInChildren<Animator>();
                        if (animator != null)
                        {
                            animator.runtimeAnimatorController = controller;
                        }
                        _animHandle = animHandle;
                    }
                }

                // Assign new player
                if (newPlayer != null)
                {
                    currentPlayer = newPlayer;

                    // Apply movement config if supported
                    var ageAware = currentPlayer.GetComponentInChildren<IAgeAwareCharacter>();
                    if (ageAware != null)
                    {
                        ageAware.ApplyAgeMovement(next.movement);
                    }

                    // Set physics layer recursively
                    var layer = next.ResolveLayer();
                    SetLayerRecursively(currentPlayer, layer);
                }

                // Release previous atlas/controller handles to free memory
                ReleaseOldHandlesExcept(_prefabHandle, _animHandle, _atlasHandle, _cutsceneHandle);

                if (next.spriteAtlas.RuntimeKeyIsValid())
                {
                    _atlasHandle = atlasHandle;
                }
                if (playCutscene && director != null && next.transitionCutscene.RuntimeKeyIsValid())
                {
                    _cutsceneHandle = cutsceneHandle;
                    director.playableAsset = cutsceneHandle.Result;
                    _lastCutsceneId = SafeGetAssetId(next.transitionCutscene);
                    director.stopped -= OnDirectorStopped;
                    director.stopped += OnDirectorStopped;
                    director.Play();
                }

                CurrentAge = next;
                CurrentIndex = index;

                // Notify age-aware character
                if (currentPlayer != null)
                {
                    var ageAware = currentPlayer.GetComponentInChildren<IAgeAwareCharacter>();
                    if (ageAware != null)
                    {
                        ageAware.OnAgeChanged(CurrentAge);
                    }
                }

                // Push to Yarn (if present) via reflection to avoid hard dep
                TrySetYarnAgeVariable(CurrentAge.ageYears);
                if (autoStartYarnNode && !string.IsNullOrEmpty(CurrentAge.yarnStartNode))
                {
                    TryStartYarnNode(CurrentAge.yarnStartNode);
                }

                OnAgeChanged?.Invoke(CurrentAge);

                // Persist age index
                try
                {
                    WWIII.SideScroller.Save.ProgressionSaveService.Instance?.SetAgeIndex(CurrentIndex);
                }
                catch { /* ignore */ }
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            if (string.IsNullOrEmpty(_lastCutsceneId)) return;
            try
            {
                WWIII.SideScroller.Save.ProgressionSaveService.Instance?.MarkCutsceneCompleted(_lastCutsceneId);
            }
            catch { /* ignore */ }
            _lastCutsceneId = null;
        }

        private static string SafeGetAssetId(AssetReference asset)
        {
            try
            {
                return asset.AssetGUID;
            }
            catch
            {
                return asset.RuntimeKey?.ToString();
            }
        }

        public async Task NextAgeAsync(bool playCutscene = true)
        {
            if (ageSet == null) return;
            var nextIdx = Mathf.Min(CurrentIndex + 1, ageSet.Count - 1);
            await RequestAgeIndexAsync(nextIdx, playCutscene);
        }

        public async Task PreviousAgeAsync(bool playCutscene = false)
        {
            if (ageSet == null) return;
            var prevIdx = Mathf.Max(CurrentIndex - 1, 0);
            await RequestAgeIndexAsync(prevIdx, playCutscene);
        }

        private void ReleaseOldHandlesExcept(
            AsyncOperationHandle<GameObject>? keepPrefab,
            AsyncOperationHandle<RuntimeAnimatorController>? keepAnim,
            AsyncOperationHandle<SpriteAtlas>? keepAtlas,
            AsyncOperationHandle<PlayableAsset>? keepCutscene)
        {
            // Animator
            if (_animHandle.HasValue && (!keepAnim.HasValue || !_animHandle.Value.Equals(keepAnim.Value)))
            {
                Addressables.Release(_animHandle.Value);
            }
            _animHandle = keepAnim;

            // Atlas
            if (_atlasHandle.HasValue && (!keepAtlas.HasValue || !_atlasHandle.Value.Equals(keepAtlas.Value)))
            {
                Addressables.Release(_atlasHandle.Value);
            }
            _atlasHandle = keepAtlas;

            // Prefab asset handle (we keep instance separately)
            if (_prefabHandle.HasValue && (!keepPrefab.HasValue || !_prefabHandle.Value.Equals(keepPrefab.Value)))
            {
                Addressables.Release(_prefabHandle.Value);
            }
            _prefabHandle = keepPrefab;

            // Cutscene
            if (_cutsceneHandle.HasValue && (!keepCutscene.HasValue || !_cutsceneHandle.Value.Equals(keepCutscene.Value)))
            {
                Addressables.Release(_cutsceneHandle.Value);
            }
            _cutsceneHandle = keepCutscene;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            if (go == null) return;
            go.layer = layer;
            foreach (Transform t in go.transform)
            {
                if (t == null) continue;
                SetLayerRecursively(t.gameObject, layer);
            }
        }

        private void TrySetYarnAgeVariable(int ageYears)
        {
            if (string.IsNullOrEmpty(yarnAgeVariable)) return;

            // Try Yarn Spinner 2.x VariableStorage
            var yarnStorageType = Type.GetType("Yarn.Unity.InMemoryVariableStorage, Yarn.Unity");
            var dialogueRunnerType = Type.GetType("Yarn.Unity.DialogueRunner, Yarn.Unity");
            if (dialogueRunnerType == null || yarnStorageType == null) return;

            var runner = UnityEngine.Object.FindFirstObjectByType(dialogueRunnerType);
            if (runner == null) return;

            // DialogueRunner has a public VariableStorage property
            var storageProp = dialogueRunnerType.GetProperty("VariableStorage");
            var storage = storageProp?.GetValue(runner);
            if (storage == null) return;

            var setValueMethod = yarnStorageType.GetMethod("SetValue");
            if (setValueMethod != null)
            {
                try { setValueMethod.Invoke(storage, new object[] { "$" + yarnAgeVariable, ageYears }); } catch { }
                try { setValueMethod.Invoke(storage, new object[] { yarnAgeVariable, ageYears }); } catch { }
            }
        }

        private void TryStartYarnNode(string node)
        {
            var dialogueRunnerType = Type.GetType("Yarn.Unity.DialogueRunner, Yarn.Unity");
            if (dialogueRunnerType == null) return;
            var runner = UnityEngine.Object.FindFirstObjectByType(dialogueRunnerType);
            if (runner == null) return;
            var startMethod = dialogueRunnerType.GetMethod("StartDialogue", new[] { typeof(string) });
            if (startMethod == null) return;
            try
            {
                startMethod.Invoke(runner, new object[] { node });
            }
            catch { }
        }

        private void OnDestroy()
        {
            if (_instanceHandle.HasValue)
            {
                Addressables.ReleaseInstance(_instanceHandle.Value);
                _instanceHandle = null;
            }
            if (_animHandle.HasValue) Addressables.Release(_animHandle.Value);
            if (_atlasHandle.HasValue) Addressables.Release(_atlasHandle.Value);
            if (_prefabHandle.HasValue) Addressables.Release(_prefabHandle.Value);
            if (_cutsceneHandle.HasValue) Addressables.Release(_cutsceneHandle.Value);
        }
    }
}
