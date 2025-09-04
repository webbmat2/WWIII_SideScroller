using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Disables conflicting Character Editor example MonoBehaviours when using the new Input System.
    /// </summary>
    public class CharacterEditorCompatibilityLayer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            DisableConflictingScripts();
        }

        private static void DisableConflictingScripts()
        {
            string[] typeNames = { "AttackingExample", "MovementExample", "TestRoom" };

            foreach (var simpleName in typeNames)
            {
                var type = FindTypeBySimpleName(simpleName);
                if (type == null) continue;

                var objects = Resources.FindObjectsOfTypeAll(type);
                foreach (var obj in objects)
                {
                    if (obj is Behaviour b)
                    {
                        b.enabled = false;
                    }
                }
            }

            Debug.Log("[CharacterEditorCompatibilityLayer] Disabled conflicting example scripts");
        }

        private static Type FindTypeBySimpleName(string simpleName)
        {
            // Try direct
            var direct = Type.GetType(simpleName);
            if (direct != null) return direct;

            // Search loaded assemblies
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetTypes().FirstOrDefault(tt => tt.Name == simpleName);
                    if (t != null) return t;
                }
                catch (ReflectionTypeLoadException rtl)
                {
                    var t = rtl.Types?.FirstOrDefault(tt => tt != null && tt.Name == simpleName);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }
    }
}

