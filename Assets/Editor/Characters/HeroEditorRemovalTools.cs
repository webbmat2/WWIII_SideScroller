// Removes HeroEditor components from the Player to eliminate conflicts.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.EditorTools
{
    public static class HeroEditorRemovalTools
    {
        [MenuItem("WWIII/Characters/Remove HeroEditor Components From Player")] 
        public static void RemoveFromPlayer()
        {
            var player = GameObject.Find("AgeSystem/Player") ?? GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("[HeroEditorRemoval] Player not found. Ensure path is /AgeSystem/Player or named 'Player'.");
                return;
            }
            int removed = 0;
            var components = player.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var c in components)
            {
                if (c == null) continue;
                var tn = c.GetType().FullName;
                if (tn.StartsWith("Assets.HeroEditor.") || tn.Contains("CharacterBuilder") || tn.Contains("LayerManager") || tn.Contains("Equipment"))
                {
                    Undo.DestroyObjectImmediate(c);
                    removed++;
                }
            }
            Debug.Log($"[HeroEditorRemoval] Removed {removed} HeroEditor-related components from Player");
        }
    }
}
#endif

