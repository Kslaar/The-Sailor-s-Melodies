using UnityEngine;

[CreateAssetMenu(menuName ="Game/DialogueActions/Start Race")]
public class StartRaceAction : DialogueAction
{
    public string courseID;
    public string questIDToStart;

    public override void Execute()
    {
        if (!string.IsNullOrWhiteSpace(questIDToStart))
            QuestManager.Instance?.StartQuest(questIDToStart);

        var course = FindCourse(courseID);
        if (course == null)
        {
            Debug.LogWarning($"[StartRaceAction] Course '{courseID}' not found in scene.");
            return;
        }

        if (RaceManager.Instance == null)
        {
            Debug.LogWarning("[StartRaceAction] RaceManager.Instance is NULL.");
            return;
        }

        DialogueManager.Instance?.EndDialogue();
        RaceManager.Instance.StartRace(course);
    }

    private RaceCourse FindCourse(string id)
    {
        foreach (var c in Object.FindObjectsByType<RaceCourse>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (c != null && c.courseID == id) return c;
        return null;
    }
}
