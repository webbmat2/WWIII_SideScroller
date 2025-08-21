using UnityEngine;

public class LevelBootstrap : MonoBehaviour
{
    [Header("Optional Prefabs (drag in if/when you make them)")]
    [Tooltip("Manager object that tracks coins. Can be an empty GameObject with a CoinManager component, or a prefab.")]
    [SerializeField] GameObject coinManagerPrefab;
    [Tooltip("UI prefab that shows the coin count (e.g., a Canvas child with a CoinUI component).")]
    [SerializeField] GameObject coinUIPrefab;

    void Awake()
    {
        // If you assign prefabs in the Inspector, we instantiate them once at startup.
        // This version avoids hard references to script types so it compiles even if those scripts don't exist yet.

        if (coinManagerPrefab != null && GameObject.Find(coinManagerPrefab.name) == null)
        {
            var mgr = Instantiate(coinManagerPrefab);
            mgr.name = coinManagerPrefab.name; // avoid "(Clone)"
        }

        if (coinUIPrefab != null && GameObject.Find(coinUIPrefab.name) == null)
        {
            var ui = Instantiate(coinUIPrefab);
            ui.name = coinUIPrefab.name;
            // If there is a Canvas in the scene and this UI isn't already parented, parent it for convenience.
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null && ui.transform.parent == null)
                ui.transform.SetParent(canvas.transform, worldPositionStays: false);
        }
    }
}