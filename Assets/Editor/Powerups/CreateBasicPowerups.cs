using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Powerups;

namespace WWIII.SideScroller.Editor.Powerups
{
    public static class CreateBasicPowerups
    {
        [MenuItem("WWIII/Diagnostics/Seed/Create Basic Powerups")] 
        public static void Create()
        {
            Directory.CreateDirectory("Assets/WWIII/Powerups");
            Make("RunningShoes", d=>{d.speedMultiplier=1.4f; d.duration=12f;});
            Make("Cheeseball", d=>{d.invulnerable=true; d.duration=8f;});
            Make("BeefJerky", d=>{d.healAmount=25f; d.duration=0f;});
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII","Basic powerups created in Assets/WWIII/Powerups","OK");
        }

        private static void Make(string name, System.Action<PowerUpDefinition> edit)
        {
            var path = $"Assets/WWIII/Powerups/{name}.asset";
            var d = ScriptableObject.CreateInstance<PowerUpDefinition>();
            d.displayName = name;
            edit?.Invoke(d);
            AssetDatabase.CreateAsset(d, path);
        }

        [MenuItem("WWIII/Diagnostics/Seed/Create Basic Powerups", true)]
        private static bool ValidateCreate()
        {
            return WWIII.SideScroller.Editor.Diagnostics.DiagnosticsMenu.IsEnabled;
        }
    }
}
