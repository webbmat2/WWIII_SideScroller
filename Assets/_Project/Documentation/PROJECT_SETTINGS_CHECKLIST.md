# Project Settings Configuration Checklist

## Required Manual Settings in Unity Editor

### 1. Player Settings (Edit → Project Settings → Player)

#### **Configuration**
- [ ] **Active Input Handling**: Set to "Both"
- [ ] **Color Space**: Linear (for better mobile performance)
- [ ] **Auto Graphics API**: Unchecked
- [ ] **Graphics APIs**: Remove Vulkan if present, keep Metal/OpenGLES3

#### **iOS Settings**
- [ ] **Target Device**: iPhone + iPad
- [ ] **Target minimum iOS Version**: iOS 14.0+
- [ ] **Target SDK**: Device SDK
- [ ] **Architecture**: ARM64
- [ ] **Enable ProMotion Support**: ✅ Checked
- [ ] **Automatically add capabilities**: ✅ Checked
- [ ] **Requires ARKit support**: ❌ Unchecked

#### **macOS Settings**
- [ ] **Target macOS Version**: macOS 11.0+
- [ ] **Architecture**: Apple Silicon + Intel
- [ ] **Create Xcode Project**: ✅ Checked

### 2. Input System Package (Edit → Project Settings → Input System Package)

#### **Create Input Actions Asset**
1. **Right-click in Project** → Create → Input Actions
2. **Name it**: "InputActions"
3. **Location**: `/Assets/_Project/Settings/`
4. **Configure Action Maps:**
   - **Player** (Move, Jump, Pause)
   - **UI** (Navigate, Submit, Cancel)

#### **Control Schemes**
- [ ] **Keyboard&Mouse**: Keyboard + Mouse
- [ ] **Gamepad**: Any Gamepad
- [ ] **Touch**: Touchscreen

### 3. Quality Settings (Edit → Project Settings → Quality)

#### **Mobile Quality Level**
- [ ] **Texture Quality**: Full Res
- [ ] **Anisotropic Textures**: Per Texture
- [ ] **Anti Aliasing**: 2x Multi Sampling
- [ ] **Soft Particles**: ✅ Enabled
- [ ] **Shadow Resolution**: Medium Resolution
- [ ] **Shadow Distance**: 20
- [ ] **Shadow Cascades**: No Cascades

### 4. Physics 2D Settings (Edit → Project Settings → Physics 2D)

#### **Performance Settings**
- [ ] **Velocity Iterations**: 8
- [ ] **Position Iterations**: 3
- [ ] **Velocity Threshold**: 1
- [ ] **Max Linear Correction**: 0.2
- [ ] **Max Angular Correction**: 8
- [ ] **Max Translation Speed**: 100
- [ ] **Max Rotation Speed**: 360

#### **Layer Collision Matrix**
Configure layer interactions:
- [ ] **Player** vs **Ground**: ✅
- [ ] **Player** vs **Enemies**: ✅  
- [ ] **Player** vs **Pickups**: ✅
- [ ] **Enemies** vs **Ground**: ✅
- [ ] **UI** vs **All**: ❌

### 5. Audio Settings (Edit → Project Settings → Audio)

#### **Audio Configuration**
- [ ] **DSP Buffer Size**: Best Performance
- [ ] **Sample Rate**: 44100 Hz
- [ ] **Audio Compression Format**: Vorbis/AAC
- [ ] **Force iOS Speakers when Recording**: ✅
- [ ] **Prepare iOS for Recording**: ❌

### 6. Time Settings (Edit → Project Settings → Time)

#### **Fixed Timestep Optimization**
- [ ] **Fixed Timestep**: 0.016666 (60 FPS)
- [ ] **Maximum Allowed Timestep**: 0.1
- [ ] **Time Scale**: 1
- [ ] **Maximum Particle Timestep**: 0.03

### 7. Graphics Settings (Edit → Project Settings → Graphics)

#### **Built-in Render Pipeline**
- [ ] **Scriptable Render Pipeline Settings**: None (Built-in)
- [ ] **Camera-Relative Rendering**: ✅ Enabled
- [ ] **Transparency Sort Mode**: Default
- [ ] **Transparency Sort Axis**: (0, 0, 1)

#### **Shader Stripping**
- [ ] **Lightmap Modes**: Manual selection
- [ ] **Fog Modes**: Manual selection
- [ ] **Instancing Variants**: Strip All

### 8. XR Plug-in Management (Edit → Project Settings → XR Plug-in Management)

#### **Disable XR (Not needed for 2D game)**
- [ ] **Initialize XR on Startup**: ❌ Unchecked
- [ ] **All Providers**: ❌ Unchecked

## Post-Configuration Steps

### 1. Create GameManager GameObject
1. **Create Empty GameObject** in scene
2. **Name**: "GameManager"
3. **Add Components**: GameManager, InputManager, MobileOptimizer
4. **Make Prefab**: Save to `/Assets/_Project/Prefabs/Core/`

### 2. Configure Input Actions Asset
1. **Open InputActions asset** created earlier
2. **Set as Project-wide Actions Asset** in Input System Package settings
3. **Generate C# Class**: Enable auto-generation

### 3. Test Configuration
1. **Build for iOS** (test build)
2. **Build for macOS** (test build)  
3. **Check Console** for zero errors/warnings
4. **Test Input** in Play mode

## Verification Checklist

- [ ] ✅ Zero compilation errors
- [ ] ✅ Zero warnings in console  
- [ ] ✅ iOS build succeeds
- [ ] ✅ macOS build succeeds
- [ ] ✅ Input responds in Play mode
- [ ] ✅ 60 FPS in empty scene
- [ ] ✅ Mobile optimizations active

## Next Steps After Configuration

1. **Create Core Scenes** (Boot, MainMenu, LevelSelect)
2. **Implement Player Controller** with Input System
3. **Set up Touch UI Overlay** for mobile
4. **Configure Gamepad Support** for Backbone/MFi