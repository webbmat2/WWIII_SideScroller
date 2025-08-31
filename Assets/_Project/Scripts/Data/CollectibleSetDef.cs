using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace WWIII.Data
{
    /// <summary>
    /// Collectible tracking system - 5 per level = badge, all levels = Grand Video
    /// </summary>
    [CreateAssetMenu(fileName = "CollectibleSetDef_", menuName = "WWIII/Collectible Set Definition")]
    public class CollectibleSetDef : ScriptableObject
    {
        [Title("Set Information")]
        public string setName = "War Medals";
        public string setDescription = "Collect 5 war medals to earn a badge";
        
        [PreviewField(80)]
        public Sprite setBadgeIcon;
        
        [Title("Collectibles")]
        [Range(1, 10)]
        public int totalCollectibles = 5;
        
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "collectibleName")]
        public List<CollectibleItem> collectibles = new List<CollectibleItem>();
        
        [Title("Rewards")]
        public string badgeName = "Medal of Honor";
        
        [TextArea(2, 4)]
        public string badgeDescription = "Awarded for collecting all war medals in this level";
        
        [PreviewField(80)]
        public Sprite badgeSprite;
        
        public int pointValue = 100;
        public bool contributesToGrandVideo = true;
        
        [Title("Audio")]
        public AudioClip collectSound;
        public AudioClip badgeEarnedSound;
        
        [Button("Generate Default Collectibles")]
        private void GenerateDefaultCollectibles()
        {
            collectibles.Clear();
            for (int i = 0; i < totalCollectibles; i++)
            {
                bool isSecret = i >= 3; // Last 2 are secrets
                collectibles.Add(new CollectibleItem
                {
                    collectibleName = $"{setName} {i + 1}",
                    description = $"A valuable {setName.ToLower()}",
                    isSecret = isSecret,
                    pointValue = isSecret ? 50 : 20
                });
            }
        }
    }
    
    [System.Serializable]
    public class CollectibleItem
    {
        [HorizontalGroup("Item")]
        [LabelWidth(100)]
        public string collectibleName = "Collectible";
        
        [HorizontalGroup("Item")]
        [LabelWidth(60)]
        public bool isSecret = false;
        
        [TextArea(2, 3)]
        public string description = "Collectible description";
        
        [PreviewField(60)]
        public Sprite icon;
        
        public Vector2 position = Vector2.zero;
        public int pointValue = 20;
        
        [ShowIf("isSecret")]
        public string secretHint = "Look for hidden areas";
    }
}