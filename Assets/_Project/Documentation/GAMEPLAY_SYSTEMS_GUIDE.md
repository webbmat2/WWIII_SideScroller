# Gameplay Systems Setup Guide

## Overview
Complete guide for setting up the core gameplay systems: Hazards, Pickups, Enemies, and Checkpoint/Respawn system for WWIII SideScroller.

## 🎯 Core Gameplay Systems

### 1. Hazard System
**Purpose**: Instant-kill obstacles that trigger player respawn  
**Script**: `Hazard.cs`

**Features**:
- ✅ Multiple hazard types (Spikes, Saws, Fire, Acid, Laser, etc.)
- ✅ Damage system with configurable values
- ✅ Particle effects and audio feedback
- ✅ Camera shake on impact
- ✅ Mobile haptic feedback
- ✅ Animation triggers
- ✅ Optional destruction on contact

### 2. Pickup System
**Purpose**: Collectible items with magnetism and visual feedback  
**Script**: `Pickup.cs`

**Features**:
- ✅ Multiple pickup types (Coins, Gems, PowerUps, Health, etc.)
- ✅ Magnetic collection within range
- ✅ Bob and rotation animations
- ✅ Flash effects and particle systems
- ✅ Audio feedback and haptic response
- ✅ Score integration ready

### 3. Enemy System
**Purpose**: AI-driven enemies with patrol, chase, and attack behaviors  
**Script**: `BasicEnemy.cs`

**Features**:
- ✅ State machine (Patrol, Chase, Attack, Wait, Return)
- ✅ Configurable patrol patterns (distance-based or waypoint-based)
- ✅ Player detection with line-of-sight
- ✅ Attack system with cooldowns
- ✅ Stomping mechanic (player can jump on enemies)
- ✅ Visual state feedback (color changes)
- ✅ Health system and death effects

### 4. Checkpoint & Respawn System
**Purpose**: Save progress and respawn player at checkpoints  
**Scripts**: `CheckpointManager.cs`, `Checkpoint.cs`

**Features**:
- ✅ Multiple checkpoint support with ordering
- ✅ Visual activation feedback (flag animation, glow)
- ✅ Respawn effects and camera shake
- ✅ Progress tracking
- ✅ One-time or reusable checkpoints
- ✅ Auto-registration system

---

## 🛠️ Setup Instructions

### Hazard Setup

**1. Create Hazard GameObject**:
```
1. Create Empty GameObject → Name: "Spike_Hazard"
2. Add SpriteRenderer → Assign spike sprite
3. Add BoxCollider2D → IsTrigger: ✅ true
4. Add Hazard component
5. Configure Hazard settings
```

**2. Hazard Configuration**:
- **Hazard Type**: Spikes (or desired type)
- **Damage**: 1 (instant kill)
- **Respawn Player**: ✅ true
- **Screen Shake**: Intensity 0.3, Duration 0.2
- **Effects**: Assign particle system and death sound

**3. Layer Setup**:
- Create "Hazards" layer
- Assign hazard objects to this layer
- Configure Physics2D collision matrix if needed

### Pickup Setup

**1. Create Pickup GameObject**:
```
1. Create Empty GameObject → Name: "Coin_Pickup"
2. Add SpriteRenderer → Assign coin sprite
3. Add CircleCollider2D → IsTrigger: ✅ true
4. Add Pickup component
5. Configure Pickup settings
```

**2. Pickup Configuration**:
- **Pickup Type**: Coin (or desired type)
- **Value**: 100 points
- **Magnet Range**: 2f units
- **Bob Animation**: ✅ enabled
- **Effects**: Assign particle system and pickup sound

**3. Animation Settings**:
- **Bob Height**: 0.3f
- **Bob Speed**: 2f
- **Rotation Speed**: 90f degrees/second

### Enemy Setup

**1. Create Enemy GameObject**:
```
1. Create Empty GameObject → Name: "Walker_Enemy"
2. Add SpriteRenderer → Assign enemy sprite
3. Add CapsuleCollider2D → Size to fit sprite
4. Add Rigidbody2D → Freeze Rotation Z, Gravity Scale: 1
5. Add BasicEnemy component
6. Configure Enemy settings
```

**2. Enemy Configuration**:
- **Enemy Type**: Walker
- **Move Speed**: 2f
- **Health**: 1
- **Detection Range**: 6f units
- **Chase Speed**: 4f
- **Attack Range**: 1.5f units

**3. Patrol Setup**:
- **Patrol Distance**: 5f (for simple back-and-forth)
- **OR Use Patrol Points**: Create empty GameObjects as waypoints

**4. Visual Setup**:
- **Normal Color**: White
- **Aggro Color**: Red
- **Can Be Stomped On**: ✅ true (allows player to jump on enemy)

