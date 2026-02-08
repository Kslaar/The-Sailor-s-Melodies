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
        if (!HasTagInParents(other.transform, "Player")) return;
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
