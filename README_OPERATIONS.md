# WWIII_SideScroller – Ops & Tooling

**Canonical path:** `~/Desktop/WWIII/WWIII_SideScroller`  
**Unity:** `6000.1.15f1`

## Daily flow
1. Unity Hub → open with 6000.1.15f1.
2. Code/Cursor Pro → edit C# in `Assets/_Project/Scripts/`.
3. GitHub Desktop → commit (GPG) and push to `origin/main`.
4. Terminal → run `tools/zip_project.sh` to export a clean zip.
5. ChatGPT → paste logs when you need fixes.

## Player setup
- Add `Rigidbody2D (Dynamic)` and `BoxCollider2D` to **Player**.
- Add `PlayerController2D` component.
- (Optional) Create a child **Feet** at the bottom of Player; assign to script.
- Set your ground tiles/objects to layer **Ground**; assign that LayerMask in the component.

## Scripts
- `bootstrap_repo.sh` – initializes git, .gitignore and LFS attributes.
- `tools/zip_project.sh` – zips the project minus caches/builds and writes `CHECKSUMS.txt`.

## Snippets
```bash
xattr -dr com.apple.quarantine ~/Desktop/WWIII/WWIII_SideScroller || true
chmod -R u+rwX ~/Desktop/WWIII/WWIII_SideScroller
```
