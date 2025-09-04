using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Save;

namespace WWIII.SideScroller.UI.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        public PlayableDirector timelinePreview;
        public AgeSet ageSet;

        [Header("Config")]
        public string firstGameplayScene = "BioLevel_Age7";

        private void Start()
        {
            if (timelinePreview != null)
            {
                timelinePreview.Play();
            }
        }

        public void OnClickNewGame()
        {
            ProgressionSaveService.Instance?.SetAgeIndex(0);
            if (!string.IsNullOrEmpty(firstGameplayScene))
            {
                SceneManager.LoadScene(firstGameplayScene);
            }
        }

        public void OnClickContinue()
        {
            var idx = ProgressionSaveService.Instance != null ? ProgressionSaveService.Instance.Data.currentAgeIndex : 0;
            var profile = ageSet != null && idx >= 0 && idx < ageSet.ages.Count ? ageSet.ages[idx] : null;
            var sceneName = profile != null ? $"BioLevel_Age{profile.ageYears}" : firstGameplayScene;
            SceneManager.LoadScene(sceneName);
        }

        public void OnClickLevelSelect()
        {
            SceneManager.LoadScene("LevelSelect");
        }

        public void OnClickPhotoAlbum()
        {
            SceneManager.LoadScene("PhotoAlbum");
        }
    }
}

