# Audio System Setup Guide

## Overview
Complete guide for setting up the professional Audio System for WWIII SideScroller. This system provides centralized audio management with music, SFX, and UI audio control.

## 🎵 Audio System Components

### 1. AudioManager System
**Purpose**: Centralized audio control with music management and SFX pooling  
**Script**: `AudioManager.cs`

**Features**:
- ✅ **Music Management**: Crossfading, looping, volume control
- ✅ **SFX Pool System**: Efficient audio source pooling for performance
- ✅ **Audio Mixer Integration**: Professional volume control with dB scaling
- ✅ **Persistent Settings**: PlayerPrefs integration for audio preferences
- ✅ **Mobile Optimizations**: Automatic pause/resume on focus changes
- ✅ **Convenience Methods**: Easy-to-use methods for common sounds

### 2. AudioTrigger System
**Purpose**: Location-based audio triggers for environmental sounds  
**Script**: `AudioTrigger.cs`

**Features**:
- ✅ **Flexible Triggering**: Trigger enter/exit, collision, or manual activation
- ✅ **3D Positional Audio**: Spatial audio for immersive experience
- ✅ **Random Variations**: Pitch and volume randomization
- ✅ **Cooldown System**: Prevents audio spam
- ✅ **Tag Filtering**: Only trigger for specific object types

### 3. Integrated Game Audio
**Purpose**: Seamless integration with existing gameplay systems

**Audio Integration Points**:
- ✅ **Player Actions**: Jump, land, movement
- ✅ **Gameplay Elements**: Pickup collection, checkpoint activation
- ✅ **UI Interactions**: Button clicks, menu transitions
- ✅ **Combat System**: Enemy attacks, deaths, damage
- ✅ **Environmental**: Hazard impacts, ambient sounds

---

## 🛠️ AudioManager Setup

### 1. Create AudioManager GameObject
```
1. Create Empty GameObject → Name: "AudioManager"
2. Add AudioManager component
3. Configure DontDestroyOnLoad (automatic)
4. Position in scene hierarchy
```

### 2. Audio Mixer Configuration (Recommended)
```
1. Create AudioMixer asset → Name: "GameAudioMixer"
2. Create groups:
   - Master (root)
   ├── Music
   └── SFX
3. Add Volume parameters:
   - MasterVolume (exposed parameter)
   - MusicVolume (exposed parameter)  
   - SFXVolume (exposed parameter)
4. Assign mixer to AudioManager
```

### 3. AudioManager Configuration

**Music Settings**:
- **Background Music Array**: Assign music clips for different scenes/moods
- **Music Volume**: Default 0.7 (70%)
- **Loop Music**: ✅ enabled for seamless background music
- **Fade Time**: 1.0s for smooth transitions

**SFX Pool Settings**:
- **SFX Pool Size**: 10 (sufficient for most scenarios)
- **SFX Volume**: 1.0 (100%)
- **SFX Source Prefab**: Optional custom AudioSource setup

**Audio Clips Assignment**:
```csharp
// UI Audio
[SerializeField] private AudioClip buttonClickSound;
[SerializeField] private AudioClip menuOpenSound;
[SerializeField] private AudioClip notificationSound;

// Gameplay Audio  
[SerializeField] private AudioClip playerJumpSound;
[SerializeField] private AudioClip coinCollectSound;
[SerializeField] private AudioClip checkpointSound;
[SerializeField] private AudioClip hazardHitSound;
```

---

## 🎮 Integration with Existing Systems

### Player Controller Integration
```csharp
// Jump sound integration (already implemented)
private void ExecuteJump()
{
    // ... jump logic ...
    
    // Play jump sound
    if (WWIII.Audio.AudioManager.Instance != null)
    {
        WWIII.Audio.AudioManager.Instance.PlayPlayerJump();
    }
}

// Landing sound integration (already implemented)
private void OnLanding()
{
    // ... landing logic ...
    
    // Play landing sound
    if (WWIII.Audio.AudioManager.Instance != null)
    {
        WWIII.Audio.AudioManager.Instance.PlayPlayerLand();
    }
}
```

### Pickup System Integration
```csharp
// Coin collection sound (already implemented)
private void PlayEffects()
{
    // Play through AudioManager if available
    if (WWIII.Audio.AudioManager.Instance != null)
    {
        WWIII.Audio.AudioManager.Instance.PlayCoinCollect();
    }
    // Fallback to local audio source
}
```

### UI System Integration
```csharp
// Button clicks in menus
private void OnButtonClick()
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayButtonClick();
    }
}

// Menu transitions
private void ShowPauseMenu()
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayMenuOpen();
    }
}
```

---

## 🎵 Music Management

### Scene-Based Music System
```csharp
// In scene loading/transition code
public void LoadLevel(string sceneName)
{
    // Play appropriate music for scene
    switch (sceneName)
    {
        case "MainMenu":
            AudioManager.Instance.PlayMusic("MenuTheme", fadeIn: true);
            break;
        case "L1_Tutorial":
            AudioManager.Instance.PlayMusic("GameplayTheme", fadeIn: true);
            break;
        case "Boss_Level":
            AudioManager.Instance.PlayMusic("BossTheme", fadeIn: true);
            break;
    }
}
```

### Dynamic Music Control
```csharp
// Combat music transitions
public void OnCombatStart()
{
    AudioManager.Instance.PlayMusic("CombatTheme", fadeIn: true);
}

public void OnCombatEnd()
{
    AudioManager.Instance.PlayMusic("ExploreTheme", fadeIn: true);
}

// Pause/Resume music
public void OnGamePaused()
{
    AudioManager.Instance.PauseMusic();
}

public void OnGameResumed()
{
    AudioManager.Instance.ResumeMusic();
}
```

