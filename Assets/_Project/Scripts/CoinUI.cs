using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CoinUI : MonoBehaviour
{
    TextMeshProUGUI label;
    void Awake() { label = GetComponent<TextMeshProUGUI>(); }
    void Update() { label.text = $"Coins: {CoinManager.Coins}"; }
}