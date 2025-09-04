// Tools to create and populate AgeAppearanceSet assets for Sprite Library workflow
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;
using WWIII.SideScroller.Characters;

namespace WWIII.SideScroller.EditorTools
{
    public static class AgeAppearanceTools
    {
        [MenuItem("WWIII/Characters/Create Default AgeAppearanceSet")]
        public static void CreateDefaultSet()
        {
            var set = ScriptableObject.CreateInstance<AgeAppearanceSet>();
            set.entries = new AgeAppearanceSet.AgeAppearance[]
            {
                new AgeAppearanceSet.AgeAppearance{ ageYears=7,  hairLabel="ChildHair",     outfitLabel="ChildOutfit",      accessoryLabel="None",          localScale=new Vector3(0.7f,0.7f,1f), notes="Innocent Childhood"},
                new AgeAppearanceSet.AgeAppearance{ ageYears=13, hairLabel="TeenHair",      outfitLabel="SchoolOutfit",     accessoryLabel="Backpack",      localScale=new Vector3(0.85f,0.85f,1f), notes="School Years"},
                new AgeAppearanceSet.AgeAppearance{ ageYears=18, hairLabel="YoungAdultHair", outfitLabel="CasualYoungAdult", accessoryLabel="Watch",         localScale=Vector3.one, notes="Young Adult"},
                new AgeAppearanceSet.AgeAppearance{ ageYears=25, hairLabel="ProfessionalHair", outfitLabel="ProfessionalAttire", accessoryLabel="WeddingRing", localScale=Vector3.one, notes="Professional"},
                new AgeAppearanceSet.AgeAppearance{ ageYears=35, hairLabel="ProfessionalHair", outfitLabel="ProfessionalAttire", accessoryLabel="Watch",       localScale=new Vector3(1.02f,1.02f,1f), notes="Maturity"},
                new AgeAppearanceSet.AgeAppearance{ ageYears=50, hairLabel="MatureHair",    outfitLabel="MatureStyle",      accessoryLabel="Glasses",       localScale=new Vector3(1.05f,1.02f,1f), notes="Reflection"}
            };

            var path = EditorUtility.SaveFilePanelInProject("Create AgeAppearanceSet", "AgeAppearanceSet", "asset", "Choose save location");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(set, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = set;
                Debug.Log("[AgeAppearanceTools] Created AgeAppearanceSet with default stages");
            }
        }

        [MenuItem("WWIII/Characters/Assign Set To Selected Character(s)")]
        public static void AssignSetToSelected()
        {
            var set = Selection.activeObject as AgeAppearanceSet;
            if (set == null)
            {
                Debug.LogWarning("[AgeAppearanceTools] Select an AgeAppearanceSet asset first.");
                return;
            }
            foreach (var go in Selection.gameObjects)
            {
                var c = go.GetComponentInChildren<CharacterAppearanceController>(true);
                if (c != null)
                {
                    Undo.RecordObject(c, "Assign AgeAppearanceSet");
                    c.appearanceSet = set;
                    EditorUtility.SetDirty(c);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[AgeAppearanceTools] Assigned set to selected characters");
        }
    }
}
#endif

