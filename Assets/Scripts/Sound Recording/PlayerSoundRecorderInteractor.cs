using UnityEngine;

public class PlayerSoundRecorderInteractor : MonoBehaviour
{
    public LayerMask RecordableLayer;
    public SoundSource CurrentSource { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & RecordableLayer) == 0) return;

        var source = other.GetComponent<SoundSource>();
        if (source != null) CurrentSource = source;
    }

    void OnTriggerExit(Collider other)
    {
        if (CurrentSource == null) return;
        if (other.GetComponent<SoundSource>() == CurrentSource) CurrentSource = null;
    }
}
