// Sets up CharacterAppearanceController on Player and ensures SpriteLibrary/Resolvers are present
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;
using WWIII.SideScroller.Characters;

namespace WWIII.SideScroller.EditorTools
{
    public static class SetupSpriteLibraryCharacter
    {
        [MenuItem("WWIII/Characters/Setup SpriteLibrary Character On Player")] 
        public static void Setup()
        {
            var player = GameObject.Find("AgeSystem/Player") ?? GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("[SetupSpriteLibraryCharacter] Player not found. Ensure path is /AgeSystem/Player or named 'Player'.");
                return;
            }

            var controller = player.GetComponent<CharacterAppearanceController>();
            if (controller == null) controller = Undo.AddComponent<CharacterAppearanceController>(player);

            // SpriteLibrary
            var library = player.GetComponentInChildren<SpriteLibrary>();
            if (library == null)
            {
                var libGo = new GameObject("SpriteLibraryRoot");
                Undo.RegisterCreatedObjectUndo(libGo, "Create SpriteLibraryRoot");
                libGo.transform.SetParent(player.transform, false);
                library = Undo.AddComponent<SpriteLibrary>(libGo);
            }
            controller.spriteLibrary = library;

            // Ensure resolvers exist
            EnsureResolver(player.transform, "Hair", ref controller.hairResolver);
            EnsureResolver(player.transform, "Outfit", ref controller.outfitResolver);
            EnsureResolver(player.transform, "Accessories", ref controller.accessoryResolver);

            EditorUtility.SetDirty(controller);
            Debug.Log("[SetupSpriteLibraryCharacter] CharacterAppearanceController setup complete on Player. Assign an AgeAppearanceSet and SpriteLibraryAsset per age.");
        }

        private static void EnsureResolver(Transform parent, string category, ref SpriteResolver resolver)
        {
            if (resolver != null) return;
            // Try find existing by category
            foreach (var r in parent.GetComponentsInChildren<SpriteResolver>(true))
            {
                if (r.GetCategory() == category)
                {
                    resolver = r; return;
                }
            }
            // Create placeholder
            var go = new GameObject(category);
            Undo.RegisterCreatedObjectUndo(go, "Create SpriteResolver");
            go.transform.SetParent(parent, false);
            var sr = Undo.AddComponent<SpriteRenderer>(go);
            resolver = Undo.AddComponent<SpriteResolver>(go);
            resolver.SetCategoryAndLabel(category, string.Empty);
        }
    }
}
#endif

