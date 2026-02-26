using System.Collections.Generic;
using UnityEngine;

public class PuzzleHintEmitter : MonoBehaviour
{
    public PuzzleListener listener;

    [Header("Assign events here (stable inspector refs)")]
    public List<SignatureToEvent> map = new();

    public float shortSoundGap = 0.35f;
    public float hintIntervalSeconds = 3f;
    public bool randomizeStartOffset = true;

    public GameObject wwiseEmitterOverride;
    public AK.Wwise.RTPC pitchRtpc;
    public float pitchCents = -400f;

    public BlockedOffArea blockedOffArea;
    public bool onlyWhilePlayerInTrigger = true;

    private bool _playerInRange;
    private float _nextTime;
    private int _stepIndex;

    void OnEnable()
    {
        float startOffset = randomizeStartOffset ? Random.Range(0f, Mathf.Max(0.1f, hintIntervalSeconds)) : 0f;
        _nextTime = Time.time + startOffset;
        _stepIndex = 0;
    }

    void Update()
    {
        if (blockedOffArea != null && blockedOffArea.IsOpened) return;
        if (onlyWhilePlayerInTrigger && !_playerInRange) return;

        var seq = (listener != null) ? listener.expectedSequence : null;
        if (seq == null || seq.Count == 0) return;

        if (Time.time >= _nextTime)
        {
            var sig = seq[_stepIndex];
            var evnt = FindEventFor(sig);
            PlayEvent(evnt);

            _stepIndex++;
            if (_stepIndex >= seq.Count)
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

    private AK.Wwise.Event FindEventFor(SoundSignature sig)
    {
        for (int i = 0; i < map.Count; i++)
            if (map[i].signature == sig)
                return map[i].hintEvent;
        return null;
    }

    private void PlayEvent(AK.Wwise.Event evnt)
    {
        if (evnt == null) return;

        GameObject emitter =
            wwiseEmitterOverride != null ? wwiseEmitterOverride :
            (blockedOffArea != null ? blockedOffArea.gameObject : gameObject);

        if (pitchRtpc != null)
            pitchRtpc.SetValue(emitter, pitchCents);

        evnt.Post(emitter);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!onlyWhilePlayerInTrigger) return;
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!onlyWhilePlayerInTrigger) return;
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;
        _stepIndex = 0;

        float startOffset = randomizeStartOffset ? Random.Range(0f, Mathf.Max(0.1f, hintIntervalSeconds)) : 0f;
        _nextTime = Time.time + startOffset;
    }
}