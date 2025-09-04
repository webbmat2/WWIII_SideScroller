using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.U2D;

namespace WWIII.SideScroller.Aging
{
    [CreateAssetMenu(menuName = "WWIII/Aging/Age Profile", fileName = "AgeProfile")]
    public class AgeProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Human-readable name, e.g., 'Child', 'Teen', 'Adult'.")]
        public string displayName = "";

        [Tooltip("Chronological age in years (e.g., 7, 12, 18). Used for UI/Yarn.")]
        public int ageYears = 7;

        [Tooltip("Index within the AgeSet. Set automatically by editor tool or manually.")]
        public int index = 0;

        [Header("Physics & Layering")]
        [Tooltip("Layer name this age uses (maps into your 19-layer physics system). Example: 'PlayerChild', 'PlayerTeen'.")]
        public string playerLayerName = "Player";

        [Tooltip("Optional: Override physics layer index explicitly if you don't use names.")]
        public int overrideLayerIndex = -1;

        [Header("Movement Config")]
        public MovementConfig movement = new MovementConfig
        {
            maxRunSpeed = 5f,
            acceleration = 50f,
            deceleration = 60f,
            jumpForce = 8f,
            gravityScale = 3f
        };

        [Serializable]
        public struct MovementConfig
        {
            public float maxRunSpeed;
            public float acceleration;
            public float deceleration;
            public float jumpForce;
            public float gravityScale;
        }

        [Header("Abilities (Corgi)")]
        [Tooltip("Per-age ability unlocks. Used by the Corgi integration binder to enable/disable abilities.")]
        public AbilityConfig abilities = new AbilityConfig
        {
            canCrouch = true,
            canDash = false,
            canWallCling = false,
            canShoot = false,
            maxNumberOfJumps = 1
        };

        [Serializable]
        public struct AbilityConfig
        {
            public bool canCrouch;
            public bool canDash;
            public bool canWallCling;
            public bool canShoot;
            [Tooltip("Total jumps allowed (1 = single jump, 2 = double jump, ...)")]
            public int maxNumberOfJumps;
        }

        [Header("Assets (Addressables)")]
        [Tooltip("Optional: Player prefab for this age. If assigned, AgeManager will swap the player instance.")]
        public AssetReferenceGameObject playerPrefab;

        [Tooltip("Optional: Animator Controller for this age. Used if prefab is not supplied.")]
        public AssetReferenceT<RuntimeAnimatorController> animatorController;

        [Tooltip("Sprite Atlas for this age's visuals (URP 2D optimized atlases recommended).")]
        public AssetReferenceT<SpriteAtlas> spriteAtlas;

        [Tooltip("Optional: Label used to load audio (SFX/VO) for this age as a group.")]
        public AssetLabelReference audioLabel;

        [Header("Cutscene & Dialogue")]
        [Tooltip("Timeline PlayableAsset that plays when transitioning INTO this age.")]
        public AssetReferenceT<PlayableAsset> transitionCutscene;

        [Tooltip("Optional Yarn node to start on entering this age.")]
        public string yarnStartNode;

        public int ResolveLayer()
        {
            if (overrideLayerIndex >= 0 && overrideLayerIndex <= 31)
                return overrideLayerIndex;

            if (!string.IsNullOrEmpty(playerLayerName))
            {
                var idx = LayerMask.NameToLayer(playerLayerName);
                if (idx >= 0)
                    return idx;
            }

            // Fallback to default Player (commonly 8) or 0 if not found
            var fallback = LayerMask.NameToLayer("Player");
            return fallback >= 0 ? fallback : 0;
        }
    }
}
