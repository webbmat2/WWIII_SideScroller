using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        CoinManager.OnCoinsChanged += SetCoins;
        SetCoins(CoinManager.Count); // Initialize with current count
    }

    private void OnDisable()
    {
        CoinManager.OnCoinsChanged -= SetCoins;
    }

    public void SetCoins(int amount)
    {
        if (label != null) label.text = amount.ToString();
    }
}
