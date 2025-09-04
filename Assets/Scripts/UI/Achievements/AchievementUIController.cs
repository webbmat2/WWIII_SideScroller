using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.UI.Achievements
{
    public class AchievementUIController : MonoBehaviour
    {
        public Transform listRoot;
        public AchievementUIItem itemPrefab;

        private void OnEnable()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            if (listRoot == null || itemPrefab == null || AchievementService.Instance == null) return;
            foreach (Transform child in listRoot) Destroy(child.gameObject);

            foreach (var def in AchievementService.Instance.known)
            {
                var item = Instantiate(itemPrefab, listRoot);
                var unlocked = AchievementService.Instance.IsUnlocked(def.id);
                var count = AchievementService.Instance.GetCount(def.id);
                item.Bind(def.title, def.description, unlocked, def.kind == AchievementKind.Counter ? count : -1, def.targetCount);
            }
        }
    }

    public class AchievementUIItem : MonoBehaviour
    {
        public UnityEngine.UI.Text title;
        public UnityEngine.UI.Text desc;
        public UnityEngine.UI.Text progress;

        public void Bind(string t, string d, bool unlocked, int count, int target)
        {
            if (title != null) title.text = (unlocked ? "✔ " : "• ") + t;
            if (desc != null) desc.text = d;
            if (progress != null)
            {
                if (count >= 0 && target > 0)
                {
                    progress.text = $"{count}/{target}";
                }
                else
                {
                    progress.text = unlocked ? "Completed" : "Locked";
                }
            }
        }
    }
}

