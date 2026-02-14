using UnityEngine;

public class SoundSource : MonoBehaviour
{
    public SoundSignature signature;
    public AudioSource hintSource;

    public void PlayHint()
    {
        if (signature != null && signature.previewClip != null && hintSource != null)
        {
            hintSource.clip = signature.previewClip;
            hintSource.Play();
        }
    }

    public void RecordTo(RecordHotbar recorder)
    {
        recorder.Record(signature);
    }
}
