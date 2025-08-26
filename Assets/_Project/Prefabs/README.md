# Prefabs Directory

Contains all reusable GameObjects:

## Structure
- **Player/** - Player character prefabs
- **Enemies/** - Enemy types and variants
- **Hazards/** - Environmental dangers, moving platforms
- **Pickups/** - Collectibles, power-ups, items
- **UI/** - UI elements, buttons, panels
- **VFX/** - Visual effects, particles
- **Audio/** - Audio sources, ambient sounds
- **Level/** - Level pieces, platforms, decorations

## Guidelines
- All prefabs should be mobile-optimized
- Use object pooling for frequently spawned items
- Include proper colliders and rigidbodies
- Follow naming convention: Category_SpecificName_Variant