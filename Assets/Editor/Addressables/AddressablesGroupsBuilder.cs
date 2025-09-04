using System.Collections.Generic;
using UnityEditor;
#if UNITY_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace WWIII.SideScroller.Editor.Addressables
{
    public static class AddressablesGroupsBuilder
    {
        private static readonly int[] Ages = { 7, 11, 14, 17, 21, 28, 42, 56, 80 };

        [MenuItem("WWIII/Addressables/Create Age Groups + Labels")]
        public static void CreateAgeGroups()
        {
#if UNITY_ADDRESSABLES
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("WWIII", "Addressables settings not found (Window > Asset Management > Addressables).", "OK");
                return;
            }
            foreach (var age in Ages)
            {
                var groupName = $"Age_{age}";
                var label = groupName;
                var group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, true, null);
                settings.AddLabel(label);
            }
            // Common PGU
            settings.FindGroup("PGU_Common") ?? settings.CreateGroup("PGU_Common", false, false, true, null);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", "Addressables Age groups and PGU_Common created.", "OK");
#else
            EditorUtility.DisplayDialog("WWIII", "UNITY_ADDRESSABLES define not set. Enable Addressables to use this.", "OK");
#endif
        }
    }
}

