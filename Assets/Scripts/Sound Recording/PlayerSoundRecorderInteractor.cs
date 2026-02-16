using UnityEngine;

public class PlayerSoundRecorderInteractor : MonoBehaviour
{
    public SoundSource CurrentSource { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        var source = other.GetComponent<SoundSource>();
        if (source != null) CurrentSource = source;
    }

    void OnTriggerExit(Collider other)
    {
        var source = other.GetComponent<SoundSource>();
        if (source != null && source == CurrentSource) CurrentSource = null;
    }
}
