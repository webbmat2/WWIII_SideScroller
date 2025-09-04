using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Editor.Aging
{
    public static class AgeAssetGenerator
    {
        private const string RootFolder = "Assets/WWIII/Ages";
        private const string ProfilesFolder = RootFolder + "/Profiles";

        [MenuItem("WWIII/Create/Ages (Sample 7→Adult)")]
        public static void CreateSampleAges()
        {
            EnsureFolder(RootFolder);
            EnsureFolder(ProfilesFolder);

            var createdProfiles = new List<AgeProfile>();

            createdProfiles.Add(CreateAgeProfile(
                fileName: "Jim_Age07_Child.asset",
                displayName: "Child",
                ageYears: 7,
                playerLayerName: "PlayerChild",
                movement: new AgeProfile.MovementConfig
                {
                    maxRunSpeed = 4.2f,
                    acceleration = 45f,
                    deceleration = 55f,
                    jumpForce = 7.2f,
                    gravityScale = 3.2f
                },
                yarnNode: "Age_07_Intro"
            ));

            createdProfiles.Add(CreateAgeProfile(
                fileName: "Jim_Age11_Preteen.asset",
                displayName: "Preteen",
                ageYears: 11,
                playerLayerName: "PlayerPreteen",
                movement: new AgeProfile.MovementConfig
                {
                    maxRunSpeed = 4.8f,
                    acceleration = 50f,
                    deceleration = 58f,
                    jumpForce = 7.6f,
                    gravityScale = 3.0f
                },
                yarnNode: "Age_11_Intro"
            ));

            createdProfiles.Add(CreateAgeProfile(
                fileName: "Jim_Age14_Teen.asset",
                displayName: "Teen",
                ageYears: 14,
                playerLayerName: "PlayerTeen",
                movement: new AgeProfile.MovementConfig
                {
                    maxRunSpeed = 5.6f,
                    acceleration = 58f,
                    deceleration = 62f,
                    jumpForce = 8.2f,
                    gravityScale = 2.9f
                },
                yarnNode: "Age_14_Intro"
            ));

            createdProfiles.Add(CreateAgeProfile(
                fileName: "Jim_Age17_YoungAdult.asset",
                displayName: "Young Adult",
                ageYears: 17,
                playerLayerName: "PlayerYoungAdult",
                movement: new AgeProfile.MovementConfig
                {
                    maxRunSpeed = 6.0f,
                    acceleration = 60f,
                    deceleration = 64f,
                    jumpForce = 8.6f,
                    gravityScale = 2.8f
                },
                yarnNode: "Age_17_Intro"
            ));

            createdProfiles.Add(CreateAgeProfile(
                fileName: "Jim_Age21_Adult.asset",
                displayName: "Adult",
                ageYears: 21,
                playerLayerName: "PlayerAdult",
                movement: new AgeProfile.MovementConfig
                {
                    maxRunSpeed = 6.2f,
                    acceleration = 60f,
                    deceleration = 65f,
                    jumpForce = 8.6f,
                    gravityScale = 2.8f
                },
                yarnNode: "Age_21_Intro"
            ));

            // Create AgeSet
            var ageSet = ScriptableObject.CreateInstance<AgeSet>();
            ageSet.ages = createdProfiles;
            var ageSetPath = Path.Combine(RootFolder, "Jim_AgeSet.asset");
            AssetDatabase.CreateAsset(ageSet, ageSetPath);

            // Save and select
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(ageSetPath));
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(ageSetPath);

            Debug.Log("WWIII: Created sample AgeProfiles and AgeSet at Assets/WWIII/Ages");
        }

        private static AgeProfile CreateAgeProfile(string fileName, string displayName, int ageYears, string playerLayerName, AgeProfile.MovementConfig movement, string yarnNode)
        {
            var asset = ScriptableObject.CreateInstance<AgeProfile>();
            asset.displayName = displayName;
            asset.ageYears = ageYears;
            asset.playerLayerName = playerLayerName;
            asset.movement = movement;
            asset.yarnStartNode = yarnNode;

            // default ability flags by age bracket
            var abilities = asset.abilities;
            if (ageYears <= 10)
            {
                abilities.canCrouch = true;
                abilities.canDash = false;
                abilities.canWallCling = false;
                abilities.canShoot = false;
                abilities.maxNumberOfJumps = 1;
            }
            else if (ageYears <= 16)
            {
                abilities.canCrouch = true;
                abilities.canDash = false;
                abilities.canWallCling = true;
                abilities.canShoot = false;
                abilities.maxNumberOfJumps = 2;
            }
            else
            {
                abilities.canCrouch = true;
                abilities.canDash = true;
                abilities.canWallCling = true;
                abilities.canShoot = true;
                abilities.maxNumberOfJumps = 2;
            }
            asset.abilities = abilities;

            var path = Path.Combine(ProfilesFolder, fileName);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
