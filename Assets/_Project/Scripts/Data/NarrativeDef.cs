using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace WWIII.Data
{
    /// <summary>
    /// Data-driven narrative system for level story beats
    /// </summary>
    [CreateAssetMenu(fileName = "NarrativeDef_", menuName = "WWIII/Narrative Definition")]
    public class NarrativeDef : ScriptableObject
    {
        [Title("Narrative Information")]
        public string narrativeName;
        public string levelId;
        
        [Title("Story Beats")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "beatTitle")]
        public List<StoryBeat> storyBeats = new List<StoryBeat>();
        
        [Title("Dialog System")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "speakerName")]
        public List<DialogEntry> dialogEntries = new List<DialogEntry>();
        
        [Title("Triggers")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "triggerName")]
        public List<NarrativeTrigger> triggers = new List<NarrativeTrigger>();
    }
    
    [System.Serializable]
    public class StoryBeat
    {
        [HorizontalGroup("Beat")]
        [LabelWidth(80)]
        public string beatTitle = "Story Beat";
        
        [HorizontalGroup("Beat")]
        [LabelWidth(80)]
        public float triggerTime = 0f;
        
        [TextArea(2, 4)]
        public string beatText = "Story beat content";
        
        public bool isRequired = true;
        public Vector2 triggerPosition = Vector2.zero;
    }
    
    [System.Serializable]
    public class DialogEntry
    {
        [HorizontalGroup("Dialog")]
        [LabelWidth(80)]
        public string speakerName = "Character";
        
        [HorizontalGroup("Dialog")]
        [LabelWidth(80)]
        public float displayDuration = 3f;
        
        [TextArea(2, 4)]
        public string dialogText = "Dialog content";
        
        public Sprite characterPortrait;
        public AudioClip voiceClip;
    }
    
    [System.Serializable]
    public class NarrativeTrigger
    {
        [HorizontalGroup("Trigger")]
        [LabelWidth(80)]
        public string triggerName = "Trigger";
        
        [HorizontalGroup("Trigger")]
        [LabelWidth(80)]
        public TriggerType triggerType = TriggerType.Position;
        
        public Vector2 position = Vector2.zero;
        public string targetTag = "Player";
        public bool triggerOnce = true;
        
        [ShowIf("triggerType", TriggerType.Event)]
        public string eventName = "";
    }
    
    public enum TriggerType
    {
        Position,
        Event,
        Timer,
        Collectible
    }
}