---

## 🛠️ AudioTrigger Setup

### 1. Create Audio Trigger Zone
```
1. Create Empty GameObject → Name: "AudioTrigger_Ambient"
2. Add BoxCollider2D or CircleCollider2D
3. Set IsTrigger: ✅ true
4. Add AudioTrigger component
5. Configure audio settings
```

### 2. AudioTrigger Configuration

**Basic Settings**:
- **Audio Clip**: Assign the sound to play
- **Volume**: 1.0 (full volume)
- **Pitch**: 1.0 (normal pitch)
- **Is 3D**: ✅ for spatial audio, ❌ for global sounds

**Trigger Settings**:
- **Trigger Tags**: ["Player"] (only trigger for player)
- **Play On Trigger Enter**: ✅ for entry sounds
- **Play On Trigger Exit**: ✅ for exit sounds
- **One Time Use**: ✅ for special events, ❌ for repeatable

**Random Variations**:
- **Randomize Pitch**: ✅ Range: 0.8 - 1.2
- **Randomize Volume**: ✅ Range: 0.8 - 1.0

### 3. Common AudioTrigger Use Cases

**Ambient Environment**:
```
- Wind sounds in outdoor areas
- Water dripping in caves
- Machinery hums in industrial areas
- Bird chirping in forest levels
```

**Interactive Elements**:
```
- Door opening/closing sounds
- Switch activation sounds
- Platform movement audio
- Secret area discovery sounds
```

**Atmospheric Zones**:
```
- Echo effects in large chambers
- Muffled sounds underwater
- Reverb in cathedral-like spaces
- Combat music triggers
```

---

## 🎛️ Volume Control Integration

### Settings Menu Integration
```csharp
// In PauseMenuManager or SettingsMenu
private void OnMasterVolumeChanged(float value)
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }
}

private void OnMusicVolumeChanged(float value)
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }
}

private void OnSFXVolumeChanged(float value)
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}
```

### PlayerPrefs Persistence
```csharp
// Settings are automatically saved/loaded
// Manual save if needed:
AudioManager.Instance.SaveAudioSettings();

// Get current volumes:
float masterVol = AudioManager.Instance.GetMasterVolume();
float musicVol = AudioManager.Instance.GetMusicVolume();
float sfxVol = AudioManager.Instance.GetSFXVolume();
```

---

## 📱 Mobile Optimizations

### Automatic Pause/Resume
```csharp
// Automatically handled by AudioManager
private void OnApplicationPause(bool pauseStatus)
{
    if (pauseStatus)
        PauseMusic();
    else
        ResumeMusic();
}

private void OnApplicationFocus(bool hasFocus)
{
    if (!hasFocus)
        PauseMusic();
    else
        ResumeMusic();
}
```

### Performance Optimizations
- **SFX Pooling**: Reuses AudioSource components for efficiency
- **Automatic Cleanup**: Stops and reuses audio sources when sounds complete
- **Memory Management**: Clips loaded on-demand, not preloaded
- **3D Audio Culling**: Spatial audio only when needed

---

## 🎨 Audio Asset Organization

### Recommended File Structure
```
Assets/_Project/Audio/
├── Music/
│   ├── MenuTheme.mp3
│   ├── GameplayTheme.mp3
│   ├── BossTheme.mp3
│   └── VictoryTheme.mp3
├── SFX/
│   ├── Player/
│   │   ├── Jump.wav
│   │   ├── Land.wav
│   │   └── Footstep.wav
│   ├── UI/
│   │   ├── ButtonClick.wav
│   │   ├── MenuOpen.wav
│   │   └── Notification.wav
│   ├── Gameplay/
│   │   ├── CoinCollect.wav
│   │   ├── Checkpoint.wav
│   │   └── HazardHit.wav
│   └── Environment/
│       ├── Wind.wav
│       ├── Water.wav
│       └── Machinery.wav
└── Mixers/
    └── GameAudioMixer.mixer
```

### Audio Import Settings
**Music Files** (.mp3, .ogg):
- Load Type: **Streaming**
- Compression: **Vorbis** (good balance)
- Quality: **70%** (adjust based on file size needs)

**Short SFX** (.wav, .ogg):
- Load Type: **Decompress On Load**
- Compression: **PCM** (for very short clips) or **ADPCM**
- Quality: **100%** (small files, max quality)

**Long SFX/Ambient** (.wav, .ogg):
- Load Type: **Compressed In Memory**
- Compression: **Vorbis**
- Quality: **70-90%**

---

## 🔧 Testing Checklist

### AudioManager Testing
- [ ] Music plays and loops correctly
- [ ] Music crossfading works smoothly
- [ ] SFX play without lag or cutting off
- [ ] Volume controls affect appropriate audio groups
- [ ] Settings persist between sessions
- [ ] No audio when volume set to 0

### AudioTrigger Testing  
- [ ] Triggers activate with correct game objects
- [ ] 3D audio positioning works correctly
- [ ] Random variations provide good variety
- [ ] Cooldown prevents audio spam
- [ ] One-time triggers work only once

### Integration Testing
- [ ] Player jump/land sounds play correctly
- [ ] Pickup collection audio works
- [ ] Checkpoint activation sounds trigger
- [ ] UI button clicks provide feedback
- [ ] Menu music transitions smoothly

### Mobile Testing
- [ ] Audio pauses when app loses focus
- [ ] Audio resumes when app regains focus
- [ ] No audio glitches on device rotation
- [ ] Performance remains stable with multiple SFX

This comprehensive audio system provides professional-grade audio management that enhances the gameplay experience while maintaining optimal performance on mobile devices.