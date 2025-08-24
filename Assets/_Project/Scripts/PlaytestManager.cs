using UnityEngine;
using System.Collections.Generic;
using System.IO;

[AddComponentMenu("Analytics/Playtest Manager")]
public class PlaytestManager : MonoBehaviour
{
    [Header("Data Collection")]
    [SerializeField] private bool enableDataCollection = true;
    [SerializeField] private string sessionName = "Playtest_Session";
    [SerializeField] private float dataCollectionInterval = 0.5f;

    [Header("Death Heatmap")]
    [SerializeField] private bool trackDeaths = true;
    [SerializeField] private float heatmapRadius = 2f;

    private List<DeathData> _deathPositions = new List<DeathData>();
    private List<PlaytestEvent> _events = new List<PlaytestEvent>();
    private float _sessionStartTime;
    private float _nextDataCollection;
    private int _coinCollectionCount;
    private int _retryCount;
    private PlayerController2D _player;

    [System.Serializable]
    public class DeathData
    {
        public Vector3 position;
        public float timeStamp;
        public string cause;
        public int healthRemaining;
    }

    [System.Serializable]
    public class PlaytestEvent
    {
        public string eventType;
        public Vector3 position;
        public float timeStamp;
        public string additionalData;
    }

    [System.Serializable]
    public class SessionSummary
    {
        public string sessionName;
        public float totalTime;
        public int totalDeaths;
        public int coinsCollected;
        public int totalRetries;
        public List<DeathData> deaths;
        public List<PlaytestEvent> events;
    }

    private void Start()
    {
        if (!enableDataCollection) return;

        _sessionStartTime = Time.time;
        _player = FindFirstObjectByType<PlayerController2D>();

        // Subscribe to events
        CoinManager.OnCoinsChanged += OnCoinCollected;
        
        LogEvent("session_start", Vector3.zero, "Session started");
    }

    private void Update()
    {
        if (!enableDataCollection) return;

        // Collect periodic data
        if (Time.time >= _nextDataCollection)
        {
            CollectPeriodicData();
            _nextDataCollection = Time.time + dataCollectionInterval;
        }

        // Check for retry input (for manual tracking)
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
        {
            _retryCount++;
            LogEvent("manual_retry", _player?.transform.position ?? Vector3.zero, $"Retry count: {_retryCount}");
        }
    }

    private void OnDestroy()
    {
        if (enableDataCollection)
        {
            ExportSessionData();
        }
    }

    public void LogDeath(Vector3 position, string cause = "Unknown")
    {
        if (!enableDataCollection) return;

        var deathData = new DeathData
        {
            position = position,
            timeStamp = Time.time - _sessionStartTime,
            cause = cause,
            healthRemaining = _player?.CurrentHealth ?? 0
        };

        _deathPositions.Add(deathData);
        LogEvent("player_death", position, cause);

        Debug.Log($"Death logged: {cause} at {position} (Time: {deathData.timeStamp:F2}s)");
    }

    public void LogEvent(string eventType, Vector3 position, string additionalData = "")
    {
        if (!enableDataCollection) return;

        var playtestEvent = new PlaytestEvent
        {
            eventType = eventType,
            position = position,
            timeStamp = Time.time - _sessionStartTime,
            additionalData = additionalData
        };

        _events.Add(playtestEvent);
    }

    private void OnCoinCollected(int newTotal)
    {
        _coinCollectionCount++;
        if (_player != null)
        {
            LogEvent("coin_collected", _player.transform.position, $"Total: {newTotal}");
        }
    }

    private void CollectPeriodicData()
    {
        if (_player == null) return;

        // Log player position for heatmap analysis
        LogEvent("player_position", _player.transform.position, 
            $"Health: {_player.CurrentHealth}, Velocity: {_player.GetComponent<Rigidbody2D>()?.linearVelocity}");
    }

    public void LogCheckpointReached(string checkpointName, Vector3 position)
    {
        LogEvent("checkpoint_reached", position, checkpointName);
    }

    public void LogJump(Vector3 position, bool successful)
    {
        LogEvent("player_jump", position, successful ? "successful" : "failed");
    }

    public void LogHazardEncounter(Vector3 position, string hazardType, bool avoided)
    {
        LogEvent("hazard_encounter", position, $"{hazardType}: {(avoided ? "avoided" : "hit")}");
    }

    private void ExportSessionData()
    {
        var summary = new SessionSummary
        {
            sessionName = sessionName + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            totalTime = Time.time - _sessionStartTime,
            totalDeaths = _deathPositions.Count,
            coinsCollected = _coinCollectionCount,
            totalRetries = _retryCount,
            deaths = _deathPositions,
            events = _events
        };

        string json = JsonUtility.ToJson(summary, true);
        
        // Save to persistent data path
        string fileName = summary.sessionName + ".json";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        
        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Playtest data exported to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export playtest data: {e.Message}");
        }

        // Also log summary to console
        Debug.Log($"=== PLAYTEST SUMMARY ===");
        Debug.Log($"Session: {summary.sessionName}");
        Debug.Log($"Duration: {summary.totalTime:F2} seconds");
        Debug.Log($"Deaths: {summary.totalDeaths}");
        Debug.Log($"Coins: {summary.coinsCollected}");
        Debug.Log($"Retries: {summary.totalRetries}");
        Debug.Log($"Events: {summary.events.Count}");
    }

    // Manual export for testing
    [ContextMenu("Export Session Data Now")]
    public void ManualExport()
    {
        ExportSessionData();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!trackDeaths || _deathPositions.Count == 0) return;

        // Draw death heatmap
        Gizmos.color = Color.red;
        foreach (var death in _deathPositions)
        {
            Gizmos.DrawWireSphere(death.position, heatmapRadius);
            UnityEditor.Handles.Label(death.position + Vector3.up * 0.5f, 
                $"Death: {death.cause}\nTime: {death.timeStamp:F1}s");
        }
    }
#endif
}