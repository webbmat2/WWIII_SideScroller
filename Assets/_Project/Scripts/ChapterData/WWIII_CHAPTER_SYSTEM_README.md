# WWIII Side-Scroller Chapter System

## Overview

This document describes the complete chapter system implementation for WWIII_SideScroller, a side-scrolling adventure game following Kristen and Matt's journey across various locations in 2024.

## Game Story & Chapters

### 1. **Meadowbrook Park (Northville)** - Tutorial Chapter
- **Setting**: Local park in Northville (Matt's hometown)
- **Story**: Tutorial level + Purple Pig boss fight
- **Mechanics**: Basic movement, hose mechanics, Slip-n-Slide gates
- **Vehicle Cameo**: 1996 GMC Jimmy (Burgundy)
- **Boss**: Purple Pig (requires Matt to grab, hose to make vulnerable)
- **Collectibles**: 5x Golden Fried ush signs

### 2. **Torch Lake** - Travel Chapter
- **Setting**: Cottage to party store journey
- **Story**: Transition and exploration
- **Mechanics**: Water hazards, swimming mechanics
- **Collectibles**: 5x Lake stones

### 3. **Notre Dame** - Campus Chapter  
- **Setting**: University campus traversal
- **Story**: Campus exploration and navigation
- **Mechanics**: Platform navigation, students as NPCs
- **Collectibles**: 5x Campus items

### 4. **High School** - Memory Chapter
- **Setting**: High school environment
- **Story**: Flashback/memory sequence
- **Mechanics**: Hallway navigation, lockers
- **Vehicle Cameo**: 1989 Jeep Wrangler
- **Collectibles**: 5x School memorabilia

### 5. **Philadelphia** - Urban Chapter
- **Setting**: City environment
- **Story**: Urban challenges and exploration
- **Mechanics**: Urban hazards, city navigation  
- **Boss**: Hamburger Helper Glove villain
- **Collectibles**: 5x Poker chips

### 6. **Parson's Chicken** - Restaurant Chapter
- **Setting**: Restaurant to airport journey
- **Story**: Food service and travel preparation
- **Mechanics**: Service challenges, time pressure
- **Collectibles**: 5x Restaurant items

### 7. **Costa Rica** - Final Chapter
- **Setting**: Jungle environment ending at Casa Lumpusita
- **Story**: Final destination and climax
- **Mechanics**: Jungle hazards (jaguars, spiders), Chiliguaro power-up
- **Boss**: Araña Reina (Spider Queen)
- **Power-ups**: Chiliguaro (grants bouncing fireballs)
- **Collectibles**: 5x Jungle artifacts

## Architecture Overview

### Core Components

#### 1. **ChapterManager** - Central System Controller
```csharp
// Singleton that manages the entire chapter system
ChapterManager.Instance.LoadChapter("meadowbrook-park");
```

#### 2. **ChapterData** - ScriptableObject Configuration
```csharp
[CreateAssetMenu(fileName = "Chapter", menuName = "WWIII/Chapter Data")]
public class ChapterData : ScriptableObject
```

#### 3. **PowerUpType** - Enumeration System
```csharp
public enum PowerUpType
{
    None, Hose, Chiliguaro, CherryPie, SmartJim, BeefJerky, CheeseBall
}
```

### Player System (Modular)

#### 1. **PlayerController** - Main Coordinator
- Orchestrates all player components
- Maintains backward compatibility
- Validates component setup

#### 2. **PlayerMovement** - Physics & Controls
- WASD/Arrow key movement
- Jump with variable height
- Crouch/duck mechanics
- Ground detection with coyote time
- Platform attachment system

#### 3. **PlayerHealth** - Damage & Respawn System
- Health management (default 3 HP)
- Invulnerability frames
- Hit-stun mechanics
- Respawn system with checkpoints
- Visual feedback (flashing)

#### 4. **PlayerAbilities** - Power-up System
- Hose mechanics (Slip-n-Slide gates, Purple Pig)
- Chiliguaro fireballs (bouncing projectiles)
- Power-up management per chapter
- Damage-based power-up loss

### Gameplay Mechanics

#### Boss Fights

**Purple Pig Boss (Meadowbrook Park)**
- **Mechanic**: Matt NPC must grab pig to make vulnerable
- **Vulnerability**: Only when grabbed by Matt AND hit by hose
- **States**: Pig form (invulnerable, mobile) ↔ Human form (vulnerable, stationary)
- **Defeat**: 3 hits in human form while grabbed

**Hamburger Helper Glove (Philadelphia)**  
- **Mechanic**: Traditional boss fight patterns
- **Attacks**: Glove-based attacks and movements

**Araña Reina/Spider Queen (Costa Rica)**
- **Mechanic**: Final boss with multiple phases
- **Environment**: Jungle setting with hazards
- **Reward**: Completion leads to Casa Lumpusita

#### Environmental Systems

**Slip-n-Slide Gates**
- **Dry State**: Solid collision, blocks player
- **Wet State**: Trigger collision, player slips through
- **Activation**: Player hose ability
- **Duration**: Configurable (default 10 seconds)

**Hazards & Damage**
- **System**: DamageOnTouch component
- **Effects**: Health loss, knockback, hit-stun
- **Integration**: Works with both old and new player systems

**Collectibles**
- **Per Chapter**: 5 collectibles with unique themes
- **Visual**: Auto-configures based on chapter data
- **Animation**: Bobbing movement for visibility
- **Progress**: Tracked by ChapterManager

#### Power-up System

**Hose (Default in most chapters)**
- **Function**: Wets Slip-n-Slide gates, damages Purple Pig
- **Range**: 3 units forward
- **Cooldown**: None (spam-friendly)

**Chiliguaro (Costa Rica exclusive)**
- **Function**: Bouncing fireballs
- **Bounces**: 3 per fireball (configurable)
- **Damage**: 1 per hit
- **Loss**: Removed when player takes damage

**Stat Boosts (Future expansion)**
- Cherry Pie: +1 Max HP
- Smart Jim: +Speed/Knowledge  
- Beef Jerky: +Damage
- Cheese Ball: +Defense

### UI System

#### GameHUD - Primary Interface
- **Health Display**: Hearts or HP counter
- **Collectibles**: Current/Total count per chapter
- **Abilities**: Shows current active ability
- **Chiliguaro Status**: Special indication when active

#### Health UI Options
- **HealthUI**: Simple text-based display
- **AdvancedHealthUI**: Animated heart containers with damage effects

### Scene Management

#### Scene Structure (All 7 chapters)
- **Basic Setup**: Camera, Player, Ground, UI Canvas
- **Chapter-Specific**: Bosses, hazards, vehicle cameos
- **Collectibles**: 5 per chapter, auto-configured
- **Checkpoints**: Respawn point management

#### Layer System
- **Ground**: Tilemap collision layer
- **Water/Hazard**: Damage-dealing objects
- **Player**: Player character layer
- **UI**: Interface elements
- **Default**: Collectibles and misc objects

## Implementation Details

### File Structure
```
Assets/_Project/Scripts/
├── ChapterData/                    # ScriptableObject assets
│   ├── WWIII_CHAPTER_SYSTEM_README.md
│   └── [7 Chapter Assets].asset
├── Player/                         # Modular player system
│   ├── PlayerController.cs        # Main coordinator
│   ├── PlayerMovement.cs          # Physics & input
│   ├── PlayerAbilities.cs         # Power-up system
│   └── ChiliguaroFireball.cs      # Projectile mechanics
├── Environment/                    # Level mechanics
│   └── SlipNSlideGate.cs          # Gate system
├── Enemies/                        # Boss implementations
│   └── PurplePigBoss.cs           # First boss
├── UI/                            # Interface systems
│   ├── GameHUD.cs                 # Primary HUD
│   └── AdvancedHealthUI.cs        # Animated health
├── Editor/                        # Development tools
│   ├── ChapterDataCreator.cs      # Asset creation
│   └── SceneCreator.cs            # Scene generation
├── ChapterManager.cs              # Core system
├── PowerUpPickup.cs               # Power-up items
├── Collectible.cs                 # Collectible system
├── DamageOnTouch.cs               # Hazard system
├── CheckpointTrigger.cs           # Respawn system
├── LayerSetup.cs                  # Physics layers
├── WWIII_Validator.cs             # Quality assurance
└── PlayerHealth2D.cs              # Legacy compatibility
```

### Editor Tools

#### Chapter Data Creator
- **Purpose**: Generate all 7 chapter ScriptableObjects
- **Location**: WWIII/Create Chapter Data Assets
- **Output**: Pre-configured chapter assets

#### Scene Creator  
- **Purpose**: Generate basic scene templates
- **Location**: WWIII/Create Chapter Scenes  
- **Features**: Auto-setup with proper components

#### WWIII Validator
- **Purpose**: Comprehensive scene validation
- **Features**: Component checks, physics validation, gameplay tests
- **Auto-fix**: Automatically resolves common issues

## Getting Started

### Quick Setup (New Scene)
1. Create empty scene
2. Add WWIII_Validator component to any GameObject
3. Run validation - it will auto-create missing components
4. Use Chapter Data Creator to generate chapter assets
5. Test with WWIII/Create Chapter Scenes for templates

### Existing Scene Integration
1. Add ChapterManager to scene
2. Replace PlayerController2D with new PlayerController
3. Update HealthUI to use PlayerHealth
4. Run WWIII_Validator for verification

### Testing Checklist
- [ ] Player movement (WASD/Arrows)
- [ ] Jump mechanics (Space/W/Up)
- [ ] Crouch mechanics (S/Down)  
- [ ] Health system (damage/respawn)
- [ ] Hose ability (X key)
- [ ] Collectible gathering
- [ ] Checkpoint system
- [ ] Camera following
- [ ] UI updates
- [ ] Chapter transitions

## Power-Up Integration

### Adding New Power-ups
1. Add enum value to PowerUpType
2. Implement logic in PlayerAbilities.ApplyPowerUp()
3. Create PowerUpPickup with appropriate type
4. Configure in ChapterData.availablePowerUps

### Custom Boss Implementation
1. Inherit from MonoBehaviour
2. Implement IDamageable interface
3. Handle state management
4. Call ChapterManager.CompleteChapter() on defeat

## Backward Compatibility

The system maintains compatibility with existing code:
- **DamageOnTouch**: Works with both old and new player systems
- **PlayerController2D**: Still functional alongside new system
- **CollectibleManager**: Integrates with new chapter collectibles
- **Scene Structure**: Existing scenes work with minimal changes

## Performance Considerations

- **Singleton Pattern**: ChapterManager for efficient access
- **Event-Driven UI**: Minimizes update loops
- **Component Caching**: Reduces FindObjectOfType calls
- **Layer Masking**: Optimized collision detection
- **ScriptableObjects**: Memory-efficient chapter data

## Future Expansions

### Potential Additions
- **Save System**: Chapter progress persistence
- **Stat System**: Numerical character progression  
- **Achievement System**: Chapter-based unlocks
- **Audio Manager**: Chapter-specific music/SFX
- **Cutscene System**: Story sequence management
- **Mini-games**: Chapter-specific gameplay varieties

### Story Extensions
- **Prequel Chapters**: Earlier timeline events
- **Parallel Stories**: Alternative character perspectives
- **Post-Game Content**: Extended adventures
- **Season System**: Recurring chapter themes

---

## Conclusion

This chapter system provides a robust foundation for WWIII_SideScroller's narrative-driven gameplay. The modular architecture supports both current requirements and future expansions while maintaining compatibility with existing project assets.

For technical support or feature requests, consult the WWIII_Validator tool and the comprehensive editor utilities provided with this system.