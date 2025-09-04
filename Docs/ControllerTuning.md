# Controller Tuning (WWIII Biographical Side-Scroller)

This checklist ensures a premium controller experience on iPhone 16 + Backbone One and Apple TV 4K + DualSense.

## Age-Based Control Evolution
- Ages 7–11 (Child): Move, Jump, Interact only. Hold-to-jump enabled. Generous deadzones.
- Ages 14–17 (Teen): Add Dash and Wall interactions. Slightly tighter deadzones.
- Ages 21+ (Adult): Add Shoot/Tooling. Firm, precise movement; stricter deadzones.

## Input Maps
- Player map: Move(Vector2), Jump, Dash, Interact, Shoot, Album, Pause
- Child map: Move(Vector2), Jump, Interact
- Deadzones: Left stick 0.20–0.25; D‑pad 0.05

## Haptics Patterns
- Age change: soft pulse (child) → two-step (teen) → firm pulse (adult)
- Jump: quick, light pulse. Dash: short firm. Land: tiny tap.

## Camera Feel
- Handheld (iPhone): HorizontalLookDistance ≈ 2.5–3.0; ResetSpeed 0.5–0.7; render scale 0.8; MSAA 2x; HDR off.
- Living room (tvOS): UI safe margins ~5–8%; ensure confiner bounds; render scale 1.0 where GPU allows.

## UI Navigation
- Ensure EventSystem uses Input System UI module.
- D-pad/Left stick move, A confirm, B cancel. LB/ RB page in album.

## QA Checklist
- Latency target <16ms input to action.
- Haptics: no buzz during idle; short patterns only.
- All actions operate under age gating.
- Keyboard works on WebGL/macOS as fallback.

## Troubleshooting
- If `vstuc` missing, install the Unity VS Code extension.
- If no IntelliSense, regenerate .csproj in Unity and restart C# server.

