using System;
using UnityEngine;

public static class ItemPickupEvents
{
    public static event Action<string> OnItemCollected;

    public static void RaiseItemCollected(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning("[ItemPuckupEvents] itemID ist null/leer - ignorieren");
            return;
        }
        OnItemCollected?.Invoke(itemID);
    }
}
