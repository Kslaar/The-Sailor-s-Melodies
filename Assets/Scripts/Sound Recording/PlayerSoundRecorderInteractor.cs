using UnityEngine;

public class PlayerSoundRecorderInteractor : MonoBehaviour
{
    public LayerMask RecordableLayer;
    public SoundSource CurrentSource { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        bool inMask = ((1 << other.gameObject.layer) & RecordableLayer) != 0;
        Debug.Log($"[RecorderInteractor] Enter '{other.name}' layer={LayerMask.LayerToName(other.gameObject.layer)} inMask={inMask}");

        if (!inMask) return;

        var source = other.GetComponent<SoundSource>();
        Debug.Log($"[RecorderInteractor] -> SoundSource found? {(source != null)}");

        if (source != null)
        {
            CurrentSource = source;
            Debug.Log($"[RecorderInteractor] CurrentSource = {source.name} signature={(source.signature ? source.signature.displayName : "NULL")}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (CurrentSource == null) return;
        if (other.GetComponent<SoundSource>() == CurrentSource)
        {
            Debug.Log($"[RecorderInteractor] Exit '{other.name}' -> clearing CurrentSource");
            CurrentSource = null;
        }
    }
}
