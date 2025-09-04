using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.U2D;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public class JimArtPipelineWindow : EditorWindow
    {
        private AIGeneratorConfig _config;
        private AgeSet _ageSet;

        [MenuItem("WWIII/Art Pipeline/Jim Pipeline Window")] 
        public static void Open()
        {
            var wnd = GetWindow<JimArtPipelineWindow>(true, "Jim Art Pipeline");
            wnd.minSize = new Vector2(420, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("Pipeline Config", EditorStyles.boldLabel);
            _config = (AIGeneratorConfig)EditorGUILayout.ObjectField("AI Config", _config, typeof(AIGeneratorConfig), false);
            _ageSet = (AgeSet)EditorGUILayout.ObjectField("Age Set", _ageSet, typeof(AgeSet), false);

            if (GUILayout.Button("Run Full Pipeline (Generate → Import → Atlas → Controller → Addressables → Assign)"))
            {
                RunFullPipeline();
            }
        }

        private void RunFullPipeline()
        {
            if (_config == null)
            {
                _config = CreateDefaultConfig();
            }
            if (_ageSet == null) _ageSet = FindAgeSetOrCreate();

            SortingLayerUtility.EnsureSortingLayers();

            foreach (var age in _config.ages)
            {
                // 1) AI Generation or placeholders
                string outFolder = Path.Combine(_config.outputRoot, $"Age_{age.ageYears}").Replace('\\','/');
                var gen = AIGeneratorsBridge.GenerateAgeSprites(_config, age, outFolder);

                // 2) Importer settings
                SpriteImportUtility.ConfigureFolderForSprites(outFolder);

                // 3) Build atlas
                string atlasPath = Path.Combine(_config.atlasRoot, $"Jim_Age{age.ageYears}_Atlas.spriteatlas").Replace('\\','/');
                var atlas = AtlasBuilder.CreateOrUpdateAtlas(atlasPath, outFolder);

                // 4) Create animation clips + controller
                var clips = AnimationRigSetup.CreateFrameAnimationClips(outFolder, age.ageYears);
                string controllerPath = Path.Combine(outFolder, $"Jim_{age.ageYears}_Controller.controller").Replace('\\','/');
                var controller = AnimationRigSetup.CreateAnimatorController(age.ageYears, clips, controllerPath);

                // 5) Create timeline per age for transitions (optional visual track later)
                string timelinePath = Path.Combine(outFolder, $"Jim_{age.ageYears}_Transition.playable" ).Replace('\\','/');
                var timeline = TimelineSetup.CreateAgeTransitionTimeline(IndexOfAge(_ageSet, age.ageYears), timelinePath);

                // 6) Addressables assignment + AgeProfile population
                var profile = GetOrCreateAgeProfile(_ageSet, age.ageYears);
                profile.displayName = age.label;
                profile.ageYears = age.ageYears;
                profile.playerLayerName = GuessLayerName(age.ageYears);
                profile.yarnStartNode = $"Age_{age.ageYears}_Intro";
                AddressablesConfigurator.AssignProfileReferences(profile, atlas, controller, timeline, $"Age_{age.ageYears}_AUDIO");
            }

            EditorUtility.DisplayDialog("WWIII", "Jim Art Pipeline completed.", "OK");
        }

        private static AIGeneratorConfig CreateDefaultConfig()
        {
            var cfg = ScriptableObject.CreateInstance<AIGeneratorConfig>();
            var path = "Assets/WWIII/Art/Jim/AIGeneratorConfig.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "Assets");
            AssetDatabase.CreateAsset(cfg, path);
            AssetDatabase.SaveAssets();
            return cfg;
        }

        private static AgeSet FindAgeSetOrCreate()
        {
            var set = AssetDatabase.LoadAssetAtPath<AgeSet>("Assets/WWIII/Ages/Jim_AgeSet.asset");
            if (set != null) return set;
            set = ScriptableObject.CreateInstance<AgeSet>();
            Directory.CreateDirectory("Assets/WWIII/Ages");
            AssetDatabase.CreateAsset(set, "Assets/WWIII/Ages/Jim_AgeSet.asset");
            AssetDatabase.SaveAssets();
            return set;
        }

        private static AgeProfile GetOrCreateAgeProfile(AgeSet set, int ageYears)
        {
            var p = set.ages.Find(x => x != null && x.ageYears == ageYears);
            if (p != null) return p;
            var profile = ScriptableObject.CreateInstance<AgeProfile>();
            var path = $"Assets/WWIII/Ages/Profiles/Jim_Age{ageYears}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "Assets");
            AssetDatabase.CreateAsset(profile, path);
            set.ages.Add(profile);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static string GuessLayerName(int ageYears)
        {
            if (ageYears <= 8) return "PlayerChild";
            if (ageYears <= 12) return "PlayerPreteen";
            if (ageYears <= 15) return "PlayerTeen";
            if (ageYears <= 18) return "PlayerYoungAdult";
            return "PlayerAdult";
        }

        private static int IndexOfAge(AgeSet set, int ageYears)
        {
            for (int i = 0; i < set.ages.Count; i++)
            {
                var p = set.ages[i];
                if (p != null && p.ageYears == ageYears) return i;
            }
            return Mathf.Clamp(set.ages.Count - 1, 0, int.MaxValue);
        }
    }
}