### Checkpoint Setup

**1. Create Checkpoint GameObject**:
```
1. Create Empty GameObject → Name: "Checkpoint_01"
2. Add SpriteRenderer → Assign flag sprite
3. Add BoxCollider2D → IsTrigger: ✅ true
4. Add Checkpoint component
5. Configure Checkpoint settings
```

**2. Checkpoint Configuration**:
- **Checkpoint Order**: 0, 1, 2, etc. (sequential)
- **Activate On Trigger**: ✅ true
- **One Time Use**: ✅ true
- **Spawn Point**: Auto-created or manually assign

**3. Visual Setup**:
- **Inactive Sprite**: Gray flag
- **Active Sprite**: Green/colored flag
- **Activation Glow**: Optional glow effect GameObject

**4. CheckpointManager Setup**:
```
1. Create Empty GameObject → Name: "CheckpointManager"
2. Add CheckpointManager component
3. Auto Find Player: ✅ true
4. Set Default Spawn Point
5. Configure respawn effects
```

---

## 🎮 Level Integration

### Scene Setup Order
1. **Place CheckpointManager** in scene (auto-finds checkpoints)
2. **Add Checkpoints** throughout level (numbered sequentially)
3. **Place Hazards** at strategic challenge points
4. **Scatter Pickups** for collection gameplay
5. **Position Enemies** with appropriate patrol areas

### Ground and Platform Setup
```
1. Create platforms with BoxCollider2D
2. Set Layer: "Ground"
3. Ensure PlayerData references Ground layer for ground checks
4. Test that all systems work with platform layouts
```

### Camera Bounds
```
1. Create GameObject → Name: "CameraBounds"
2. Add BoxCollider2D → IsTrigger: ✅ true
3. Size to cover entire level area
4. Tag: "CameraBounds"
5. CameraFollowController will auto-detect
```

---

## ⚙️ Testing Checklist

### Hazard System Testing
- [ ] Player respawns when touching hazards
- [ ] Particle effects play on contact
- [ ] Audio feedback works
- [ ] Camera shake triggers
- [ ] Haptic feedback on mobile

### Pickup System Testing
- [ ] Pickups magnetize when player approaches
- [ ] Collection triggers effects and sound
- [ ] Bob and rotation animations work
- [ ] Flash effect on magnet activation
- [ ] Score integration (if implemented)

### Enemy System Testing
- [ ] Enemies patrol correctly
- [ ] Player detection works within range
- [ ] Chase behavior activates properly
- [ ] Attack system functions with cooldown
- [ ] Stomping mechanic works (player bounces)
- [ ] Enemy death effects trigger

### Checkpoint System Testing
- [ ] Checkpoints activate when player touches them
- [ ] Visual feedback shows activation
- [ ] Player respawns at last activated checkpoint
- [ ] Respawn effects and camera shake work
- [ ] Progress tracking functions correctly

---

## 🎨 Visual Polish

### Particle Effects
- **Hazard Death**: Explosion or impact particles
- **Pickup Collection**: Sparkle or shine effect
- **Enemy Death**: Puff or disintegration effect
- **Checkpoint Activation**: Flag wave or magical glow
- **Player Respawn**: Spawn-in effect

### Audio Design
- **Hazards**: Sharp impact sound (metal clash, spike pierce)
- **Pickups**: Pleasant chime or coin sound
- **Enemies**: Grunt/growl for detection, attack sounds
- **Checkpoints**: Success chime or flag wave sound
- **Respawn**: Magical whoosh or teleport sound

### Animation Integration
- **Hazards**: Saw rotation, spike emergence, fire flicker
- **Pickups**: Continuous bob and spin
- **Enemies**: Walk cycle, attack animation, death sequence
- **Checkpoints**: Flag wave, activation pulse

---

## 🔧 Advanced Configuration

### Scriptable Object Integration
Create data assets for different configurations:
- `HazardData.asset` - Different hazard types and damages
- `PickupData.asset` - Various pickup values and effects
- `EnemyData.asset` - Enemy stats and behaviors
- `LevelData.asset` - Checkpoint progression and requirements

### Performance Optimization
- **Object Pooling**: For frequently spawned effects
- **LOD System**: Reduce enemy AI update rates when far from player
- **Culling**: Disable distant enemy behaviors
- **Effect Limiting**: Max particle systems active at once

### Mobile Considerations
- **Touch-Friendly**: Larger collision areas for pickups
- **Haptic Feedback**: Different intensities for different events
- **Visual Clarity**: High contrast colors for gameplay elements
- **Performance**: Optimized for 60 FPS on target devices

This system provides a complete gameplay foundation that's ready for the tutorial level and easily extensible for additional content.