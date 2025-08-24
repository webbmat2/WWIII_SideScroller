using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Gameplay/Player Inventory")]
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private List<string> items = new List<string>();
    [SerializeField] private int maxItems = 10;

    public event System.Action<string> OnItemAdded;
    public event System.Action<string> OnItemRemoved;

    public List<string> Items => new List<string>(items); // Return copy to prevent external modification

    public void AddItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;

        if (items.Count >= maxItems)
        {
            Debug.LogWarning($"Inventory full! Cannot add {itemID}");
            return;
        }

        items.Add(itemID);
        OnItemAdded?.Invoke(itemID);
        Debug.Log($"Item added to inventory: {itemID}");
    }

    public bool RemoveItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return false;

        bool removed = items.Remove(itemID);
        if (removed)
        {
            OnItemRemoved?.Invoke(itemID);
            Debug.Log($"Item removed from inventory: {itemID}");
        }
        return removed;
    }

    public bool HasItem(string itemID)
    {
        return !string.IsNullOrEmpty(itemID) && items.Contains(itemID);
    }

    public int GetItemCount(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return 0;

        int count = 0;
        foreach (string item in items)
        {
            if (item == itemID) count++;
        }
        return count;
    }

    public void ClearInventory()
    {
        var itemsCopy = new List<string>(items);
        items.Clear();
        
        foreach (string item in itemsCopy)
        {
            OnItemRemoved?.Invoke(item);
        }
        
        Debug.Log("Inventory cleared");
    }

    public bool HasSpace()
    {
        return items.Count < maxItems;
    }

    // Debug method to show inventory contents
    [ContextMenu("Show Inventory")]
    public void ShowInventory()
    {
        Debug.Log($"=== INVENTORY ({items.Count}/{maxItems}) ===");
        for (int i = 0; i < items.Count; i++)
        {
            Debug.Log($"{i + 1}. {items[i]}");
        }
        if (items.Count == 0)
        {
            Debug.Log("Inventory is empty");
        }
    }
}