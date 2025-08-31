# Data Assembly

This folder contains ScriptableObject definitions and data structures.

**Assembly Definition**: Data.asmdef
**Namespace**: WWIII.Data
**Dependencies**: None (pure data)

## Scripts in this assembly:
- LevelDef (level configuration data)
- NarrativeDef (story beats and dialog)
- CollectibleSetDef (collectible rewards)
- PowerUpDef (power-up configurations)
- WeaponDef (weapon stats and behaviors)
- EnemyDef (enemy archetypes and AI)
- FamilyMemberDef (character stats and abilities)

## Assembly Definition Configuration:
```json
{
    "name": "WWIII.Data",
    "rootNamespace": "WWIII.Data",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```