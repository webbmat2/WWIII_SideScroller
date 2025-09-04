using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Editor.Level;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Bio
{
    public static class GenerateBiographicalLevels
    {
        private class Entry { public int years; public string prompt; }

        [MenuItem("WWIII/Level/Generate 9 Biographical Levels")] 
        public static void GenerateAll()
        {
            var ageSet = AssetDatabase.LoadAssetAtPath<AgeSet>("Assets/WWIII/Ages/Jim_AgeSet.asset");
            var theme = AssetDatabase.LoadAssetAtPath<LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
            if (theme == null)
            {
                EditorUtility.DisplayDialog("WWIII", "Missing LevelThemeProfile at Assets/WWIII/Data/LevelThemeProfile.asset", "OK");
                return;
            }

            var entries = new List<Entry>
            {
                new Entry{years=7,prompt="Childhood playground: simple platforms, safe exploration, bright colors."},
                new Entry{years=11,prompt="School corridors and classrooms: learning challenges, social navigation."},
                new Entry{years=14,prompt="Teen neighborhood: complex choices, identity, branching routes."},
                new Entry{years=17,prompt="High school campus: responsibility, exams, future planning cues."},
                new Entry{years=21,prompt="College/career start: independence, cityscape, part-time job props."},
                new Entry{years=28,prompt="Career growth: offices, commute platforms, productivity hazards."},
                new Entry{years=45,prompt="Family life: home + park balance, support tasks."},
                new Entry{years=50,prompt="Reflection: mentoring others, calm pace, long vistas."},
                new Entry{years=80,prompt="Life retrospective: memory lane, curated highlights, photo reveals."},
            };

            foreach (var e in entries)
            {
                var w = EditorWindow.GetWindow<WWIII.SideScroller.Editor.Level.BiographicalLevelGeneratorWindow>();
                // call internal methods directly since window is just a facade
                GenerateSingle(ageSet, theme, e.years, e.prompt);
            }

            EditorUtility.DisplayDialog("WWIII", "Generated 9 biographical levels under Assets/WWIII/Scenes.", "OK");
        }

        private static void GenerateSingle(AgeSet set, LevelThemeProfile theme, int years, string prompt)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var maps = BiographicalTilemapBuilder.CreateGridWithTilemaps();
            var ageTheme = theme.GetForYears(years);
            BiographicalTilemapBuilder.PopulateGround(maps.ground, ageTheme.groundTiles.ToArray(), seed: AILevelLayoutSampler.SeedFromPrompt(prompt));

            // Age system
            var ageRoot = new GameObject("AgeSystem");
            var mgr = ageRoot.AddComponent<AgeManager>();
            mgr.ageSet = set;
            if (set != null) { var idx = set.ages.FindIndex(a => a != null && a.ageYears == years); mgr.initialAgeIndex = Mathf.Max(0, idx); }

            // Save prompt file
            System.IO.Directory.CreateDirectory("Assets/WWIII/Scenes");
            var promptPath = $"Assets/WWIII/Scenes/BioLevel_Age{years}_Prompt.txt";
            System.IO.File.WriteAllText(promptPath, prompt);
            AssetDatabase.ImportAsset(promptPath);

            // Place a few age-gated props
            var propsRoot = new GameObject("StoryProps");
            foreach (var p in ageTheme.propPrefabs)
            {
                if (p == null) continue;
                var go = PrefabUtility.InstantiatePrefab(p) as GameObject;
                if (go == null) continue;
                go.transform.position = new Vector3(Random.Range(6, 86), 10, 0);
                go.transform.SetParent(propsRoot.transform);
                var gate = go.GetComponent<AgeConditionalObject>() ?? go.AddComponent<AgeConditionalObject>();
                gate.useYears = true; gate.minYears = years; gate.maxYears = 999;
            }

            EditorSceneManager.SaveScene(scene, $"Assets/WWIII/Scenes/BioLevel_Age{years}.unity");
        }
    }
}
