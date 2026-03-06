using UnityEngine;

public class QuestZoneTrigger : MonoBehaviour
{
    public string zoneID = "";

    private bool _fired;

    private void OnTriggerEnter(Collider other)
    {
        if (_fired) return;

        if (!HasTagInParents(other.transform, "Player"))
            return;

        _fired = true;
        QuestZoneEvent.RaiseEntered(zoneID);
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
