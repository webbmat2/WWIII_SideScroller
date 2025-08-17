using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(BoxCollider2D))]
public class FitBoundsToTilemap : MonoBehaviour
{
    public Renderer sourceRenderer;
    public Vector2 padding = new Vector2(1f, 1f);
    public bool autoUpdate = true;

    BoxCollider2D box;

    void OnEnable() { box = GetComponent<BoxCollider2D>(); Sync(); }
    void OnValidate() { Sync(); }
    void Update() { if (!Application.isPlaying && autoUpdate) Sync(); }

    public void Sync()
    {
        if (!box || !sourceRenderer) return;
        var b = sourceRenderer.bounds;
        box.size = new Vector2(b.size.x + padding.x * 2f, b.size.y + padding.y * 2f);
        box.offset = (Vector2)b.center - (Vector2)transform.position;
        box.isTrigger = true; // must be trigger so it never blocks the player
    }
}