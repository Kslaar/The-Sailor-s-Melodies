using UnityEngine;

public class SoundSource : MonoBehaviour
{
    public SoundSignature signature;
    public AK.Wwise.Event hintPlayEvent;
    public float hintCooldown = 2f;

    private float _lastHintTime;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TryPlayHint();
    }

    public void TryPlayHint()
    {
        if (Time.time < _lastHintTime + hintCooldown) return;
        _lastHintTime = Time.time;

        // Hint-Event zuerst, sonst fallback auf signature.playEvent
        var ev = hintPlayEvent != null ? hintPlayEvent : signature != null ? signature.playEvent : null;
        if (ev != null)
            ev.Post(gameObject);
    }

    public void RecordTo(RecordHotbar recorder)
    {
        if (signature == null || recorder == null) return;

        recorder.Record(signature);
    }
}
