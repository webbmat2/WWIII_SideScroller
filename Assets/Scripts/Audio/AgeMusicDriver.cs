using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Audio
{
    public class AgeMusicDriver : MonoBehaviour
    {
        [Header("Age Monitoring")]
        public int currentAge = 7;
        public bool startMusicOnAwake = true;
        
        [Header("Debug")]
        public bool logAgeChanges = true;

        private AgeManager _ageManager;
        
        private void Start()
        {
            // Get initial age from AgeManager if available
            _ageManager = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (_ageManager != null)
            {
                currentAge = _ageManager.CurrentAge?.ageYears ?? currentAge;
                _ageManager.OnAgeChanged += HandleAgeChanged;
                if (logAgeChanges)
                    Debug.Log($"[AgeMusicDriver] Connected to AgeManager, current age: {currentAge}");
            }
            else if (logAgeChanges)
            {
                Debug.LogWarning("[AgeMusicDriver] AgeManager not found, using manual age: " + currentAge);
            }
            
            // Start with appropriate music for current age
            if (startMusicOnAwake)
            {
                BiographicalAudioManager.Instance?.SwitchMusicForAge(currentAge);
            }
        }
        
        private void OnDestroy()
        {
            if (_ageManager != null)
            {
                _ageManager.OnAgeChanged -= HandleAgeChanged;
            }
        }
        
        private void HandleAgeChanged(AgeProfile newAge)
        {
            if (newAge == null) return;
            if (logAgeChanges)
                Debug.Log($"[AgeMusicDriver] Age changed from {currentAge} to {newAge.ageYears}");
            currentAge = newAge.ageYears;
            BiographicalAudioManager.Instance?.SwitchMusicForAge(currentAge);
        }

        // Manual testing method
        [ContextMenu("Test Age Music")]
        public void TestCurrentAgeMusic()
        {
            BiographicalAudioManager.Instance?.SwitchMusicForAge(currentAge);
        }
    }
}
