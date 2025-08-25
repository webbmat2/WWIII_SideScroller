# Layer Setup for WWIII_SideScroller

## Required Layers

Add these layers in **Project Settings → Tags and Layers**:

1. **Player** (Layer 6)
2. **Ground** (Layer 3) - Already exists
3. **Hazard** (Layer 7) 
4. **Collectible** (Layer 8)
5. **CameraBounds** (Layer 9)

## Collision Matrix Setup

In **Project Settings → Physics 2D → Layer Collision Matrix**:

### Enable Collisions:
- ✅ **Player ↔ Ground**: ON (solid platform collisions)

### Disable Collisions:
- ❌ **Player ↔ Hazard**: OFF (hazards use triggers only)
- ❌ **Player ↔ CameraBounds**: OFF (camera bounds are triggers)
- ❌ **Everything ↔ CameraBounds**: OFF (camera bounds don't block anything)

### Trigger-Only Interactions:
- **Hazard**: All DamageOnTouch components use `isTrigger = true`
- **CameraBounds**: BoxCollider2D with `isTrigger = true`
- **Checkpoints**: BoxCollider2D with `isTrigger = true`

## Layer Assignments

### Player GameObject:
- Layer: **Player**
- Rigidbody2D constraints: **FreezeRotation** only
- BoxCollider2D: **NOT** trigger

### Ground Tilemap:
- Layer: **Ground** 
- CompositeCollider2D: **NOT** trigger

### Hazards (Spikes, Pits):
- Layer: **Hazard**
- Collider2D: **IS** trigger
- Has DamageOnTouch component

### Camera Bounds:
- Layer: **CameraBounds**
- BoxCollider2D: **IS** trigger
- Used by FitBoundsToTilemap and CameraFollow2D

## Current Setup Status

✅ Ground layer exists and working
✅ Water layer can be used as Hazard fallback
⚠️ Need to create: Player, Hazard, Collectible, CameraBounds layers
⚠️ Need to configure collision matrix

## Quick Setup Commands

Run these in Level1Setup component:
1. `Setup Level 1 Now` - Auto-configures scene
2. `Validate Level 1` - Checks all systems
3. `Test Damage System` - Tests player damage/knockback