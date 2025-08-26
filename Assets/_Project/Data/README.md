# Data Directory

Contains all ScriptableObject data files:

## Structure
- **Levels/** - Level definitions, spawn points, objectives
- **Characters/** - Player stats, enemy configurations
- **Items/** - Pickup definitions, power-up effects
- **Story/** - Story beats, dialog text
- **Settings/** - Game configuration, difficulty settings

## Guidelines
- No hardcoded content - load everything from data files
- Use ScriptableObjects for game configuration
- Support JSON import/export for external editing
- Include lightweight editors with spawn gizmos
- Auto-populate Level_Select from level data