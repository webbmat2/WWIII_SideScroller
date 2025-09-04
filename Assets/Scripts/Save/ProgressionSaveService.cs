using System;
using System.IO;
using UnityEngine;

namespace WWIII.SideScroller.Save
{
    public interface IProgressionPersistence
    {
        bool TryLoad(out ProgressionSaveData data);
        void Save(ProgressionSaveData data);
    }

    public class FileProgressionPersistence : IProgressionPersistence
    {
        private readonly string _path;
        public FileProgressionPersistence(string filename = "progression.json")
        {
            _path = Path.Combine(Application.persistentDataPath, filename);
        }

        public bool TryLoad(out ProgressionSaveData data)
        {
            try
            {
                if (File.Exists(_path))
                {
                    var json = File.ReadAllText(_path);
                    data = JsonUtility.FromJson<ProgressionSaveData>(json) ?? new ProgressionSaveData();
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Progression load failed: {e.Message}");
            }
            data = new ProgressionSaveData();
            return false;
        }

        public void Save(ProgressionSaveData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data);
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_path, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Progression save failed: {e.Message}");
            }
        }
    }

    public class PlayerPrefsProgressionPersistence : IProgressionPersistence
    {
        private readonly string _key;
        public PlayerPrefsProgressionPersistence(string key = "progression_json")
        {
            _key = key;
        }

        public bool TryLoad(out ProgressionSaveData data)
        {
            if (PlayerPrefs.HasKey(_key))
            {
                var json = PlayerPrefs.GetString(_key, "{}");
                data = JsonUtility.FromJson<ProgressionSaveData>(json) ?? new ProgressionSaveData();
                return true;
            }
            data = new ProgressionSaveData();
            return false;
        }

        public void Save(ProgressionSaveData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(_key, json);
            PlayerPrefs.Save();
        }
    }

    public class ProgressionSaveService : MonoBehaviour
    {
        public static ProgressionSaveService Instance { get; private set; }

        public ProgressionSaveData Data { get; private set; } = new ProgressionSaveData();

        private IProgressionPersistence _persistence;

        [Tooltip("Use PlayerPrefs on WebGL automatically; others default to File. You can force PlayerPrefs when needed.")]
        public bool forcePlayerPrefs = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bool usePrefs = forcePlayerPrefs || Application.platform == RuntimePlatform.WebGLPlayer;
            _persistence = usePrefs ? new PlayerPrefsProgressionPersistence() : new FileProgressionPersistence();

            if (_persistence.TryLoad(out var loaded))
            {
                Data = loaded ?? new ProgressionSaveData();
            }
        }

        public void SetAgeIndex(int index)
        {
            Data.currentAgeIndex = Mathf.Max(0, index);
            _persistence.Save(Data);
        }

        public void AddPhoto(string id)
        {
            if (!Data.collectedPhotoIds.Contains(id))
            {
                Data.collectedPhotoIds.Add(id);
                _persistence.Save(Data);
            }
        }

        public void MarkCutsceneCompleted(string id)
        {
            if (!Data.completedCutsceneIds.Contains(id))
            {
                Data.completedCutsceneIds.Add(id);
                _persistence.Save(Data);
            }
        }
    }
}

