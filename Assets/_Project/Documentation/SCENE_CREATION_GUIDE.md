# Scene Creation Guide

## Scene Flow Architecture

```
Boot → MainMenu → Level_Select → L1_Tutorial → L2_Stub/L3_Stub/L4_Stub/L5_Stub
```

## Required Scenes to Create

### 1. Boot Scene
**File**: `Boot.unity`  
**Purpose**: System initialization and splash screen  
**Components**:
- BootstrapManager (handles initialization)
- Canvas with splash UI
- Background audio source

**Setup Steps**:
1. Create new scene: `File` → `New Scene` → `Basic (Built-in)` 
2. Save as `/Assets/_Project/Scenes/Boot.unity`
3. Delete default Main Camera and Directional Light
4. Create GameObject → Name: "Bootstrap"
5. Add Component: `BootstrapManager`
6. Create UI Canvas for splash screen
7. Add to Build Settings (Index 0)

### 2. MainMenu Scene  
**File**: `MainMenu.unity`  
**Purpose**: Main menu with navigation options  
**Components**:
- MainMenuManager
- UI Canvas with menu buttons
- Audio source for background music

**Setup Steps**:
1. Create new scene: `File` → `New Scene` → `Basic (Built-in)`
2. Save as `/Assets/_Project/Scenes/MainMenu.unity`
3. Create GameObject → Name: "MenuManager"
4. Add Component: `MainMenuManager`
5. Create UI Canvas → Name: "MainMenuUI"
6. Add buttons: Play, Settings, Credits, Quit
7. Configure MainMenuManager button references
8. Add to Build Settings (Index 1)

### 3. Level_Select Scene
**File**: `Level_Select.unity`  
**Purpose**: Level selection interface  
**Components**:
- LevelSelectManager
- Scrollable level grid
- Level button prefabs

**Setup Steps**:
1. Create new scene: `File` → `New Scene` → `Basic (Built-in)`
2. Save as `/Assets/_Project/Scenes/Level_Select.unity`
3. Create GameObject → Name: "LevelSelectManager"
4. Add Component: `LevelSelectManager`
5. Create UI Canvas with ScrollRect
6. Create level button prefab
7. Configure level data array
8. Add to Build Settings (Index 2)

### 4. L1_Tutorial Scene
**File**: `L1_Tutorial.unity`  
**Purpose**: Fully playable tutorial level  
**Components**:
- Player spawn point
- Tutorial triggers and UI
- Basic platforming elements
- Camera bounds

**Setup Steps**:
1. Create new scene: `File` → `New Scene` → `2D`
2. Save as `/Assets/_Project/Scenes/L1_Tutorial.unity`
3. Set up camera bounds
4. Add player spawn point
5. Create basic platform layout
6. Add tutorial UI elements
7. Add to Build Settings (Index 3)

### 5. Stub Scenes (L2-L5)
**Files**: `L2_Stub.unity`, `L3_Stub.unity`, `L4_Stub.unity`, `L5_Stub.unity`  
**Purpose**: Placeholder levels for future development  
**Components**:
- Basic camera setup
- "Coming Soon" UI message
- Return to menu functionality

**Setup Steps for Each**:
1. Create new scene: `File` → `New Scene` → `2D`
2. Save with appropriate name
3. Add "Coming Soon" UI
4. Add return to menu button
5. Add to Build Settings (Indices 4-7)

## Build Settings Configuration

**Final Build Settings Order**:
0. Boot
1. MainMenu  
2. Level_Select
3. L1_Tutorial
4. L2_Stub
5. L3_Stub
6. L4_Stub
7. L5_Stub

## Scene-Specific Components

### Camera Configuration (All Scenes)
- **Projection**: Orthographic (for 2D)
- **Size**: 5 (adjust based on level design)
- **Background Color**: Appropriate for scene theme
- **Culling Mask**: Configured for layer visibility

### Canvas Setup (UI Scenes)
- **Render Mode**: Screen Space - Overlay
- **UI Scale Mode**: Scale With Screen Size
- **Reference Resolution**: 1920x1080
- **Screen Match Mode**: Match Width Or Height (0.5)

### Audio Source Configuration
- **Play On Awake**: True for background music
- **Loop**: True for background tracks
- **Volume**: 0.7 (adjustable)
- **Priority**: 128 (default)

## Post-Creation Checklist

### For Each Scene:
- [ ] Saved in correct location (`/Assets/_Project/Scenes/`)
- [ ] Added to Build Settings in correct order
- [ ] Camera properly configured
- [ ] Required manager components added
- [ ] UI elements properly referenced
- [ ] Audio sources configured
- [ ] Scene loads without errors
- [ ] Transitions work correctly

### Scene Transition Testing:
- [ ] Boot → MainMenu works
- [ ] MainMenu → Level_Select works
- [ ] Level_Select → L1_Tutorial works
- [ ] Level_Select → Stub scenes work
- [ ] Back navigation functions properly
- [ ] Scene loading shows no console errors

## Level Data Configuration

### LevelData Array Setup (Level_Select)
```csharp
levels[0] = L1_Tutorial (unlocked: true, isStub: false)
levels[1] = L2_Stub (unlocked: false, isStub: true)
levels[2] = L3_Stub (unlocked: false, isStub: true)
levels[3] = L4_Stub (unlocked: false, isStub: true)
levels[4] = L5_Stub (unlocked: false, isStub: true)
```

## Camera Bounds Setup

### Tutorial Level Bounds
- Create empty GameObject → Name: "CameraBounds"
- Add BoxCollider2D → IsTrigger: true
- Size to encompass playable area
- Position at level center
- Tag as "CameraBounds"

This ensures the camera system can properly contain the view within level boundaries as specified in project rules.