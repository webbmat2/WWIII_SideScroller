using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Editor.Level;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Level
{
    public class BiographicalLevelGeneratorWindow : EditorWindow
    {
        private AgeSet _ageSet;
        private LevelThemeProfile _theme;
        private int _ageYears = 7;
        private int _seed = 12345;
        private string _prompt = "Side-scrolling level for Jim at age 7: safe schoolyard platforms, low height, bright colors, simple hazards.";

        [MenuItem("WWIII/Level/Generate Biographical Level")] 
        public static void Open()
        {
            var w = GetWindow<BiographicalLevelGeneratorWindow>(true, "Biographical Level Generator");
            w.minSize = new Vector2(420, 240);
        }

        private void OnGUI()
        {
            _ageSet = (AgeSet)EditorGUILayout.ObjectField("Age Set", _ageSet, typeof(AgeSet), false);
            _theme = (LevelThemeProfile)EditorGUILayout.ObjectField("Theme Profile", _theme, typeof(LevelThemeProfile), false);
            _ageYears = EditorGUILayout.IntSlider("Target Age (years)", _ageYears, 7, 99);
            _prompt = EditorGUILayout.TextField("AI Prompt", _prompt);
            if (GUILayout.Button("Seed From Prompt"))
            {
                _seed = AILevelLayoutSampler.SeedFromPrompt(_prompt);
            }
            _seed = EditorGUILayout.IntField("Seed", _seed);

            if (GUILayout.Button("Generate New Level Scene"))
            {
                Generate();
            }
        }

        private void Generate()
        {
            if (_theme == null)
            {
                EditorUtility.DisplayDialog("WWIII", "Assign a LevelThemeProfile.", "OK");
                return;
            }
            var ageTheme = _theme.GetForYears(_ageYears);
            if (ageTheme == null)
            {
                EditorUtility.DisplayDialog("WWIII", "No matching age theme in profile.", "OK");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

            var maps = BiographicalTilemapBuilder.CreateGridWithTilemaps();
            BiographicalTilemapBuilder.PopulateGround(maps.ground, ageTheme.groundTiles.ToArray(), seed: _seed);

            // Age system root
            var ageRoot = new GameObject("AgeSystem");
            var mgr = ageRoot.AddComponent<AgeManager>();
            if (_ageSet == null) _ageSet = AssetDatabase.LoadAssetAtPath<AgeSet>("Assets/WWIII/Ages/Jim_AgeSet.asset");
            mgr.ageSet = _ageSet;
            mgr.initialAgeIndex = Mathf.Max(0, _ageSet != null ? _ageSet.ages.FindIndex(a => a != null && a.ageYears == _ageYears) : 0);

            // Place storytelling props
            var propsRoot = new GameObject("StoryProps");
            foreach (var p in ageTheme.propPrefabs)
            {
                if (p == null) continue;
                var go = PrefabUtility.InstantiatePrefab(p) as GameObject;
                if (go == null) continue;
                go.transform.position = new Vector3(Random.Range(4, 80), 12, 0);
                go.transform.SetParent(propsRoot.transform);
                // Add age gating helper to enable after this age or within range
                var gate = go.GetComponent<WWIII.SideScroller.Level.AgeConditionalObject>() ?? go.AddComponent<WWIII.SideScroller.Level.AgeConditionalObject>();
                gate.useYears = true; gate.minYears = ageTheme.ageYears; gate.maxYears = 999;
            }

            // Save prompt alongside the scene for provenance
            System.IO.Directory.CreateDirectory("Assets/WWIII/Scenes");
            var promptPath = $"Assets/WWIII/Scenes/BioLevel_Age{_ageYears}_Prompt.txt";
            System.IO.File.WriteAllText(promptPath, _prompt);
            AssetDatabase.ImportAsset(promptPath);

            EditorSceneManager.SaveScene(scene, $"Assets/WWIII/Scenes/BioLevel_Age{_ageYears}.unity");
            EditorUtility.DisplayDialog("WWIII", "Biographical level generated.", "OK");
        }
    }
}
