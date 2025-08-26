# Player Controller Setup Guide

## Overview
Complete guide for setting up the advanced Player Controller system with coyote time, jump buffering, variable jump height, and mobile-optimized controls.

## Required Components

### 1. PlayerData ScriptableObject
**Location**: `/Assets/_Project/Scripts/Data/PlayerData.cs`  
**Purpose**: Data-driven configuration for all player movement parameters

**Create PlayerData Asset**:
1. Right-click in Project → `Create` → `WWIII` → `Player Data`
2. Name: `DefaultPlayerData`
3. Location: `/Assets/_Project/Data/`
4. Configure movement settings in Inspector

**Key Settings**:
- **Move Speed**: 8f (units per second)
- **Jump Height**: 4f (world units)
- **Coyote Time**: 0.15f (seconds after leaving ground)
- **Jump Buffer**: 0.2f (seconds to register early jump input)
- **Ground Layer Mask**: Set to "Ground" layer

### 2. PlayerController Script
**Location**: `/Assets/_Project/Scripts/Player/PlayerController.cs`  
**Features**:
- ✅ Coyote time (jump after leaving ground)
- ✅ Jump buffering (register jumps early)
- ✅ Variable jump height (hold for higher jumps)
- ✅ Air jumps (configurable double/triple jump)
- ✅ Fast falling (hold down to fall faster)
- ✅ Mobile haptic feedback
- ✅ Smooth ground detection
- ✅ Physics-based movement

### 3. TouchControls System
**Location**: `/Assets/_Project/Scripts/UI/TouchControls.cs`  
**Purpose**: Mobile touch overlay controls with visual feedback

### 4. CameraFollowController
**Location**: `/Assets/_Project/Scripts/Camera/CameraFollowController.cs`  
**Features**: Look-ahead, bounds checking, deadzone, camera shake

## Player GameObject Setup

### Step 1: Create Player GameObject
1. Create Empty GameObject → Name: "Player"
2. Position: (0, 0, 0)
3. Tag: "Player"
4. Layer: "Default" (or create "Player" layer)

### Step 2: Add Required Components

**Physics Components**:
- Add `Rigidbody2D`
  - Mass: 1
  - Drag: 0
  - Angular Drag: 0.05
  - Gravity Scale: 0 (handled by PlayerController)
  - Freeze Rotation: ✅ Z
  - Collision Detection: Continuous

**Collider Setup**:
- Add `CapsuleCollider2D`
  - Size: (0.8, 1.8) - adjust based on sprite
  - Offset: (0, 0.9) - center collider on sprite
  - Material: Create Physics Material 2D with Friction: 0

**Visual Component**:
- Add `SpriteRenderer`
  - Assign player sprite
  - Order in Layer: 0

**Player Logic**:
- Add `PlayerController` component
- Assign PlayerData asset to PlayerData field

### Step 3: Ground Check Setup
1. Create child GameObject → Name: "GroundCheck"
2. Position relative to Player: (0, -0.9, 0)
3. PlayerController will automatically use this transform

