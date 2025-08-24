# Level Design Implementation Guide

## Quick Setup Checklist

### 1. Scene Setup (5 minutes)
- [ ] Create new scene from template
- [ ] Add **GameStateManager** prefab to scene
- [ ] Add **AudioFXManager** prefab to scene  
- [ ] Add **PlaytestManager** prefab for data collection
- [ ] Set up Cinemachine camera with **CameraManager**

### 2. Player Setup (2 minutes)
- [ ] Place **Player** prefab at spawn point
- [ ] Add **PlayerConstraintsFix** component (critical!)
- [ ] Verify constraints are "FreezeRotation" only
- [ ] Set respawn point via PlayerController2D

### 3. Beat Map Implementation

#### Beat 1: Safe Spawn + Goal Peek
```
[Spawn] ─→ (coin) ─→ [wide platform] ─→ ... [Goal visible in distance]
```
**Components needed:**
- Player spawn point
- 1 coin as breadcrumb
- LevelExit at far right
- Wide platform for safety

#### Beat 2: Jump Primer  
```
[Platform A] ──(2-tile gap)── [Platform B]
                   ↑
               (coin arc)
```
**Implementation:**
```csharp
// In LevelBuilder.cs - use this setup
CreateGround(new Vector3(0, 0, 0));     // Platform A
CreateGround(new Vector3(4, 0, 0));     // Platform B (2-tile gap)
Instantiate(coinPrefab, new Vector3(2, 1.5f, 0));  // Arc hint
```

#### Beat 3: First Spikes (Low Stakes)
```
[Safe Platform] → [Single Spike] → [Checkpoint]
```
**Components:**
```csharp
CreateSpike(new Vector3(5, 0, 0));
Instantiate(checkpointPrefab, new Vector3(7, 0, 0));
```

#### Beat 4: Moving Platform Teach
```
[Gap] ← [Slow Moving Platform] → [Landing]
```
**Setup:**
- MovingPlatform with slow speed (2f)
- 3-tile gap
- Clear visual timing

#### Beat 5: Vertical Read  
```
    [Top Platform] ← (coin)
         ↑
[One-Way Platform] ← (coin)
         ↑  
[One-Way Platform] ← (coin)
         ↑
    [Start Platform]
```
**Use:** `CreateVerticalLadder(position, 5)` from LevelBuilder

#### Beat 6: Spike + Platform Mix
```
[Moving Platform] ↓↑ (passes near spikes)
     ↑               ↑
  [Spikes]      [Risk Coin]
```

#### Beat 7: Mini-Arena (Test)
```
[Flat Area with PatrolEnemy + TurretEnemy]
         ↓
   [Health Pickup] (reward)
```
**Implementation:**
```csharp
CreatePatrolSetup(arenaCenter);
Instantiate(turretEnemyPrefab, arenaCenter + Vector3.right * 5);
Instantiate(healthPickupPrefab, arenaCenter + Vector3.right * 8);
```

#### Beat 8: Secret Detour
```
[Breakable Wall] → [Coin Cache + Keycard]
```
**Use:** `CreateSecretCache(position)` from LevelBuilder

#### Beat 9: Chase Strip (Flow Payoff)
```
[Gap] → [Platform] → [Gap] → [Platform] → [Gap] → [Exit]
```
**Features:**
- 3-4 alternating gaps/platforms
- Screen shake on landings (automatic in PlayerController2D)
- Increasing pace/momentum

#### Beat 10: Exit Gate
```
[Clear Vista] → [LevelExit with coin requirement]
```

## 3. Collectibles Placement

### Coin Economy (20-30 total):
```csharp
// Cluster 1: Tutorial area (5 coins)
for(int i = 0; i < 5; i++) {
    Vector3 pos = startPos + Vector3.right * i * 2f + Vector3.up * 0.5f;
    Instantiate(coinPrefab, pos);
}

// Cluster 2: Vertical section (8 coins) 
CreateVerticalLadder(verticalStart, 8);

// Cluster 3: Arena reward (7 coins)
CreatePatrolSetup(arenaPos); // Already includes coins

// Secret cache (5-10 coins)
CreateSecretCache(secretPos);
```

