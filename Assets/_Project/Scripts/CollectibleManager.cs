using System;
using UnityEngine;

// Tracks SPECIAL collectibles per level (distinct from coins)
public static class CollectibleManager
{
    public static int Count  { get; private set; }
    public static int Target { get; private set; } = 5;

    // (count, target)
    public static event Action<int,int> OnCollectiblesChanged;

    public static void Reset(int target)
    {
        Target = Mathf.Max(1, target);
        Count  = 0;
        OnCollectiblesChanged?.Invoke(Count, Target);
    }

    public static void Add(int amount)
    {
        if (amount == 0) return;
        Count = Mathf.Clamp(Count + amount, 0, Target);
        OnCollectiblesChanged?.Invoke(Count, Target);
    }
}