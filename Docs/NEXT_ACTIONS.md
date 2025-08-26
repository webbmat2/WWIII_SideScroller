NEXT ACTIONS

1) Fix camera clamp cleanly
- Create CameraBounds (Empty) + BoxCollider2D (IsTrigger = ON).
- Add FitBoundsToTilemap; Source Renderer = TilemapRenderer; padding (1,1); autoUpdate ON.
- Main Camera → CameraFollow2D.WorldBounds = CameraBounds (BoxCollider2D).
Commit: "fix(camera): re-add non-blocking CameraBounds + auto-fit; clamp CameraFollow2D"

2) Install Git LFS (needed for art)
- brew install git-lfs
- git lfs install
- echo "*.png filter=lfs diff=lfs merge=lfs -text" >> .gitattributes
Commit: "chore(git): enable Git LFS for PNG art"

3) Editor helper
- Keep Assets/_Project/Editor/TileStripPainter.cs
Commit: "feat(editor): add TileStripPainter for tile strip layout"
