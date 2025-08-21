ART BRIEF — PARALLAX PACKS

Goal: Unity-ready parallax backgrounds and foreground sprites per street block, painted from reference photos (no direct use of Google Street View imagery).

Output
- PNG, sRGB. Height 1080 px, variable width. 64 px transparent feather at left/right edges.
- Layers:
  * BG_far: sky + distant trees
  * BG_mid: primary houses/shapes
  * BG_near: shrubs, curbline masses
- Foreground sprites (separate PNGs): mailbox, fence posts, porch lips, rocks, trees (pivot bottom-center).
- Optional tiles: 32×32 curb/sidewalk/grass variants.

Style
- Posterized photo-toon (flat color masses, mild edge cleanup).
- Consistent daylight; avoid copyright marks or brand logos.

Naming
- L1_B03_BG_far.png, L1_B03_BG_mid.png, L1_B03_BG_near.png
- L1_B03_mailbox_A.png …

Delivery
- Include a low-res JPEG preview with layer layout.
- Provide a JSON/YAML with suggested parallax speeds (far 0.2, mid 0.5, near 0.8).
