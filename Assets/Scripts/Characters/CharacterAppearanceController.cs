using System;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Characters
{
    /// <summary>
    /// Applies age-based appearance using Unity Sprite Library/Resolver.
    /// Keeps physics/controller separate and light.
    /// </summary>
    public class CharacterAppearanceController : MonoBehaviour, IAgeAwareCharacter
    {
        [Header("Config")]
        public AgeAppearanceSet appearanceSet;

        [Header("Sprite Library Root")] 
        public SpriteLibrary spriteLibrary; // optional; can be found on children

        [Header("Categories (SpriteResolvers)")]
        public SpriteResolver hairResolver;
        public SpriteResolver outfitResolver;
        public SpriteResolver accessoryResolver;

        [Header("Tint Targets")] 
        public SpriteRenderer skinRenderer; // assign main body/skin sprite
        public SpriteRenderer hairRenderer; // assign hair sprite

        [Header("Movement Bridge (Optional)")]
        public Age7Character age7Character;

        private void Reset()
        {
            spriteLibrary = GetComponentInChildren<SpriteLibrary>();
            FindResolvers();
            age7Character = GetComponent<Age7Character>();
        }

        private void Awake()
        {
            if (spriteLibrary == null) spriteLibrary = GetComponentInChildren<SpriteLibrary>();
            if (hairResolver == null || outfitResolver == null || accessoryResolver == null)
            {
                FindResolvers();
            }
            if (age7Character == null) age7Character = GetComponent<Age7Character>();
        }

        private void FindResolvers()
        {
            var resolvers = GetComponentsInChildren<SpriteResolver>(true);
            hairResolver = resolvers.FirstOrDefault(r => r.GetCategory() == "Hair");
            outfitResolver = resolvers.FirstOrDefault(r => r.GetCategory() == "Outfit");
            accessoryResolver = resolvers.FirstOrDefault(r => r.GetCategory() == "Accessories" || r.GetCategory() == "Accessory");
            if (hairResolver == null || outfitResolver == null)
            {
                Debug.LogWarning("[CharacterAppearanceController] Missing Hair/Outfit SpriteResolvers. Ensure children have categories 'Hair' and 'Outfit'.");
            }
        }

        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            if (age7Character != null)
            {
                age7Character.ApplyAgeMovement(config);
            }
        }

        public void OnAgeChanged(AgeProfile profile)
        {
            if (appearanceSet == null || profile == null) return;
            var entry = appearanceSet.GetForAge(profile.ageYears);
            if (entry == null) return;
            ApplyAppearance(entry);
            if (age7Character != null)
            {
                age7Character.OnAgeChanged(profile);
            }
            Debug.Log($"[CharacterAppearanceController] Applied age {profile.ageYears} appearance");
        }

        public void ApplyAppearance(AgeAppearanceSet.AgeAppearance entry)
        {
            if (entry == null) return;
            // Swap per-age library if provided
            if (spriteLibrary == null) spriteLibrary = GetComponentInChildren<SpriteLibrary>();
            if (entry.spriteLibraryAsset != null && spriteLibrary != null)
            {
                spriteLibrary.spriteLibraryAsset = entry.spriteLibraryAsset;
            }

            // Labels
            if (hairResolver != null && !string.IsNullOrEmpty(entry.hairLabel))
            {
                hairResolver.SetCategoryAndLabel("Hair", entry.hairLabel);
            }
            if (outfitResolver != null && !string.IsNullOrEmpty(entry.outfitLabel))
            {
                outfitResolver.SetCategoryAndLabel("Outfit", entry.outfitLabel);
            }
            if (accessoryResolver != null && !string.IsNullOrEmpty(entry.accessoryLabel))
            {
                accessoryResolver.SetCategoryAndLabel(accessoryResolver.GetCategory() ?? "Accessories", entry.accessoryLabel);
            }

            // Tints (via MaterialPropertyBlock to avoid instance materials)
            ApplyTint(skinRenderer, entry.skinTint);
            ApplyTint(hairRenderer, entry.hairTint);

            // Scale (applies to a designated child or the object itself)
            transform.localScale = entry.localScale;
        }

        private static void ApplyTint(SpriteRenderer sr, Color tint)
        {
            if (sr == null) return;
            var mpb = new MaterialPropertyBlock();
            sr.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", tint);
            sr.SetPropertyBlock(mpb);
        }
    }
}

