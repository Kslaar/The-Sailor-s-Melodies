using System.Collections.Generic;
using UnityEngine;

public class PuzzleHintEmitter : MonoBehaviour
{
    public PuzzleListener listener;
    public List<SoundSignature> hintSequence = new();
    public float shortSoundGap = 0.35f;
    public float hintIntervalSeconds = 3f;
    public bool randomizeStartOffset = true;
    public AK.Wwise.Event hintEventOverride;

    public AK.Wwise.RTPC pitchRtpc;
    public float pitchCents = -400f;
    public BlockedOffArea blockedOffArea; // Sobald die Area geöffnet wird wollen wir ja nicht dass der Hint weiterspielt (wird ja über BlockedOffArea geregelt)

    public bool onlyWhilePlayerInTrigger = true;

    private bool _playerInRange;
    private float _nextTime;
    private int _stepIndex;

    void OnEnable()
    {
        float startOffset = randomizeStartOffset ? Random.Range(0f, hintIntervalSeconds) : 0f;
        _nextTime = Time.time + startOffset;
        _stepIndex = 0;
    }

    void Update()
    {
        if (blockedOffArea != null && blockedOffArea.IsOpened) return;
        if (onlyWhilePlayerInTrigger && !_playerInRange) return;

        var sequence = GetSequence();
        if (sequence == null || sequence.Count == 0) return;

        if (Time.time >= _nextTime)
        {
            PlayStep(sequence[_stepIndex]);
            _stepIndex++;

            if (_stepIndex >= sequence.Count)
            {
                _stepIndex = 0;
                _nextTime = Time.time + Mathf.Max(0.1f, hintIntervalSeconds);
            }
            else
            {
                _nextTime = Time.time + Mathf.Max(0.05f, shortSoundGap);
            }
        }
    }

    private IReadOnlyList<SoundSignature> GetSequence()
    {
        if (listener != null && listener.expectedSequence != null && listener.expectedSequence.Count > 0)
            return listener.expectedSequence;

        return hintSequence;
    }
    private void PlayStep(SoundSignature sig)
    {
        if (sig == null)
        {
            Debug.LogWarning($"[PuzzleHintEmitter] Step signature is NULL on {name}");
            return;
        }
        
        var evnt = hintEventOverride != null ? hintEventOverride : sig.playEvent;

        if (evnt == null)
        {
            Debug.LogWarning($"[PuzzleHintEmitter] No hint for signature {name}");
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

        // Verlässt man den HintEmitter während einer Sequence, dann wollen wir dass die Sequence beim erneuten Betreten von Beginn an spielt
        _stepIndex = 0;
        float startOffset = randomizeStartOffset ? Random.Range(0f, Mathf.Max(0.1f, hintIntervalSeconds)) : 0f;
        _nextTime = Time.time + startOffset;
    }
}