**Manual Ground Check** (if automatic doesn't work):
- Assign GroundCheck transform in PlayerController
- Adjust position to bottom of player collider

### Step 4: Animation Setup (Optional)
1. Add `Animator` component
2. Create Animator Controller: `PlayerAnimator`
3. Setup animation parameters:
   - `IsGrounded` (Bool)
   - `VelocityX` (Float)
   - `VelocityY` (Float)
   - `Jump` (Trigger)
   - `Land` (Trigger)

## Scene Setup

### Camera Configuration
1. Main Camera → Add `CameraFollowController`
2. Set Target: Player Transform
3. Configure bounds (see Camera Bounds section)

### Ground Setup
1. Create ground platforms with `BoxCollider2D`
2. Set Layer: "Ground"
3. Assign Ground layer to PlayerData Ground Layer Mask

### Input Actions Asset
1. Create Input Actions: `/Assets/_Project/Settings/InputActions.inputactions`
2. Configure Action Maps:
   - **Player Map**: Move (Vector2), Jump (Button)
   - **UI Map**: Navigate, Submit, Cancel
3. Assign to InputManager in GameManager

## Mobile Touch Controls Setup

### Touch UI Canvas
1. Create UI Canvas → Name: "TouchControlsCanvas"
2. Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Screen Match Mode: Match Width Or Height (0.5)

### Touch Control Buttons
1. Create UI Panel → Name: "TouchControls"
2. Add three buttons:
   - **Left Button**: Bottom-left corner
   - **Right Button**: Bottom-left corner (next to left)
   - **Jump Button**: Bottom-right corner

### TouchControls Component
1. Add `TouchControls` script to TouchControls panel
2. Assign button references
3. Configure colors and settings

**Button Setup**:
- Normal Color: White (1,1,1,0.8)
- Pressed Color: Gray (0.8,0.8,0.8,1)
- Button Scale: 1.0 (auto-adjusts for screen size)

## Camera Bounds Setup

### Method 1: GameObject with Collider
1. Create GameObject → Name: "CameraBounds"
2. Add `BoxCollider2D`
   - IsTrigger: ✅ true
   - Size: Cover entire level area
3. Tag: "CameraBounds"
4. CameraFollowController will auto-detect

### Method 2: Manual Bounds
1. In CameraFollowController, set Use Bounds: ✅ true
2. Configure Min Bounds and Max Bounds manually
3. Bounds are world coordinates where camera center can move

## Layer Setup

### Required Layers
1. **Ground** (Layer 3): All ground/platform objects
2. **Player** (Layer 8): Player character
3. **UI** (Layer 5): Touch controls and UI

### Physics Settings
1. Open: `Edit` → `Project Settings` → `Physics 2D`
2. Layer Collision Matrix:
   - Player should collide with Ground
   - UI should not collide with anything

## Testing Checklist

### Basic Movement
- [ ] Player moves left/right with input
- [ ] Player stops smoothly when input released
- [ ] Movement works with keyboard/gamepad/touch

### Jump Mechanics
- [ ] Normal jump works
- [ ] Variable jump height (hold/release)
- [ ] Coyote time allows jump after leaving ground
- [ ] Jump buffering registers early input
- [ ] Air jumps work (if configured)
- [ ] Fast fall works (hold down)

### Mobile Controls
- [ ] Touch buttons respond to input
- [ ] Visual feedback shows button presses
- [ ] Haptic feedback works on device
- [ ] Controls auto-hide on non-mobile platforms

### Camera System
- [ ] Camera follows player smoothly
- [ ] Camera respects bounds
- [ ] Look-ahead system works
- [ ] Camera shake triggers properly

### Physics Integration
- [ ] Player lands correctly on platforms
- [ ] Ground detection works reliably
- [ ] No stuttering or jitter
- [ ] Consistent 60 FPS performance

## Performance Considerations

### Mobile Optimization
- Touch controls only render on mobile platforms
- Haptic feedback uses platform-specific APIs
- Rigidbody2D uses Continuous collision detection
- Ground checks use OverlapBox for efficiency

### Frame Rate Consistency
- All movement uses FixedUpdate for physics
- Input handling in Update for responsiveness
- Timer updates in Update for accuracy
- Visual updates in LateUpdate

## Debug Features

### Gizmos (Scene View)
- **Green/Red Box**: Ground check area
- **Blue Line**: Current velocity vector
- **Yellow Wire Cube**: Camera bounds
- **Green Wire Cube**: Camera deadzone
- **Blue Wire Sphere**: Camera look-ahead target

### Debug Options
- Enable Debug Gizmos: Shows ground check
- Enable Debug Log: Console output for jump events
- FPS Counter: Displays current frame rate

## Troubleshooting

### Common Issues
1. **Player falls through ground**: Check Ground layer mask in PlayerData
2. **Jumps don't register**: Verify Input Actions assignment
3. **Camera doesn't follow**: Check Target assignment in CameraFollowController
4. **Touch controls don't work**: Verify EventTriggers are properly configured
5. **Inconsistent movement**: Ensure Rigidbody2D Gravity Scale is 0

### Performance Issues
1. **Frame drops**: Check for GC allocations in movement code
2. **Stuttering**: Verify movement calculations in FixedUpdate
3. **Input lag**: Ensure input handling in Update, not FixedUpdate

This setup provides a production-ready player controller suitable for mobile and desktop platforms with advanced movement mechanics and professional polish.