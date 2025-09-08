# 1) FINDINGS SUMMARY

| Item | Severity | Action | Doc URL |
|---|---|---|---|
| Input Actions lack tvOS/iOS gamepad coverage | BLOCKER | Create/update a consolidated `.inputactions` with Gamepad bindings for DualSense/Backbone (Move, Jump, Dash, Interact, Pause, UI Navigate) | https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Actions.html |
| tvOS/iOS Player settings not set for shipping | BLOCKER | Set Scripting Backend=IL2CPP, Architecture=ARM64; Active Input Handling=Both; tvOS Require Extended Controller=Enabled | https://docs.unity3d.com/Manual/ |
| Addressables baseline not configured | WARNING | Create minimal groups (e.g., Scenes), mark scene Addressable, Analyze→Fix→Build | https://docs.unity3d.com/Packages/com.unity.addressables@2.7/manual/index.html |
| Odin assemblies incomplete (PDBs only) | WARNING | Reimport Odin 3.3.1.13 or remove incomplete `Assets/ThirdParty/Sirenix` to avoid editor noise; clear Odin defines if unused | https://odininspector.com/patch-notes/3-3-1-13 |
| Corgi Engine content missing for referenced scenes | WARNING | Reimport Corgi 9.3 to `Assets/ThirdParty/CorgiEngine` if those scenes/scripts are to be used | https://corgi-engine-docs.moremountains.com/index.html |
| Adaptive Performance assets present (not pinned) | WARNING | Remove `Assets/Adaptive Performance/*` content to reduce import noise | https://docs.unity3d.com/Manual/ |
| Legacy XR folders/resources present | WARNING | Remove `Assets/XR/*` (XR not in project scope) | https://docs.unity3d.com/Manual/ |
| URP SRP asset naming preference | NICE | Rename `Assets/_Project/URP_Asset.asset` to `Assets/_Project/WWIII_URP_Asset.asset`, then assign in Graphics Settings | https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.2 |
| DOTween setup verification | NICE | Run DOTween Setup; ensure UI, Sprite, Physics2D modules; verify link.xml for IL2CPP | https://dotween.demigiant.com/documentation.php#dotweenPro |

Notes:
- Pinned stack confirmed from instructions/stack.manifest.json: Unity 6000.2.1f1, URP 17.2.0, Input System 1.14.2, Timeline 1.8.9, Addressables 2.7.2, Cinemachine 3.1.4. All recommendations stay within these versions (VERSION GATE satisfied).
- URP asset at `Assets/_Project/URP_Asset.asset` already matches mobile/tvOS targets: HDR Off, MSAA 2x, Render Scale 0.8, Additional Lights Off, Shadows Off (distance 0).


# 2) ACTION PLAN

A) File Hygiene
1) Delete stale folder: `Temp 2`
2) Delete `Assets/Adaptive Performance` (not pinned)
3) Delete `Assets/XR` (legacy, out of scope)
4) Delete empty `Assets/Plugins/Sirenix`
5) Delete incomplete `Assets/ThirdParty/Sirenix` (PDBs only)
6) Delete empty duplicate folder: `Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate/Textures/UI/Button 2`
7) Rename `Assets/_Project/URP_Asset.asset` → `Assets/_Project/WWIII_URP_Asset.asset` (and `.meta`)

B) Packages (install/remove/update within pinned versions)
8) Verify pinned packages installed: URP 17.2.0, Input System 1.14.2, Timeline 1.8.9, Addressables 2.7.2, Cinemachine 3.1.4, UGUI 2.0.0
9) Ensure unpinned extras remain removed: Collab, XR Management, Visual Scripting, 2D Enhancers, Feature.Mobile, Feature.Gameplay-Storytelling

C) Project Settings (Unity click paths → expected values)
10) Player → Active Input Handling: Both
11) iOS/tvOS → Scripting Backend: IL2CPP; Architecture: ARM64
12) tvOS → Require Extended Game Controller: Enabled
13) Graphics → Scriptable Render Pipeline Settings: `Assets/_Project/WWIII_URP_Asset.asset`

D) DOTween setup + asmdef cleanup
14) Tools → DOTween Utility Panel → Setup; enable UI, Sprite, Physics2D modules; verify IL2CPP link
15) Duplicate asmdefs to delete: None detected (keep DOTween.Modules, DOTweenPro.*, and `Assets/_Project/Scripts/WWIII.Scripts.asmdef`)

E) Addressables baseline (groups/profiles, Analyze→Fix→Build)
16) Create Groups: “Scenes” (Bundled, Local Build/Load Path = default)
17) Mark Addressable: `Assets/WWIII/Scenes/BioLevel_Age7_Clean.unity`
18) Analyze: Fix Selected Rules; Build → New Build → Default Build Script

F) Input Actions for tvOS DualSense + iOS Backbone
19) Create/update `Assets/_Project/Settings/GameInput.inputactions` with:
   - Player: Move (Gamepad leftStick + WASD), Jump (buttonSouth + space), Dash (rightShoulder + leftShift), Interact (buttonWest + e), Pause (start + escape)
   - UI: Navigate (Gamepad dpad + Arrow keys), Submit (buttonSouth + enter), Cancel (buttonEast + escape)
   - Control Schemes: Gamepad, Keyboard&Mouse, Touch
   - Optional device-specific bindings (still within 1.14.2): `<DualSenseGamepadHID>`, `<DualShockGamepadHID>`, `<iOSGameController>` mirroring Gamepad bindings

G) Scene/UI camera stack validation
20) Each playable scene should have: Base camera (Renderer=2D/URP), UI camera (Overlay) stacked on Base; Canvas in Screen Space - Camera bound to UI camera

