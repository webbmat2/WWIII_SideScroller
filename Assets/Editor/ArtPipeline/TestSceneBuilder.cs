using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Integration;
using WWIII.SideScroller.Player;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class TestSceneBuilder
    {
        [MenuItem("WWIII/Art Pipeline/Create Demo Scene (Jim Ages)")]
        public static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "Jim_Ages_Demo";

            // Camera setup
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5f;
            }

            // AgeManager root
            var root = new GameObject("AgeSystem");
            var ageMgr = root.AddComponent<AgeManager>();

            // Player placeholder
            var player = new GameObject("Jim");
            player.layer = LayerMask.NameToLayer("Characters");
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Characters";
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<PlayerAgeAdapter>();
            player.AddComponent<JimAgeController>();
            ageMgr.currentPlayer = player;
            ageMgr.playerRoot = player.transform.parent;

            // Yarn bridge + demo
            var yarnGo = new GameObject("YarnBridge");
            var yarnBridge = yarnGo.AddComponent<YarnAgeBridge>();
            yarnBridge.ageManager = ageMgr;

            // Optional demo driver if present (not required in production)
            var demoType = System.Type.GetType("WWIII.SideScroller.Tests.AgeManagerYarnDemo, Assembly-CSharp");
            if (demoType != null)
            {
                var demo = new GameObject("AgeDemo");
                var comp = demo.AddComponent(demoType) as Component;
                var t = demoType;
                try
                {
                    var f = t.GetField("ageManager");
                    if (f != null) f.SetValue(comp, ageMgr);
                    else
                    {
                        var p = t.GetProperty("ageManager");
                        if (p != null && p.CanWrite) p.SetValue(comp, ageMgr);
                    }
                }
                catch { }
                try
                {
                    var f = t.GetField("yarnBridge");
                    if (f != null) f.SetValue(comp, yarnBridge);
                    else
                    {
                        var p = t.GetProperty("yarnBridge");
                        if (p != null && p.CanWrite) p.SetValue(comp, yarnBridge);
                    }
                }
                catch { }
            }

            // Find or create AgeSet
            var set = AssetDatabase.LoadAssetAtPath<AgeSet>("Assets/WWIII/Ages/Jim_AgeSet.asset");
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<AgeSet>();
                Directory.CreateDirectory("Assets/WWIII/Ages");
                AssetDatabase.CreateAsset(set, "Assets/WWIII/Ages/Jim_AgeSet.asset");
                AssetDatabase.SaveAssets();
            }
            ageMgr.ageSet = set;

            EditorSceneManager.SaveScene(scene, "Assets/WWIII/Scenes/Jim_Ages_Demo.unity");
            EditorUtility.DisplayDialog("WWIII", "Demo scene created at Assets/WWIII/Scenes/Jim_Ages_Demo.unity", "OK");
        }
    }
}
