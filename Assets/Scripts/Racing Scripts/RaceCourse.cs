using System.Collections.Generic;
using UnityEngine;

public class RaceCourse : MonoBehaviour
{
    [Header("Island Identity")]
    public string courseID = "race_island_1";

    [Header("Return Dialogue")]
    public DockZone returnDock;
    public Transform returnPoint;
    public NPCDialogueSelector questGiverDialogue;
    public string questID;

    [Header("The Course")]
    public Transform startPoint;
    public Collider finishTrigger; 
    public List<Collider> checkpoints = new(); // Spieler soll ja nicht einfach sich zur finishline drehen können

    [Header("Rules & Time")]
    public float successTimeSeconds = 120f; 
    public float maxTimeSeconds = 210f; // Immer größer als successTimeSeconds
    public float maxDistanceFromIsland = 80f; // Wie weit vom nächsten Checkpoint es erlaubt ist zu sein
    public float outOfBoundsGracePeriod = 5f;

    private void Awake()
    {
        var rm = RaceManager.Instance;

        if (rm != null && rm.CurrentCourse == this)
            return;
            
        SetRaceTriggersActive(false);
    }
    public IEnumerable<Vector3> GetAnchorPositions()
    {
        if (startPoint != null) yield return startPoint.position;
        if (finishTrigger != null) yield return finishTrigger.bounds.center;
        foreach (var cp in checkpoints)
            if (cp != null) yield return cp.bounds.center;
    }

    public void SetRaceTriggersActive(bool active)
    {
        if (finishTrigger != null) finishTrigger.enabled = active;
        if (checkpoints != null)
            foreach (var cp in checkpoints)
                if (cp != null) cp.enabled = active;
    }
}