H) Reimport/Build order
21) Close Unity; delete `Library/`
22) Reopen; run DOTween Setup
23) Input Actions: Generate C# class; set as default in Input System settings
24) Addressables: Analyze→Fix→Build
25) Validate a scene; Build tvOS (primary), iOS (fallback)


# 3) SHELL BLOCK (zsh-safe)

rm -rf 'Temp 2'
rm -rf 'Assets/Adaptive Performance'
rm -rf 'Assets/XR'
rmdir 'Assets/Plugins/Sirenix'
rm -rf 'Assets/ThirdParty/Sirenix'
rm -rf 'Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate/Textures/UI/Button 2'
mv 'Assets/_Project/URP_Asset.asset' 'Assets/_Project/WWIII_URP_Asset.asset'
mv 'Assets/_Project/URP_Asset.asset.meta' 'Assets/_Project/WWIII_URP_Asset.asset.meta'
find 'Assets' -name '.DS_Store' -type f -exec rm -f '{}' \;


# 4) UNITY CHECKLISTS

- Project Settings (C)
  - Edit → Project Settings → Player → Other Settings (for iOS and tvOS):
    - Scripting Backend: IL2CPP
    - Architecture: ARM64
    - Active Input Handling: Both
  - Player → tvOS:
    - Require Extended Game Controller: Enabled
  - Edit → Project Settings → Graphics:
    - Scriptable Render Pipeline Settings: select `Assets/_Project/WWIII_URP_Asset.asset`
  - Verify:
    - Build target tvOS shows IL2CPP + ARM64
    - No missing SRP asset warnings

- DOTween + asmdef (D)
  - Tools → Demigiant → DOTween Utility Panel → Setup DOTween…
  - Modules: Enable UI, Sprite, Physics2D (TextMeshPro optional)
  - Verify:
    - `Assets/Resources/DOTweenSettings.asset` exists (modules set)
    - Play: no DOTween missing module errors
    - asmdefs present: DOTween.Modules, DOTweenPro.Scripts, DOTweenPro.EditorScripts, WWIII.Scripts; no duplicates

- Addressables (E)
  - Window → Asset Management → Addressables → Groups
  - Create Group: “Scenes” (Bundled, Local paths)
  - Mark Addressable: `Assets/WWIII/Scenes/BioLevel_Age7_Clean.unity`
  - Analyze: Tools (hamburger) → Analyze → Fix Selected Rules
  - Build: Build → New Build → Default Build Script
  - Verify:
    - Analyze returns clean
    - Build creates content catalog without errors

- Input System (F)
  - Project: select/create `Assets/_Project/Settings/GameInput.inputactions`
  - Inspector:
    - Generate C# Class: Enabled (e.g., GameInput)
    - Control Schemes: Gamepad, Keyboard&Mouse, Touch
  - Edit → Project Settings → Input System Package:
    - Default Input Actions: assign GameInput
  - Verify:
    - In a test scene, add `PlayerInput` (Actions=GameInput, Default Map=Player)
    - DualSense/Backbone: Move/Jump/Dash/Interact/Pause work; UI navigation works

- Scene/UI Camera Stack (G)
  - Open representative scene (e.g., `Assets/WWIII/Scenes/BioLevel_Age7_Clean.unity`)
  - Base Camera:
    - Render Type: Base
    - Renderer: 2D Renderer from SRP asset
  - UI Camera:
    - Render Type: Overlay
    - Added to Base camera’s Stack
  - Canvas:
    - Render Mode: Screen Space - Camera
    - Render Camera: UI Camera
  - Verify:
    - UI renders on top; single Base camera; no depth artifacts

- Reimport/Build Order (H)
  - Close Unity; delete `Library/`
  - Reopen; wait for import
  - DOTween Setup; Input Actions: Generate class; Addressables: Analyze→Fix→Build
  - File → Build Settings → tvOS → Switch Platform
  - Build; then test iOS fallback build


# 5) NARRATIVE NEXT STEPS

- Boot/Main Flow Scenes
  - Create Boot → MainMenu → LevelSelect scenes; wire Addressables load for levels.
  - Success: Boot to Menu < 2s; Level loads via Addressables; zero console errors.

- Yarn Integration Baseline
  - Reimport Yarn Spinner 3.0.3; add a simple dialogue node and a minimal runner hook.
  - Success: Dialogue starts and advances via Submit; no runtime errors.

- Save/Checkpoint Service (ES3)
  - Reimport ES3 3.5.24; implement progression save (last level, options) if absent.
  - Success: Relaunch preserves state; ES3 file <1 MB; no blocking GC spikes.

- Performance Pass (tvOS/iOS)
  - Verify URP settings (HDR Off, MSAA 2x, 0.8 scale, no extra lights/shadows); enable texture streaming and atlas caps.
  - Success: 60 FPS on Apple TV 4K empty scene; draw calls <50; runtime memory <200 MB.

- Controller Polish (DualSense/Backbone)
  - Add haptic/lightbar hooks using Input System haptics (where available).
  - Success: Landing/Interact haptics trigger; no exceptions on iOS/tvOS.

- Addressables Profiles per Platform
  - Add “Local-tvos” and “Local-ios” profiles; set build/load paths appropriately.
  - Success: Build with each profile; runtime loads content correctly on each platform.

- Asset Organization Cleanup
  - Keep all vendor assets under `Assets/ThirdParty/*` (except `Assets/Plugins/Demigiant`); remove empties/duplicates.
  - Success: Clean import; no missing meta GUIDs; organized Project window.

- Scene Validator Tooling
  - Add an editor validator to check Base+UI cameras, Canvas mode, PlayerInput presence, Addressable flags.
  - Success: Running across `Assets/WWIII/Scenes/*` returns 0 errors.

