using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WWIII.SideScroller.Aging;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WWIII.SideScroller.Characters
{
#if SPINE_UNITY
    using Spine.Unity;
    using Spine;
#endif

    /// <summary>
    /// Spine integration: loads per-age SkeletonData via Addressables, applies skins (hair/outfit/accessories), tint and scale.
    /// Wrapped in SPINE_UNITY so project compiles without spine-unity installed. Define SPINE_UNITY after importing spine-unity.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpineAppearanceController : MonoBehaviour, IAgeAwareCharacter
    {
#if SPINE_UNITY
        [System.Serializable]
        public class AgeConfig
        {
            [Header("Age Range (inclusive)")]
            public int minAge = 7;
            public int maxAge = 12;

            [Header("Spine Data (Addressables)")]
            public AssetReferenceT<SkeletonDataAsset> skeletonDataRef;

            [Header("Global Appearance")]
            public float scale = 1f;
            public Color tint = Color.white;

            [Header("Skin Labels (category/name)")]
            public string hairSkin;      // e.g. Hair/Short_Black
            public string outfitSkin;    // e.g. Outfit/SchoolUniform
            public string accessorySkin; // e.g. Acc/Backpack

            public bool ContainsAge(int age) => age >= minAge && age <= maxAge;
        }

        [Header("Spine Animator")]
        [SerializeField] private SkeletonMecanim skeletonMecanim;

        [Header("Age Configs")] 
        [SerializeField] private List<AgeConfig> ages = new List<AgeConfig>();

        private readonly Dictionary<AgeConfig, AsyncOperationHandle<SkeletonDataAsset>> _handles = new();

        // Optional movement bridge
        private Age7Character age7Character;
        [Header("CSV Integration (StreamingAssets/Biographical/Character_Appearance.csv)")]
        [SerializeField] private bool useCSVOverrides = true;
        [SerializeField] private string csvRelativePath = "Biographical/Character_Appearance.csv";
        private List<WWIII.SideScroller.Data.CharacterAppearanceData> _csv;

        private void Awake()
        {
            if (skeletonMecanim == null) skeletonMecanim = GetComponentInChildren<SkeletonMecanim>();
            age7Character = GetComponent<Age7Character>();
            if (useCSVOverrides)
            {
                var path = System.IO.Path.Combine(Application.streamingAssetsPath, csvRelativePath);
                _csv = WWIII.SideScroller.Data.CharacterAppearanceData.LoadFromCSV(path);
            }
        }

        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            if (age7Character != null) age7Character.ApplyAgeMovement(config);
        }

        public void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;
            var cfg = GetConfigForAge(profile.ageYears);
            if (cfg == null)
            {
                Debug.LogWarning($"[SpineAppearanceController] No AgeConfig covers age {profile.ageYears}.");
                return;
            }
            StopAllCoroutines();
            StartCoroutine(ApplyAgeCoroutine(cfg, profile.ageYears));

            if (age7Character != null) age7Character.OnAgeChanged(profile);
        }

        private AgeConfig GetConfigForAge(int age)
        {
            foreach (var c in ages)
            {
                if (c != null && c.ContainsAge(age)) return c;
            }
            return ages.Count > 0 ? ages[0] : null;
        }

        private IEnumerator ApplyAgeCoroutine(AgeConfig cfg, int age)
        {
            if (skeletonMecanim == null)
            {
                Debug.LogWarning("[SpineAppearanceController] SkeletonMecanim not assigned.");
                yield break;
            }

            if (cfg.skeletonDataRef == null || !cfg.skeletonDataRef.RuntimeKeyIsValid())
            {
                Debug.LogWarning("[SpineAppearanceController] Invalid SkeletonDataAsset reference.");
                yield break;
            }

            if (!_handles.TryGetValue(cfg, out var handle) || !handle.IsValid())
            {
                handle = Addressables.LoadAssetAsync<SkeletonDataAsset>(cfg.skeletonDataRef);
                yield return handle;
                _handles[cfg] = handle;
            }

            var dataAsset = handle.Result;
            if (dataAsset == null)
            {
                Debug.LogWarning("[SpineAppearanceController] Loaded SkeletonDataAsset is null.");
                yield break;
            }

            // Swap skeleton data and re-init
            skeletonMecanim.skeletonDataAsset = dataAsset;
            skeletonMecanim.Initialize(true);

            var skeleton = skeletonMecanim.Skeleton;
            if (skeleton != null)
            {
                // Combine category skins
                var combined = new Skin("combined");
                var sdata = skeleton.Data;
                string hair = cfg.hairSkin, outfit = cfg.outfitSkin, acc = cfg.accessorySkin;
                Color tint = cfg.tint; float scl = cfg.scale;

                if (useCSVOverrides && _csv != null)
                {
                    var row = _csv.Find(r => r.age == age);
                    if (row != null)
                    {
                        if (!string.IsNullOrEmpty(row.hairStyle)) hair = $"Hair/{row.hairStyle}";
                        if (!string.IsNullOrEmpty(row.outfitType)) outfit = $"Outfit/{row.outfitType}";
                        if (!string.IsNullOrEmpty(row.accessories) && !string.Equals(row.accessories, "None", System.StringComparison.OrdinalIgnoreCase))
                            acc = $"Acc/{row.accessories.Split('|')[0]}"; // first accessory for now
                        if (WWIII.SideScroller.Data.CharacterAppearanceData.TryParseHexColor(row.skinTone, out var c)) tint = c;
                        if (row.bodyScale > 0) scl = row.bodyScale;
                    }
                }

                AddSkinIfPresent(combined, sdata, hair);
                AddSkinIfPresent(combined, sdata, outfit);
                AddSkinIfPresent(combined, sdata, acc);
                skeleton.SetSkin(combined);
                skeleton.SetSlotsToSetupPose();

                // Tint
                skeleton.SetColor(tint);
            }

            // Scale
            float finalScale = cfg.scale;
            if (useCSVOverrides && _csv != null)
            {
                var row = _csv.Find(r => r.age == age);
                if (row != null && row.bodyScale > 0) finalScale = row.bodyScale;
            }
            transform.localScale = Vector3.one * finalScale;

            Debug.Log("[SpineAppearanceController] Applied Spine age config.");
        }

        private static void AddSkinIfPresent(Skin combined, SkeletonData data, string categoryAndName)
        {
            if (string.IsNullOrEmpty(categoryAndName)) return;
            var skin = data.FindSkin(categoryAndName);
            if (skin != null) combined.AddSkin(skin);
            else Debug.LogWarning($"[SpineAppearanceController] Skin not found: {categoryAndName}");
        }

        private void OnDestroy()
        {
            foreach (var kv in _handles)
            {
                if (kv.Value.IsValid()) Addressables.Release(kv.Value);
            }
            _handles.Clear();
        }
#else
        // Fallback stub so the project compiles without spine-unity.
        public void ApplyAgeMovement(AgeProfile.MovementConfig config) { }
        public void OnAgeChanged(AgeProfile profile)
        {
            Debug.LogWarning("[SpineAppearanceController] spine-unity not installed or SPINE_UNITY define not set. Import spine-unity and add Scripting Define Symbol 'SPINE_UNITY'.");
        }
#endif
    }
}
