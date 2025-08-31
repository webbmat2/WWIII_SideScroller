# Systems Assembly

This folder contains game systems and managers.

**Assembly Definition**: Systems.asmdef
**Namespace**: WWIII.Systems
**Dependencies**: WWIII.Core, WWIII.Data

## Scripts in this assembly:
- AudioManager (music and SFX)
- UIManager (menu and HUD systems)
- LevelManager (level flow and checkpoints)
- SaveSystem (progress tracking)

## Assembly Definition Configuration:
```json
{
    "name": "WWIII.Systems",
    "rootNamespace": "WWIII.Systems",
    "references": [
        "WWIII.Core",
        "WWIII.Data",
        "Unity.InputSystem"
    ],
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