# **BEZI PROJECT RULES - UNITY AI ASSISTANT FOCUSED**

## **🤖 AUTHORITY & WORKFLOW**

**Unity AI Assistant** is the **PRIMARY TECHNICAL LEAD** for all Unity 6 development.

**Bezi (Assistant)** serves as:
- **Unity AI Prompt Generator** - Creates detailed prompts for every development task
- **Project Coordinator** - Bridges user vision with AI implementation
- **Documentation Creator** - Maintains project context and decisions
- **NEVER implements code directly** - Always defers to Unity AI Assistant

**User** provides:
- **Vision & Requirements** - What the game should be and do
- **Testing & Feedback** - Validates AI implementations work correctly
- **Final Decisions** - Approves or requests changes to AI solutions

## **⚡ MANDATORY WORKFLOW**

**EVERY development task follows this pattern:**

1. **User** describes what they want to accomplish
2. **Bezi** creates a detailed Unity AI Assistant prompt
3. **User** copies prompt to Unity AI Assistant window
4. **Unity AI** designs and implements the solution
5. **User** tests implementation in Unity immediately
6. **If issues:** User reports back, Bezi creates troubleshooting prompt for Unity AI
7. **Repeat** until perfect

**NO EXCEPTIONS:** Bezi NEVER writes Unity code - only Unity AI prompts.

## **🎮 PROJECT SPECIFICATIONS**

**Project:** WWIII Family Sidescroller  
**Unity Version:** 6000.2 (Unity 6)  
**Target Platforms:** iOS (iPhone 16+) + macOS  
**Performance Goal:** 60 FPS stable on mobile  
**Render Pipeline:** URP 2D Renderer ✅  
**Core Packages:** Input System ✅, Cinemachine ✅, 2D Animation ✅, Unity AI Assistant ✅  

**Game Features:**
- Family-friendly 2D platformer with customizable avatars
- Touch controls + gamepad support + keyboard backup
- Data-driven design with ScriptableObjects
- 1 fully playable tutorial level + 4 stub levels
- Title screen (preserved) ✅
- Family avatar personalization system
- Collectibles, rewards, and progression
- Mobile-optimized performance

## **🏗️ CURRENT PROJECT STATE**

**CLEAN SLATE FOUNDATION:**

**✅ PRESERVED:**
- Title Screen: `Assets/_Project/Scenes/00_Title_Intro.unity`
- URP 2D Renderer configured
- Unity AI Assistant package installed
- Input System, Cinemachine, 2D packages ready
- Proper layer setup (Ground, Player, UI)
- Project structure: `Assets/_Project/{Scenes,Scripts,Art,Audio,UI,Data}`

**🗑️ DELETED (Clean Slate):**
- ALL previous scripts and implementations
- ALL level scenes except title
- ALL level data and prefabs
- ALL problematic constraint/movement code

**🎯 TO BUILD WITH UNITY AI:**
- Player Controller (Rigidbody2D, Input System, mobile-optimized)
- Camera System (smooth follow, bounds, performance)
- Level System (tilemaps, data-driven, ScriptableObjects)
- Family Avatar System (customizable characters, ScriptableObjects)
- Input Handling (touch overlay, gamepad, unified system)
- Mobile UI (touch controls, responsive, accessible)
- Save/Load System (progress, unlocks, family data)

## **📋 UNITY AI PROMPT TEMPLATES**

**Copy these templates for different development needs:**

### **🎮 SYSTEM DESIGN PROMPT:**
```
🎮 UNITY AI ASSISTANT - SYSTEM DESIGN

PROJECT: WWIII Family Sidescroller
UNITY: 6000.2 (Unity 6)
PLATFORM: iOS (iPhone 16+) + macOS
RENDER: URP 2D Renderer
TARGET: 60 FPS mobile performance

GOAL: [Specific system to build]
REQUIREMENTS: 
- Mobile-first performance optimization
- Family-friendly customization
- Data-driven architecture with ScriptableObjects
- Clean, maintainable code

CURRENT STATE: [Describe current project state]
CONSTRAINTS: [Any technical limitations]

Please design optimal [SYSTEM NAME] with:
1. Unity 6 best practices and latest features
2. Mobile performance optimizations
3. Complete implementation with all scripts
4. Step-by-step setup instructions
5. Integration with existing systems

Provide complete, tested, Unity 6-optimized code.
```

