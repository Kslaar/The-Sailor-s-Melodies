using UnityEngine;

public class PuzzleHintEmitter : MonoBehaviour
{
    public SoundSignature requiredSignature;
    public AK.Wwise.Event hintEventOverride;
    public float hintIntervalSeconds = 3f;
    public bool randomizeStartOffset = true;

    public AK.Wwise.RTPC pitchRtpc;
    public float pitchCents = -400f;
    public BlockedOffArea blockedOffArea; // Sobald die Area geöffnet wird wollen wir ja nicht dass der Hint weiterspielt (wird ja über BlockedOffArea geregelt)

    public bool onlyWhilePlayerInTrigger = true;

    private bool _playerInRange;
    private float _nextTime;

    void OnEnable()
    {
        float startOffset = randomizeStartOffset ? Random.Range(0f, hintIntervalSeconds) : 0f;
        _nextTime = Time.time + startOffset;
    }

    void Update()
    {
        if (blockedOffArea != null && blockedOffArea.IsOpened) return;
        if (onlyWhilePlayerInTrigger && !_playerInRange) return;

        if (Time.time >= _nextTime)
        {
            PlayHint();
            _nextTime = Time.time + Mathf.Max(0.1f, hintIntervalSeconds);
        }
    }

    private void PlayHint()
    {
        var evnt = hintEventOverride != null ? hintEventOverride : (requiredSignature != null ? requiredSignature.playEvent : null);

        if (evnt == null)
        {
            Debug.LogWarning($"[PuzzleHintEmitter] No hint event set on {name}");
            return;
        }

        if (pitchRtpc != null)
            pitchRtpc.SetValue(gameObject, pitchCents);

        evnt.Post(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!onlyWhilePlayerInTrigger) return;
        if (!other.CompareTag("Player")) return;

        _playerInRange = true;

        /*
        _nextTime = Time.time; // Der Sound wird sofort beim Betreten abgespielt, damit eine solche Area nicht verpasst wird
        */
    }

    private void OnTriggerExit(Collider other)
    {
        if (!onlyWhilePlayerInTrigger) return;
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;
    }
}
