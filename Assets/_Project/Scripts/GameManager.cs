using UnityEngine;

[AddComponentMenu("Gameplay/Game Manager")]
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool resetCoinsOnStart = true;
    [SerializeField] private int startingCoins = 0;

    private void Start()
    {
        if (resetCoinsOnStart)
        {
            CoinManager.Reset(startingCoins);
        }

        Debug.Log($"Game started with {CoinManager.Count} coins");
    }

    private void Update()
    {
        // Debug key to reset coins
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
        {
            CoinManager.Reset(startingCoins);
            Debug.Log("Coins reset!");
        }
    }
}