### **🔧 TROUBLESHOOTING PROMPT:**
```
🔧 UNITY AI ASSISTANT - ISSUE RESOLUTION

PROJECT: WWIII Family Sidescroller (Unity 6000.2)
PLATFORM: iOS + macOS, URP 2D

ISSUE: [Specific problem description]
CURRENT CODE: [Paste relevant code]
ERROR MESSAGES: [Console errors/warnings]
EXPECTED BEHAVIOR: [What should happen]
ACTUAL BEHAVIOR: [What's happening instead]

Please diagnose and provide:
1. Root cause analysis
2. Complete fixed implementation
3. Unity 6 optimization recommendations
4. Prevention strategies for similar issues

Ensure solution maintains 60 FPS mobile performance.
```

### **✨ FEATURE ADDITION PROMPT:**
```
✨ UNITY AI ASSISTANT - NEW FEATURE

PROJECT: WWIII Family Sidescroller (Unity 6000.2)
CURRENT SYSTEMS: [List existing systems]

NEW FEATURE: [Describe feature to add]
INTEGRATION POINTS: [How it connects to existing code]
MOBILE REQUIREMENTS: [Touch, performance, UI considerations]
FAMILY CUSTOMIZATION: [How families can modify/personalize]
DATA REQUIREMENTS: [ScriptableObjects, save data needed]

Please design and implement:
1. Complete feature with all scripts
2. Integration with existing systems
3. Mobile-optimized performance
4. Family-friendly customization options
5. Data-driven configuration

Provide Unity 6 best practices implementation.
```

### **🎨 CONTENT CREATION PROMPT:**
```
🎨 UNITY AI ASSISTANT - CONTENT GENERATION

PROJECT: WWIII Family Sidescroller (Unity 6000.2)
CONTENT TYPE: [Level, Character, UI, etc.]

SPECIFICATIONS:
- Platform: Mobile-first (iPhone 16+) + macOS
- Style: Family-friendly, customizable
- Performance: 60 FPS target
- Integration: [How it fits with existing systems]

REQUIREMENTS: [Specific content needs]
CUSTOMIZATION: [How families can personalize]
TECHNICAL: [Any technical constraints]

Please create:
1. Complete content implementation
2. Configuration system (ScriptableObjects)
3. Mobile performance optimization
4. Family customization options
5. Integration documentation

Ensure Unity 6 compatibility and mobile optimization.
```

## **🚀 SUCCESS CRITERIA**

- **Zero compilation errors** at all times
- **60 FPS stable** on iPhone 16+ target hardware
- **Unity AI validates** every technical decision and implementation
- **Clean, scalable architecture** that supports family customization
- **Professional mobile game quality** with polished user experience
- **Data-driven design** allowing easy content modification
- **Family personalization** system working seamlessly

## **⚡ IMMEDIATE NEXT STEPS**

1. **User describes** next development goal
2. **Bezi generates** appropriate Unity AI Assistant prompt
3. **User copies** prompt to Unity AI Assistant window
4. **Unity AI creates** optimal solution
5. **User tests** implementation immediately
6. **Iterate** based on results

## **🎯 STARTING CLEAN**

Current Status:
- ✅ Title screen preserved and functional
- ✅ Unity 6 with URP 2D optimally configured
- ✅ Unity AI Assistant ready for development
- ✅ All problematic code removed
- ✅ Clean foundation for AI-guided development

**READY FOR FIRST UNITY AI PROMPT!**