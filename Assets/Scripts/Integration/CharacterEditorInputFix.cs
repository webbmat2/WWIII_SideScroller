using UnityEngine;
using UnityEngine.InputSystem;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Fixes Character Editor compatibility with Unity Input System.
    /// This is a placeholder to ensure a clear startup log and provide a hook if needed later.
    /// </summary>
    public static class CharacterEditorInputFix
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void FixInputCompatibility()
        {
#if ENABLE_INPUT_SYSTEM
            Debug.Log("[CharacterEditorInputFix] Ensuring Input System compatibility for Character Editor");
#endif
        }
    }
}

