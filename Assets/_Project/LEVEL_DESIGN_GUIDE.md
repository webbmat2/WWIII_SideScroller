# 🎮 2D Platformer Level Design Guide

## 📏 **Proven Design Measurements**

### **Jump Distances (Based on Mario/Mega Man)**
- **Beginner jumps:** 2-3 tiles wide
- **Standard jumps:** 3-4 tiles wide  
- **Advanced jumps:** 4-5 tiles wide
- **Maximum safe jump:** 5-6 tiles wide

### **Platform Spacing**
- **Landing platforms:** Minimum 2 tiles wide
- **Safe rest areas:** 4-6 tiles wide
- **Between challenges:** Every 3-4 jumps

### **Vertical Heights**
- **Basic jump:** 3-4 tiles high
- **Running jump:** 4-5 tiles high
- **Maximum jump:** 5-6 tiles high

## 🎯 **Level 1 Tutorial Layout**

```
Level 1: "First Steps" - Horizontal Tutorial
===========================================

     [💰]     [💰]      [🏁]
                   ████████████
    ████   ████    █     █     █
Player  █    █       █     █████
█████████    █         █

Measurements:
- Gap 1: 2 tiles (easy)
- Gap 2: 3 tiles (standard) 
- Gap 3: 4 tiles (challenging)
- Platforms: 2-4 tiles wide each
```

## 🏗️ **Classic Platformer Structures**

### **Mario-Style Elements:**
1. **Starting safe zone** (6+ tiles wide)
2. **Progressive difficulty** (start easy)
3. **Visual telegraphing** (clear landing spots)
4. **Forgiveness mechanics** (coyote time)

### **Mega Man-Style Elements:**
1. **Precise jumps** with clear spacing
2. **Pattern-based challenges**
3. **Safe reset points** every 20-30 seconds

## 🎨 **Visual Design Principles**

### **Readability:**
- **Platforms:** Bright, contrasting colors
- **Background:** Muted, non-distracting
- **Hazards:** Red/orange warning colors
- **Collectibles:** Yellow/gold attention-grabbing

### **Z-Depth Layers:**
- **Background:** -2 to -1
- **Platforms:** 0
- **Player:** +1  
- **Collectibles:** +2
- **UI:** +10

## 🎮 **Controls & Feel Reference**

### **Movement Values (Mario-inspired):**
- **Walk Speed:** 6-8 units/sec
- **Jump Height:** 3-4 units
- **Gravity:** -25 to -30 units/sec²
- **Coyote Time:** 0.1-0.15 seconds
- **Jump Buffer:** 0.1-0.2 seconds

### **Best Practices:**
1. **Test every jump** yourself
2. **No blind leaps** (player can see landing)
3. **Multiple solutions** when possible
4. **Clear visual language** throughout

## 📚 **Study These Games:**

### **2D Platformer Classics:**
- **Super Mario Bros 1-3** (horizontal progression)
- **Mega Man 2** (precise jumping)
- **Sonic 1** (momentum-based movement)
- **Celeste** (modern tight controls)
- **Hollow Knight** (exploration platforming)

### **Modern Indies to Study:**
- **A Hat in Time** (3D Mario-style in 2D)
- **Ori and the Blind Forest** (fluid movement)
- **Super Meat Boy** (ultra-precise platforming)
- **Rayman Legends** (creative level design)

## 🛠️ **Unity Setup Checklist:**

### **Player Setup:**
- ✅ **Kinematic Rigidbody2D** (avoid constraint bugs)
- ✅ **Custom gravity system**
- ✅ **Box Collider 2D** (0.8x1.8 units)
- ✅ **Ground detection** (multi-point)

### **Level Setup:**
- ✅ **Ground layer** (Layer 3)
- ✅ **Proper Z-ordering** (platforms behind player)
- ✅ **Consistent tile grid** (1 Unity unit = 1 tile)
- ✅ **Camera bounds** (prevent showing outside level)

### **Physics Settings:**
- ✅ **Fixed Timestep:** 0.02 (50 FPS)
- ✅ **Gravity:** -9.81 (or custom in script)
- ✅ **Collision Detection:** Discrete
- ✅ **Layer Collision Matrix** properly set

---

*This guide is based on 40+ years of proven 2D platformer design principles from arcade classics to modern indies.*