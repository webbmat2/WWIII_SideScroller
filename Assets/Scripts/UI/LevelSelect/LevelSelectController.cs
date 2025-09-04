using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Level;
using WWIII.SideScroller.Save;

namespace WWIII.SideScroller.UI.LevelSelect
{
    public class LevelSelectController : MonoBehaviour
    {
        [Header("Data")]
        public AgeSet ageSet;
        public LevelRegistry levelRegistry;

        [Header("UI Hooks")]
        [Tooltip("Parent transform where level buttons are instantiated.")]
        public Transform listRoot;
        [Tooltip("Prefab with a LevelSelectItem component.")]
        public LevelSelectItem itemPrefab;

        private void Start()
        {
            Populate();
        }

        public void Populate()
        {
            if (listRoot == null || itemPrefab == null || ageSet == null) return;
            foreach (Transform child in listRoot) Destroy(child.gameObject);

            int unlockedIndex = ProgressionSaveService.Instance != null ? ProgressionSaveService.Instance.Data.currentAgeIndex : 0;

            for (int i = 0; i < ageSet.ages.Count; i++)
            {
                var profile = ageSet.ages[i];
                if (profile == null) continue;
                var item = Instantiate(itemPrefab, listRoot);
                var def = levelRegistry != null ? levelRegistry.GetByAgeYears(profile.ageYears) : null;
                item.Bind(profile.displayName, profile.ageYears, def != null ? def.sceneName : $"BioLevel_Age{profile.ageYears}", i <= unlockedIndex);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }

    public class LevelSelectItem : MonoBehaviour
    {
        public UnityEngine.UI.Text title;
        public UnityEngine.UI.Button button;
        private string _sceneName;

        public void Bind(string label, int ageYears, string sceneName, bool unlocked)
        {
            _sceneName = sceneName;
            if (title != null) title.text = $"Age {ageYears}: {label}";
            if (button != null)
            {
                button.interactable = unlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneName));
            }
        }
    }
}

