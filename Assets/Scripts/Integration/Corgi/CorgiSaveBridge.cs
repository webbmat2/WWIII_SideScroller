using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using WWIII.SideScroller.Save;

namespace WWIII.SideScroller.Integration.Corgi
{
    [DisallowMultipleComponent]
    public class CorgiSaveBridge : MonoBehaviour, MMEventListener<CorgiEngineEvent>
    {
        [Tooltip("If true, pushes simple level complete milestones to the achievement system and persists age index via ProgressionSaveService.")]
        public bool pushMilestones = true;

        public void OnMMEvent(CorgiEngineEvent e)
        {
            if (!pushMilestones) return;
            switch (e.EventType)
            {
                case CorgiEngineEventTypes.LevelComplete:
                case CorgiEngineEventTypes.LevelEnd:
                    TryRecordLevelComplete();
                    break;
                case CorgiEngineEventTypes.PlayerDeath:
                    TryRecordDeath();
                    break;
            }
        }

        private void TryRecordLevelComplete()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            // Persist current age index (AgeManager does this too on change; here we ensure we save on level complete)
            try { ProgressionSaveService.Instance?.SetAgeIndex(ProgressionSaveService.Instance.Data.currentAgeIndex); } catch { }

            // Forward to achievements if present
            var achiever = FindFirstObjectByType<UI.Achievements.AchievementService>();
            if (achiever != null)
            {
                achiever.Unlock($"LEVEL_{sceneName}_COMPLETE");
            }
        }

        private void TryRecordDeath()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var achiever = FindFirstObjectByType<UI.Achievements.AchievementService>();
            if (achiever != null)
            {
                achiever.IncrementCounter($"LEVEL_{sceneName}_DEATHS", 1, 999);
            }
        }

        private void OnEnable()
        {
            this.MMEventStartListening<CorgiEngineEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<CorgiEngineEvent>();
        }
    }
}

