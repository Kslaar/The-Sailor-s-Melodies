using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quests/Objectives/Complete Race")]
public class CompleteRaceObjective : QuestObjective
{
    public string courseID;

    [System.NonSerialized] private bool done;

    public override bool IsComplete => done;
    public override string ProgressText => done ? "Done" : "0/1";
    public override float Progress01 => done ? 1f : 0f;

    public override void Register()
    {
        done = false;
        if (RaceManager.Instance != null)
            RaceManager.Instance.OnRaceFinished += HandleFinished;
    }

    public override void Unregister()
    {
        if (RaceManager.Instance != null)
            RaceManager.Instance.OnRaceFinished -= HandleFinished;
    }

    private void HandleFinished(string finishedCourseID, float time)
    {
        if (done) return;
        if (finishedCourseID != courseID) return;
        done = true;
        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
    }
}
