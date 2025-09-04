using System;
using System.Reflection;
using UnityEngine;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Cleans up Character Editor missing script references
    /// </summary>
    public class CharacterEditorCleanup : MonoBehaviour
    {
        [ContextMenu("Clean Missing Firearm References")]
        public void CleanMissingReferences()
        {
            // Disable FirearmCollection auto-initialization to stop spam
            var firearmCollectionType = Type.GetType("Assets.HeroEditor.Common.Scripts.Collections.FirearmCollection");
            if (firearmCollectionType != null)
            {
                var flagField = firearmCollectionType.GetField("SkipAutoInitialization", BindingFlags.Public | BindingFlags.Static);
                if (flagField != null)
                {
                    flagField.SetValue(null, true);
                    Debug.Log("[CharacterEditorCleanup] Preventing FirearmCollection auto-initialization");
                }
                else
                {
                    // Fallback: reflect Initialize method existence (informational only)
                    var initializeMethod = firearmCollectionType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static);
                    if (initializeMethod != null)
                    {
                        Debug.Log("[CharacterEditorCleanup] FirearmCollection.Initialize found (no skip flag). Consider vendor guard.");
                    }
                }
            }
        }
        
        private void Awake()
        {
            // Automatically clean on startup
            CleanMissingReferences();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreemptiveDisable()
        {
            try
            {
                var firearmCollectionType = Type.GetType("Assets.HeroEditor.Common.Scripts.Collections.FirearmCollection");
                var flagField = firearmCollectionType?.GetField("SkipAutoInitialization", BindingFlags.Public | BindingFlags.Static);
                if (flagField != null)
                {
                    flagField.SetValue(null, true);
                }
            }
            catch { /* best effort */ }
        }
    }
}

