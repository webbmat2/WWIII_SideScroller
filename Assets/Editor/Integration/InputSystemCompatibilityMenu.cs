using System;
using System.Reflection;
using UnityEditor;

namespace WWIII.SideScroller.Editor.Integration
{
    public static class InputSystemCompatibilityMenu
    {
        [MenuItem("WWIII/Character Editor/Set Active Input Handling: Both")] 
        public static void SetActiveInputHandlingBoth()
        {
            try
            {
                var psType = typeof(PlayerSettings);
                var enumType = psType.GetNestedType("ActiveInputHandler", BindingFlags.Public | BindingFlags.NonPublic);
                object bothValue = null;
                if (enumType != null)
                {
                    bothValue = Enum.Parse(enumType, "Both");
                }

                // Try property first
                var prop = psType.GetProperty("activeInputHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (prop != null && bothValue != null)
                {
                    prop.SetValue(null, bothValue, null);
                    UnityEngine.Debug.Log("[InputSystemCompatibility] Set Active Input Handling to Both via property");
                    return;
                }

                // Fallback to internal SetPropertyInt
                var setPropInt = psType.GetMethod("SetPropertyInt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (setPropInt != null)
                {
                    int bothInt = bothValue != null ? (int)Convert.ChangeType(bothValue, typeof(int)) : 2; // 2 usually maps to Both
                    setPropInt.Invoke(null, new object[] { "activeInputHandler", bothInt, string.Empty });
                    UnityEngine.Debug.Log("[InputSystemCompatibility] Set Active Input Handling to Both via SetPropertyInt");
                    return;
                }

                UnityEngine.Debug.LogWarning("[InputSystemCompatibility] Could not set Active Input Handling via API. Please set it manually: Project Settings > Player > Other Settings > Active Input Handling = Both.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[InputSystemCompatibility] Failed to set Active Input Handling: {e.Message}. Please set manually: Project Settings > Player > Other Settings > Active Input Handling = Both.");
            }
        }
    }
}