### Health Pickup:
```csharp
// Place after mini-arena as reward
Vector3 medkitPos = arenaPos + Vector3.right * 10f;
Instantiate(healthPickupPrefab, medkitPos);
```

## 4. Checkpoint Strategy

### Rule: Place after "new + risk" beats
```csharp
// After first spikes
Instantiate(checkpointPrefab, postSpikesPos);

// After vertical climb
Instantiate(checkpointPrefab, topOfClimbPos);

// After mini-arena
Instantiate(checkpointPrefab, postArenaPos);
```

## 5. Audio/FX Integration

All systems auto-integrate with **AudioFXManager**:
- Jump/land sounds ✓
- Damage sounds ✓  
- Coin collection ✓
- Checkpoint activation ✓
- Screen shake on impacts ✓

## 6. Balancing Values

### Jump Distance Limits:
```csharp
// Max safe gap = player jump distance - 1 tile safety
float maxGap = 3.5f; // For jumpForce = 10f
float safeGap = maxGap - 1f; // = 2.5 tiles
```

### Platform Timing:
```csharp
// Moving platform speeds
float teachSpeed = 2f;     // First introduction
float normalSpeed = 4f;    // Standard gameplay  
float challengeSpeed = 6f; // Advanced sections
```

### Enemy Damage:
```csharp
// Conservative values for approachable first-time experience
patrolEnemyDamage = 1;     // Out of 3-5 max health
turretDamage = 1;
spikeDamage = 1;
```

## 7. Quick Build Commands

In scene, select **LevelBuilder** and use:

```csharp
// Context menu commands available:
[ContextMenu("Create Hazard Lane")]      // 3 spikes + reward coin
[ContextMenu("Create Patrol Setup")]     // Platform + enemy + coins
[ContextMenu("Create Vertical Ladder")]  // One-way platforms + coins
[ContextMenu("Create Secret Cache")]     // Breakable wall + treasure
```

## 8. Playtest Data Collection

**PlaytestManager** automatically tracks:
- Death positions (heatmap)
- Time to complete
- Coin collection rate
- Retry attempts
- Event timeline

**Data exports to:** `Application.persistentDataPath/Playtest_Session_[timestamp].json`

## 9. Common Issues & Fixes

### ❌ Player won't move:
- **Fix:** Add PlayerConstraintsFix component
- **Check:** Rigidbody2D constraints = "FreezeRotation" only

### ❌ Spikes don't damage:
- **Check:** Layer setup (Water/Hazard)
- **Check:** DamageOnTouch has correct settings
- **Fix:** Use Reset() method on DamageOnTouch

### ❌ Moving platforms stuck:
- **Check:** Constraints are NOT "FreezeAll"  
- **Check:** Points A and B are different
- **Fix:** Use Reset() method on MovingPlatform

### ❌ Coins don't collect:
- **Check:** CoinManager.Reset() called at game start
- **Check:** CoinUI subscribes to CoinManager events
- **Fix:** Add GameManager to scene

### ❌ No audio:
- **Fix:** Add AudioFXManager to scene
- **Check:** Audio clips assigned in AudioFXManager

## 10. Performance Tips

### Optimization:
- Use object pooling for projectiles (not implemented yet)
- Limit PlaytestManager data collection frequency
- Use LOD for distant decorative elements
- Batch similar operations in FixedUpdate

### Frame Rate:
- Target: 60 FPS (set in GameStateManager)
- VSync: Enabled by default
- Profiler: Monitor frame time during complex sections

## Final Checklist Before Playtest:

- [ ] Player moves and jumps correctly
- [ ] All hazards deal damage with proper knockback
- [ ] Checkpoints activate and respawn works
- [ ] Coins collect and UI updates
- [ ] Camera follows smoothly
- [ ] Audio plays for all major actions
- [ ] No console errors
- [ ] PlaytestManager is recording data

**Ready for first playtest! 🎮**