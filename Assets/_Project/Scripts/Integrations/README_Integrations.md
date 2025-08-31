# Integrations Assembly

This folder contains adapters and integrations for third-party assets.

**Assembly Definition**: Integrations.asmdef
**Namespace**: WWIII.Integrations
**Dependencies**: WWIII.Core, WWIII.Data, MoreMountains.CorgiEngine, DOTween

## Scripts in this assembly:
- CorgiEngineAdapter (adapt data to Corgi systems)
- TexturePackerIntegration (sprite atlas utilities)
- DOTweenEffects (animation and juice effects)
- OdinEditorTools (custom inspectors)

## Assembly Definition Configuration:
```json
{
    "name": "WWIII.Integrations",
    "rootNamespace": "WWIII.Integrations",
    "references": [
        "WWIII.Core",
        "WWIII.Data", 
        "MoreMountains.CorgiEngine",
        "DOTween.Modules"
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