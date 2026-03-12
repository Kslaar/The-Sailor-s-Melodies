using System.Collections;
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
            return;
        }

        if (RaceManager.Instance == null)
        {
            return;
        }

        RaceManager.Instance.StartCoroutine(StartRaceNextFrame(course));
    }

    private IEnumerator StartRaceNextFrame(RaceCourse course)
    {
        yield return null; // 1 Frame auf den State und das UI warten...
        RaceManager.Instance.StartRace(course);
    }

    private RaceCourse FindCourse(string id)
    {
        foreach (var c in Object.FindObjectsByType<RaceCourse>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (c != null && c.courseID == id) return c;
        return null;
    }
}
