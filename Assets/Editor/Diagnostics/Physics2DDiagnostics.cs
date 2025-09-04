#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WWIII.SideScroller.EditorTools
{
    public static class Physics2DDiagnostics
    {
        [MenuItem("WWIII/Diagnostics/Print Collision Matrix (Player/Ground/Default)")]
        public static void PrintCollisionMatrix()
        {
            int player = LayerMask.NameToLayer("Player");
            int ground = LayerMask.NameToLayer("Ground");
            int def = LayerMask.NameToLayer("Default");
            if (player < 0) player = 11;
            if (ground < 0) ground = 3;
            if (def < 0) def = 0;

            bool pg = !Physics2D.GetIgnoreLayerCollision(player, ground);
            bool pd = !Physics2D.GetIgnoreLayerCollision(player, def);
            Debug.Log($"[Diag] Player({player})-Ground({ground}): {pg}; Player-Default({def}): {pd}");
        }

        [MenuItem("WWIII/Diagnostics/Validate Ground Tilemap Collider")]
        public static void ValidateGroundTilemap()
        {
            var go = GameObject.Find("Tilemaps_age7/Ground") ?? GameObject.Find("Ground");
            if (go == null)
            {
                Debug.LogWarning("[Diag] Ground object not found.");
                return;
            }
            var tmc = go.GetComponent<TilemapCollider2D>();
            var cc = go.GetComponent<CompositeCollider2D>();
            if (!tmc) Debug.LogWarning("[Diag] TilemapCollider2D missing.");
            else Debug.Log($"[Diag] TilemapCollider2D.usedByComposite={tmc.usedByComposite}");
            if (!cc) Debug.LogWarning("[Diag] CompositeCollider2D missing.");
            else Debug.Log($"[Diag] Composite paths={cc.pathCount}, bounds={cc.bounds}");
        }
    }
}
#endif

