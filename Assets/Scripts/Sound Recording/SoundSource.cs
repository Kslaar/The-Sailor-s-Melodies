using UnityEngine;

public class SoundSource : MonoBehaviour
{
    public SoundSignature signature;
    public AK.Wwise.Event hintPlayEvent;

    public void PlayHint()
    {
        if (hintPlayEvent != null) 
            hintPlayEvent.Post(gameObject);
        else if (signature != null && signature.playEvent != null)
        {
            signature.playEvent.Post(gameObject);
        }
    }

    public void RecordTo(RecordHotbar recorder)
    {
        if (signature == null || recorder == null) return;

        recorder.Record(signature);
    }
}
