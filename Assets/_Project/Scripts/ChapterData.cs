using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Chapter_", menuName = "WWIII/Chapter Data")]
public class ChapterData : ScriptableObject
{
    [Header("Chapter Identity")]
    public string chapterId;
    public string title;
    public string location;
    
    [Header("Presentation")]
    public Sprite backgroundTheme;
    public string musicTag;
    
    [Header("Gameplay")]
    [TextArea(3, 5)]
    public string[] objectives;
    public bool hasBoss;
    public string bossType;
    
    [Header("Power-ups Available")]
    public PowerUpType[] availablePowerUps;
    
    [Header("Collectibles")]
    public Sprite collectibleSprite;
    public string collectibleName;
    [Range(1, 5)]
    public int maxCollectibles = 5;
    
    [Header("Special Rules")]
    [TextArea(2, 4)]
    public string[] specialRules;
    public bool requiresHose;
    public bool requiresCrouch;
    public bool hasVehicleCameo;
    public string vehicleDescription;
    
    [Header("Flow")]
    public string nextChapterId;
    public string sceneName;
    
    [Header("Debug")]
    public bool isUnlocked = true;
    public bool isCompleted = false;
}

[System.Serializable]
public enum PowerUpType
{
    None,
    Hose,
    CherryPie,
    Chiliguaro,
    SmartJim,
    BeefJerky,
    CheeseBall
}