# Core Assembly

This folder contains core gameplay systems and foundational scripts.

**Assembly Definition**: Core.asmdef
**Namespace**: WWIII.Core
**Dependencies**: Unity.InputSystem, Unity.Cinemachine

## Scripts in this assembly:
- PlayerController (main character controller)
- GameManager (core game state management)
- InputManager (unified input handling)
- CameraController (camera follow and effects)

## Assembly Definition Configuration:
```json
{
    "name": "WWIII.Core",
    "rootNamespace": "WWIII.Core",
    "references": [
        "Unity.InputSystem",
        "Unity.Cinemachine"
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