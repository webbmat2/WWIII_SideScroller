using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static int Coins { get; private set; }
    public static void ResetAll() => Coins = 0;
    public static void Add(int amount) => Coins += amount;
}