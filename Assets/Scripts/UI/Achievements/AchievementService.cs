using System;
using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.UI.Achievements
{
    public class AchievementService : MonoBehaviour
    {
        public static AchievementService Instance { get; private set; }

        [Tooltip("Optional known achievements list for UI.")]
        public List<AchievementDefinition> known = new();

        private const string PrefsKey = "wwiii_achievements_json";

        [Serializable]
        private class State
        {
            public List<string> unlocked = new();
            public Dictionary<string, int> counters = new();
        }

        private State _state = new();

        public event Action<string> OnUnlocked;
        public event Action<string, int> OnCounterChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (PlayerPrefs.HasKey(PrefsKey))
            {
                var json = PlayerPrefs.GetString(PrefsKey, "{}");
                try { _state = JsonUtility.FromJson<State>(json) ?? new State(); }
                catch { _state = new State(); }
            }
        }

        private void Save()
        {
            var json = JsonUtility.ToJson(_state);
            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }

        public bool IsUnlocked(string id) => _state.unlocked.Contains(id);
        public int GetCount(string id) => _state.counters.TryGetValue(id, out var v) ? v : 0;

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || _state.unlocked.Contains(id)) return;
            _state.unlocked.Add(id);
            Save();
            OnUnlocked?.Invoke(id);
        }

        public void IncrementCounter(string id, int delta, int autoUnlockAt)
        {
            if (string.IsNullOrEmpty(id)) return;
            var value = GetCount(id) + Mathf.Max(0, delta);
            _state.counters[id] = value;
            if (autoUnlockAt > 0 && value >= autoUnlockAt)
            {
                Unlock(id + "_DONE");
            }
            Save();
            OnCounterChanged?.Invoke(id, value);
        }
    }
}

