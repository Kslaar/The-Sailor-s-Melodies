using System.Collections.Generic;
using UnityEngine;

public class PuzzleListener : MonoBehaviour
{
    public RecordHotbar recorder;

    public List<SoundSignature> expectedSequence = new();

    public BlockedOffArea target;

    [Header("Rule")]
    public bool resetOnWrong = true;

    [Header("Timer")]
    public bool countStepsTimer = true;
    public float stepTimeoutSeconds = 5f;

    private int _index = 0;
    private bool _playerInRange = false;
    private float _lastCorrectionTime = -999f;

    void OnEnable()
    {
        if (recorder != null) recorder.OnSoundPlayed += HandlePlayed;
    }

    void OnDisable()
    {
        if (recorder != null) recorder.OnSoundPlayed -= HandlePlayed;
    }

    void Update()
    {
        if (!_playerInRange) return;
        if (!countStepsTimer) return;
        if (_index <= 0) return;
        if (Time.time - _lastCorrectionTime > stepTimeoutSeconds)
        {
            _index = 0;
        }
    }

    private void HandlePlayed(SoundSignature played)
    {
        if (!_playerInRange) return;
        if (target != null && target.IsOpened) return;
        if (expectedSequence.Count == 0) return;

        if (countStepsTimer && _index > 0 && (Time.time - _lastCorrectionTime > stepTimeoutSeconds))
        {
            _index = 0;
        }

        if (played == expectedSequence[_index])
        {
            _index++;
            _lastCorrectionTime = Time.time;
            if (_index >= expectedSequence.Count)
            {
                target.Open();
            }
        }
        else
        {
            if (resetOnWrong) _index = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            _index = 0;
        }
    }
}
