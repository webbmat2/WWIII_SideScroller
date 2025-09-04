using UnityEngine;
using UnityEngine.Playables;
using WWIII.SideScroller.Aging;
using MoreMountains.CorgiEngine;

namespace WWIII.SideScroller.Integration.Corgi
{
    [DisallowMultipleComponent]
    public class CorgiCameraDirector : MonoBehaviour
    {
        [Tooltip("PlayableDirector to listen to. If null, will search on this GameObject and on AgeManager.")]
        public PlayableDirector director;

        [Tooltip("AgeManager to observe for retargeting.")]
        public AgeManager ageManager;

        private CameraController _cameraController;

        private void Reset()
        {
            director = GetComponent<PlayableDirector>();
            ageManager = FindFirstObjectByType<AgeManager>();
        }

        private void Awake()
        {
            if (ageManager == null) ageManager = FindFirstObjectByType<AgeManager>();
            if (director == null)
            {
                director = GetComponent<PlayableDirector>();
                if (director == null && ageManager != null)
                {
                    director = ageManager.director;
                }
            }
        }

        private void OnEnable()
        {
            _cameraController = LevelManager.HasInstance ? LevelManager.Instance.LevelCameraController : null;
            if (director != null)
            {
                director.played += OnDirectorPlayed;
                director.stopped += OnDirectorStopped;
            }
            if (ageManager != null)
            {
                ageManager.OnAgeChanged += OnAgeChanged;
            }
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.played -= OnDirectorPlayed;
                director.stopped -= OnDirectorStopped;
            }
            if (ageManager != null)
            {
                ageManager.OnAgeChanged -= OnAgeChanged;
            }
        }

        private void OnDirectorPlayed(PlayableDirector d)
        {
            if (_cameraController == null) _cameraController = LevelManager.HasInstance ? LevelManager.Instance.LevelCameraController : null;
            if (_cameraController != null)
            {
                _cameraController.FollowsPlayer = false;
            }
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            if (_cameraController == null) _cameraController = LevelManager.HasInstance ? LevelManager.Instance.LevelCameraController : null;
            if (_cameraController != null)
            {
                _cameraController.FollowsPlayer = true;
                RetargetToPlayer();
            }
        }

        private void OnAgeChanged(AgeProfile _)
        {
            RetargetToPlayer();
        }

        private void RetargetToPlayer()
        {
            if (_cameraController == null) _cameraController = LevelManager.HasInstance ? LevelManager.Instance.LevelCameraController : null;
            if (_cameraController == null) return;
            if (LevelManager.HasInstance && LevelManager.Instance.Players != null && LevelManager.Instance.Players.Count > 0)
            {
                _cameraController.SetTarget(LevelManager.Instance.Players[0].transform);
            }
        }
    }
}

