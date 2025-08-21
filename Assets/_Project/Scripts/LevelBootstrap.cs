using UnityEngine;
using UnityEngine.UI;

public class LevelBootstrap : MonoBehaviour
{
    [Header("UI Prefabs (optional, will be spawned if missing)")]
    [SerializeField] CoinUI coinUIPrefab;
    [SerializeField] CollectibleUI collectibleUIPrefab;

    [Header("Collectibles")]
    [SerializeField, Min(1)] int collectibleTarget = 5;

    [Header("Canvas Settings")] 
    [SerializeField] Vector2 referenceResolution = new Vector2(1920, 1080);
    [SerializeField, Range(0,1)] float matchWidthOrHeight = 0.5f;

    void Awake()
    {
        // 1) Ensure Canvas exists and is scaled correctly
        var canvas = FindFirstObjectByType<Canvas>();
        if (!canvas)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (!scaler) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = matchWidthOrHeight;

        // 2) Reset collectible target at level start
        CollectibleManager.Reset(collectibleTarget);

        // 3) Spawn UI if missing
        EnsureCoinUI(canvas.transform);
        EnsureCollectibleUI(canvas.transform);
    }

    void EnsureCoinUI(Transform parent)
    {
        var existing = FindFirstObjectByType<CoinUI>(FindObjectsInactive.Include);
        if (existing) return;
        if (!coinUIPrefab) return;
        var ui = Instantiate(coinUIPrefab, parent);
        // anchor top-left
        var rt = ui.GetComponent<RectTransform>();
        if (rt)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20);
        }
    }

    void EnsureCollectibleUI(Transform parent)
    {
        var existing = FindFirstObjectByType<CollectibleUI>(FindObjectsInactive.Include);
        if (existing) return;
        if (!collectibleUIPrefab) return;
        var ui = Instantiate(collectibleUIPrefab, parent);
        // anchor top-right
        var rt = ui.GetComponent<RectTransform>();
        if (rt)
        {
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-20, -20);
        }
    }
}