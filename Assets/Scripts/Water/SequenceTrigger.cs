using UnityEngine;

public class SequenceTrigger : MonoBehaviour
{
    public string sequenceID = "";
    public int stepIndex = 0;

    private bool _playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!HasTagInParents(other.transform, "Player"))
            return;
        if (_playerInside) return;
        _playerInside = true;

        SequenceEvent.Raise(sequenceID, stepIndex);
        Debug.Log($"[SequenceTrigger] seq={sequenceID} step={stepIndex} by={other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!HasTagInParents(other.transform, "Player"))
            return;
        
        _playerInside = false;
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