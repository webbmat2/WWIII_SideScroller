using System;
using System.IO;
using UnityEditor;
using UnityEngine;
#if ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
using UnityEngine.Playables;
using UnityEngine.U2D;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class AddressablesConfigurator
    {
#if ADDRESSABLES
        public static AddressableAssetEntry EnsureAddressable(Object asset, string groupName, string label)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressables settings not found. Create Addressables settings first.");
                return null;
            }

            var group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, true, new System.Collections.Generic.List<AddressableAssetGroupSchema>
            {
                settings.DefaultGroup.Schemas[0]
            }, typeof(BundledAssetGroupSchema));

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var entry = settings.CreateOrMoveEntry(guid, group);
            if (!string.IsNullOrEmpty(label))
            {
                settings.AddLabel(label);
                entry.SetLabel(label, true, true);
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
            return entry;
        }
#endif

        public static void AssignProfileReferences(AgeProfile profile, SpriteAtlas atlas, RuntimeAnimatorController controller, PlayableAsset cutscene, string audioLabel)
        {
#if ADDRESSABLES
            if (atlas != null)
            {
                var aEntry = EnsureAddressable(atlas, $"Ages/Jim_{profile.ageYears}", $"Age_{profile.ageYears}");
                if (aEntry != null)
                {
                    profile.spriteAtlas = new UnityEngine.AddressableAssets.AssetReferenceT<SpriteAtlas>(aEntry.guid);
                }
            }
            if (controller != null)
            {
                var cEntry = EnsureAddressable(controller, $"Ages/Jim_{profile.ageYears}", $"Age_{profile.ageYears}");
                if (cEntry != null)
                {
                    profile.animatorController = new UnityEngine.AddressableAssets.AssetReferenceT<RuntimeAnimatorController>(cEntry.guid);
                }
            }
            if (cutscene != null)
            {
                var tEntry = EnsureAddressable(cutscene, $"Ages/Jim_{profile.ageYears}", $"Age_{profile.ageYears}");
                if (tEntry != null)
                {
                    profile.transitionCutscene = new UnityEngine.AddressableAssets.AssetReferenceT<PlayableAsset>(tEntry.guid);
                }
            }
            if (!string.IsNullOrEmpty(audioLabel))
            {
                // Assign label reference
                var so = new SerializedObject(profile);
                so.Update();
                var labelProp = so.FindProperty("audioLabel");
                if (labelProp != null)
                {
                    var labelStringProp = labelProp.FindPropertyRelative("m_LabelString");
                    if (labelStringProp != null) labelStringProp.stringValue = audioLabel;
                }
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
#else
            Debug.LogWarning("Addressables not enabled (define ADDRESSABLES). Skipping asset assignment.");
#endif
        }
    }
}

