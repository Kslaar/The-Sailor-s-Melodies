using UnityEngine;

public class RaceTrigger : MonoBehaviour
{
    public enum TriggerType { Checkpoint, Finish }

    // [SerializeField] private string requiredTag = "Player";
    public TriggerType triggerType = TriggerType.Checkpoint;
    public string courseID;
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[RaceTrigger] HIT {name} type={triggerType} courseID={courseID} idx={checkpointIndex} other={other.name}");
    
        if (!HasTagInParents(other.transform, "Player"))
            return;

        var gsm = GameStateManager.Instance;
        if (gsm == null || gsm.State != GameState.Racing)
            return;
    
        RaceManager.Instance?.OnTriggerHit(courseID, triggerType, checkpointIndex);
    }


    private static bool HasTagInParents(Transform t, string tag)
    {
        while (t != null)
        {
            if (t.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }
}
