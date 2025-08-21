using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    void Awake()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>();
        UpdateLabel(CoinManager.Count);
    }

    void OnEnable()  { CoinManager.OnCoinsChanged += UpdateLabel; }
    void OnDisable() { CoinManager.OnCoinsChanged -= UpdateLabel; }

    void UpdateLabel(int newCount)
    {
        if (label) label.text = $"Coins: {newCount}";
    }
}
