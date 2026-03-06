using System;
using UnityEngine;

public static class QuestZoneEvent
{
    public static event Action<string> OnZoneEntered;
    public static void RaiseEntered(string zoneID)
    {
        if (string.IsNullOrWhiteSpace(zoneID)) return;
        OnZoneEntered?.Invoke(zoneID);
    }
}
