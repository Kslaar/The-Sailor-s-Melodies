using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName ="Game/Quests/Objectives/Collect Items (Universal)")]
public class UniversalCollectItemQuest : QuestObjective
{
    public enum Mode
    {
        CountSingleItem, 
        CollectUniqueIDs,
    }

    [Header("Mode")]
    [SerializeField] private Mode mode = Mode.CountSingleItem;

    [Header("CountSingleItem")]
    public string itemID = "fuel_canisters";
    public int requiredCount = 1;

    public List<string> requiredItemIDs = new() { "page_1", "page_2", "page_3", "page_4" };

    [NonSerialized] private int currentCount;
    [NonSerialized] private HashSet<string> collectedUnique;

    public override void Register()
    {
        ItemPickupEvents.OnItemCollected -= HandleCollected;
        ItemPickupEvents.OnItemCollected += HandleCollected;

        var reg = ItemCollectionRegistry.Instance;

        switch (mode)
        {
            case Mode.CountSingleItem:
                currentCount = reg != null ? reg.GetCount(itemID) : 0;
                break;

            case Mode.CollectUniqueIDs:
                collectedUnique = new HashSet<string>();

                if (requiredItemIDs != null && reg != null)
                {
                    // Haben wir in der Registry einen höheren Wert als 0? Dann haben wir ID eingesammelt
                    foreach (var id in requiredItemIDs)
                    {
                        if (reg.GetCount(id) > 0)
                            collectedUnique.Add(id);
                    }
                }
                break;
        }

        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();  
    }

    public override void Unregister()
    {
        ItemPickupEvents.OnItemCollected -= HandleCollected;
    }

    private void HandleCollected(string collectedID)
    {
        if (IsComplete) return;
        if (string.IsNullOrWhiteSpace(collectedID)) return;

        switch(mode)
        {
            case Mode.CountSingleItem:
                if (collectedID != itemID) return;
                currentCount++;
                Debug.Log($"[Quest] Collected {itemID}: {currentCount}/{requiredCount}");
                break;
            
            case Mode.CollectUniqueIDs:
                if (requiredItemIDs == null || requiredItemIDs.Count == 0) return;
                if (!requiredItemIDs.Contains(collectedID)) return;
                if (!collectedUnique.Add(collectedID)) return; // Falls bereits eingesammelt
                Debug.Log($"[Quest] Collected unique {collectedID}: {collectedUnique.Count}/{requiredItemIDs.Count}");
                break;
        }

        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
    }

    public override bool IsComplete
    {
        get
        {
            return mode switch
            {
                Mode.CountSingleItem => currentCount >= requiredCount,
                Mode.CollectUniqueIDs => requiredItemIDs != null // existieren unser ItemIDs?...
                                        && requiredItemIDs.Count > 0 // Sichergehen das Liste nicht Leer ist
                                        && collectedUnique != null 
                                        && HasAllUnique(),
                _ => false // andere Schreibweise für "default"
                // In diesem Fall: Falls es ein anderer Wert wäre ist es gleich incomplete...
            };
        }
    }

    private bool HasAllUnique() // Sind alle required-IDs da? (Man braucht alle 4 Pages bspw.)
    {
        foreach (var id in requiredItemIDs)
            if (!collectedUnique.Contains(id)) return false;
        return true;
    }

    public override string ProgressText
    {
        get
        {
            return mode switch
            {
                Mode.CountSingleItem => $"{Mathf.Min(currentCount, requiredCount)}/{requiredCount}",
                Mode.CollectUniqueIDs =>
                    $"{(collectedUnique != null ? collectedUnique.Count : 0)}/{(requiredItemIDs != null ? requiredItemIDs.Count : 0)}",
                _ => "" // default
            };
        }
    }

    public override float Progress01 // Fortschritt der Quest
    {
        get
        {
            return mode switch
            {
                Mode.CountSingleItem => requiredCount <= 0 ? 1f : Mathf.Clamp01((float)currentCount / requiredCount),
                Mode.CollectUniqueIDs => (requiredItemIDs == null || requiredItemIDs.Count == 0) ? 1f :
                    Mathf.Clamp01((float)(collectedUnique != null ? collectedUnique.Count : 0) / requiredItemIDs.Count),
                _ => 0f // default
            };
        }
    }
}
