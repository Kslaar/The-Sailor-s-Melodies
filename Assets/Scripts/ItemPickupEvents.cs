using System;
using UnityEngine;

public static class ItemPickupEvents
{
    public static event Action<string> OnItemCollected;

    public static void RaiseItemCollected(string itemID)
    {
        OnItemCollected?.Invoke(itemID);
    }
}
