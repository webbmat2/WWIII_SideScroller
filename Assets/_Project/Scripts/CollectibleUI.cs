using UnityEngine;
using TMPro;

public class CollectibleUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        UpdateLabel(CollectibleManager.Count, CollectibleManager.Target);
    }

    void OnEnable()  { CollectibleManager.OnCollectiblesChanged += UpdateLabel; }
    void OnDisable() { CollectibleManager.OnCollectiblesChanged -= UpdateLabel; }

    void UpdateLabel(int count, int target)
    {
        if (label != null) label.text = $"Collectibles: {count}/{target}";
    }
}