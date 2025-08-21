using System;

public static class CoinManager
{
    public static int Count { get; private set; }
    public static event Action<int> OnCoinsChanged;

    public static void Reset(int start = 0)
    {
        Count = start;
        OnCoinsChanged?.Invoke(Count);
    }

    public static void Add(int amount)
    {
        if (amount == 0) return;
        Count += amount;
        OnCoinsChanged?.Invoke(Count);
    }
}