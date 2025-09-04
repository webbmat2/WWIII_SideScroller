using UnityEngine;

namespace WWIII.SideScroller.UI.Achievements
{
    public enum AchievementKind { Unlock, Counter }

    [CreateAssetMenu(menuName = "WWIII/Achievements/Achievement", fileName = "Achievement")]
    public class AchievementDefinition : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea]
        public string description;
        public AchievementKind kind = AchievementKind.Unlock;
        [Tooltip("For counter achievements, the count required to consider it completed.")]
        public int targetCount = 1;
    }
}

