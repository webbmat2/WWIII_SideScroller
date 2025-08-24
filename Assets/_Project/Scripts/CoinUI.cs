using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    public void SetCoins(int amount)
    {
        if (label != null) label.text = amount.ToString();
    }
}
