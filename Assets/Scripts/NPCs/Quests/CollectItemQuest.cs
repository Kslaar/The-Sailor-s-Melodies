using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(menuName ="Game/Quests/Objectives/Collect Item")]
public class CollectItemQuest : QuestObjective
{
    public string itemID = "fuel_canister";
    public int requiredCount = 1;

    [System.NonSerialized] private int current;

    public int Current => current;
    public int Required => requiredCount;

    public override bool IsComplete => current >= requiredCount;
    public override string ProgressText => $"{Mathf.Min(current, requiredCount)}/{requiredCount}";
    public override float Progress01 => requiredCount <= 0 ? 1f : Mathf.Clamp01((float)current / requiredCount);

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

        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
    }
}
