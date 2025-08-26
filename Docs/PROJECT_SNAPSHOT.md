Unity 6.x LTS. Project path ~/Desktop/WWIII/WWIII_SideScroller.
Active camera: Main Camera + CameraFollow2D (Offset 0,1,-10). Bounds collider intended (IsTrigger ON).
Movement: coyoteTime ~0.12, jumpBuffer ~0.12, variableJump ON.
Level: Grid+Tilemap, CompositeCollider2D (Merge), RB2D Static, Layer “Ground”.
Scene: 10_L1.Northville.unity
Open items:
- Re-add CameraBounds as trigger + FitBoundsToTilemap (source = TilemapRenderer).
- Install Git LFS (logs previously showed missing).
- Commit staged Editor tool: Assets/_Project/Editor/TileStripPainter.cs
