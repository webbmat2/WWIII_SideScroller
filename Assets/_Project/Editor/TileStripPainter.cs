// Assets/_Project/Editor/TileStripPainter.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileStripPainter : EditorWindow
{
    [Header("Targets")]
    public Tilemap tilemap;
    public TileBase tile; // e.g., Assets/_Project/Tiles/Tile_White.asset

    [Header("Strip Settings")]
    public int rowY = 0;          // Tilemap cell Y row to paint on
    public int startX = 0;        // First cell X
    public int length = 100;      // How many tiles
    public bool overwriteExisting = true;

    [MenuItem("WWIII/Tile Tools/Tile Strip Painter")]
    public static void ShowWindow()
    {
        var w = GetWindow<TileStripPainter>("Tile Strip Painter");
        w.minSize = new Vector2(360, 200);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            tilemap = (Tilemap)EditorGUILayout.ObjectField("Tilemap", tilemap, typeof(Tilemap), true);
            if (GUILayout.Button("Use Selection", GUILayout.Width(110)))
            {
                tilemap = Selection.activeGameObject ? Selection.activeGameObject.GetComponentInChildren<Tilemap>() : null;
            }
        }

        tile = (TileBase)EditorGUILayout.ObjectField("Tile", tile, typeof(TileBase), false);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Strip Settings", EditorStyles.boldLabel);
        rowY   = EditorGUILayout.IntField("Row Y", rowY);
        startX = EditorGUILayout.IntField("Start X", startX);
        length = Mathf.Max(1, EditorGUILayout.IntField("Length", length));
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Scan From Existing"))
                TryAutoFillRowAndStart();
            if (GUILayout.Button("Paint Strip", GUILayout.Height(28)))
                Paint();
        }
    }

    void TryAutoFillRowAndStart()
    {
        if (!tilemap) { ShowNotification(new GUIContent("Assign a Tilemap.")); return; }

        // Find the row (Y) with the most tiles, then set startX to the first empty cell to the right.
        var bounds = tilemap.cellBounds;
        int bestY = 0, bestCount = -1, rightmostXOnBest = 0;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            int count = 0;
            int right = bounds.xMin;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                if (tilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    count++;
                    if (x > right) right = x;
                }
            }
            if (count > bestCount)
            {
                bestCount = count;
                bestY = y;
                rightmostXOnBest = right;
            }
        }

        rowY = bestY;
        startX = bestCount > 0 ? rightmostXOnBest + 1 : bounds.xMin;
        Repaint();
    }

    void Paint()
    {
        if (!tilemap) { ShowNotification(new GUIContent("Assign a Tilemap.")); return; }
        if (!tile)    { ShowNotification(new GUIContent("Assign a Tile asset.")); return; }

        Undo.RecordObject(tilemap, "Paint Tile Strip");

        for (int i = 0; i < length; i++)
        {
            var p = new Vector3Int(startX + i, rowY, 0);
            if (!overwriteExisting && tilemap.HasTile(p)) continue;
            tilemap.SetTile(p, tile);
        }

        tilemap.RefreshAllTiles();
        EditorUtility.SetDirty(tilemap);
        ShowNotification(new GUIContent($"Painted {length} tiles at Y={rowY} from X={startX}."));
    }
}