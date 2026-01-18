using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(menuName ="Game/Quests/Objectives/Collect Item")]
public class CollectItemQuest : QuestObjective
{
    public string itemID = "fuel_canister";
    public int requiredCount = 1;

    [System.NonSerialized] private int current;

    public override bool IsComplete => current >= requiredCount;
    public override void Register()
    {
        current = 0;
        ItemPickupEvents.OnItemCollected += HandleCollected;
    }

    public override void Unregister()
    {
        ItemPickupEvents.OnItemCollected -= HandleCollected;
    }

    private void HandleCollected(string collectedID)
    {
        if (IsComplete) return;
        if (collectedID != itemID) return;

        current++;
        Debug.Log($"[Quest] Collected {itemID}: {current}/{requiredCount}");
    }
}
