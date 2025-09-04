using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Integration.Corgi;

namespace WWIII.SideScroller.Editor.Integration.Corgi
{
    public static class CorgiAdapterSetup
    {
        [MenuItem("WWIII/Corgi/Attach Age Ability Binder to Selected")]
        public static void Attach()
        {
            foreach (var obj in Selection.gameObjects)
            {
                var go = obj;
                var binder = go.GetComponent<AgeCorgiAbilityBinder>() ?? go.AddComponent<AgeCorgiAbilityBinder>();
                // Optional explicit wiring; binder also auto-discovers on Awake/Reset
                if (binder.ageManager == null) binder.ageManager = UnityEngine.Object.FindFirstObjectByType<WWIII.SideScroller.Aging.AgeManager>();
                EditorUtility.SetDirty(go);
            }
            AssetDatabase.SaveAssets();
        }

        // No longer needed to use name-based reflection-based matching; binder is type-safe
    }
}
