# Development Notes

## Architecture Decisions

### Input System
- **Decision**: Use new Input System as primary with "Both" fallback
- **Rationale**: Modern input handling, better gamepad support, mobile-ready
- **Implementation**: Touch overlay + Backbone mapping + desktop controls

### Rendering Pipeline
- **Decision**: Built-in render pipeline (not URP 2D)
- **Rationale**: Simpler for 2D, better mobile performance, fewer dependencies
- **Note**: Can migrate to URP 2D later if advanced 2D lighting needed

### Project Structure
- **Decision**: Conventional 2D layout with _Project namespace
- **Rationale**: Clear separation, scalable organization, team-friendly
- **Benefits**: Easy asset location, build optimization, version control

### Data-First Systems
- **Decision**: No hardcoded content, ScriptableObject-driven
- **Rationale**: Designer-friendly, data-driven development, easy balancing
- **Implementation**: Level definitions, enemy configs, item stats in data files

## Performance Optimizations

### Mobile-First Approach
- SpriteAtlases for texture batching
- Object pooling for frequent spawns
- Compressed audio formats
- Optimized fixed timestep
- GC allocation avoidance

### Target Platform Considerations
- iPhone 16+ resolution support
- Backbone controller integration
- Touch haptics implementation
- macOS keyboard/mouse + gamepad

## Asset Store Integration
- Platform Game Assets Ultimate: Primary 2D art source
- FREE Casual SFX: Sound effects library
- DOTween Pro: Animation system
- Odin: Enhanced inspector workflow

## Known Issues & Solutions
- CS1022 Error: Resolved via complete file recreation (encoding issue)
- Adaptive Performance Warning: Package removed as unnecessary for 2D

## Project Settings Configuration ✅

### Input System Setup
- **Active Input Handling**: Set to "Both" (Input Manager + Input System)
- **Control Schemes**: Keyboard&Mouse, Gamepad, Touch
- **Key Bindings**: WASD/Arrows + Space, Gamepad support, Touch overlay
- **InputManager.cs**: Unified input handling with event system

### Performance Optimization
- **Target Frame Rate**: 60 FPS (120 FPS for ProMotion displays)
- **Fixed Timestep**: 0.016666f (60 FPS physics)
- **VSync**: Disabled for mobile battery optimization
- **Graphics Jobs**: Enabled for multi-threaded rendering

### Mobile Settings
- **iOS Haptics**: Enabled with feedback types
- **ProMotion Support**: 120Hz on supported devices
- **Battery Optimization**: Dynamic frame rate adjustment
- **Audio**: Background mute option, compressed formats

### Quality Settings
- **Shadows**: Low-Medium resolution, 15-20f distance
- **Anti-Aliasing**: 0-2x for mobile performance
- **Texture Limit**: Optimized for mobile memory
- **Particle Budget**: 100 max systems

## Next Development Phases
1. ✅ Project architecture setup
2. ✅ Core systems and project settings
3. ✅ Scene management and UI foundations 
4. ✅ Player controller and mobile controls system
5. ✅ Core gameplay mechanics (hazards, pickups, enemies, checkpoints)
6. ✅ HUD and UI systems (pause, game over, notifications)
7. ✅ Audio system (music management, SFX pooling, environmental audio)
8. 🎯 Scene creation (Boot→MainMenu→LevelSelect→Tutorial)
9. Level editor tools and data systems
10. Polish and juice effects