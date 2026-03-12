using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quests/Objectives/Enter Zone")]
public class EnterZoneObjective : QuestObjective
{
    public string requiredZoneID = "";

    [NonSerialized] private bool _done;

    public override void Register()
    {
        _done = false;
        QuestZoneEvent.OnZoneEntered -= OnZoneEntered;
        QuestZoneEvent.OnZoneEntered += OnZoneEntered;

        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
    }

    public override void Unregister()
    {
        QuestZoneEvent.OnZoneEntered -= OnZoneEntered;
    }

    private void OnZoneEntered(string zoneID)
    {
        if (_done) return;
        if (!string.Equals(zoneID, requiredZoneID, StringComparison.OrdinalIgnoreCase))
            return;

        _done = true;
        Debug.Log($" Quest Completed: entered '{requiredZoneID}'");
        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
    }

    public override bool IsComplete => _done;
    public override string ProgressText => _done ? "Done" : "0/1";
    public override float Progress01 => _done ? 1f : 0f;
}