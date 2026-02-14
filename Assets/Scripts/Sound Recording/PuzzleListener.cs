using System.Collections.Generic;
using UnityEngine;

public class PuzzleListener : MonoBehaviour
{
    public RecordHotbar recorder;

    public List<SoundSignature> expectedSequence = new();

    public BlockedOffArea target;

    public bool resetOnWrong = true;

    private int _index = 0;
    private bool _playerInRange = false;

    void OnEnable()
    {
        if (recorder != null) recorder.OnSoundPlayed += HandlePlayed;
    }

    void OnDisable()
    {
        if (recorder != null) recorder.OnSoundPlayed -= HandlePlayed;
    }

    private void HandlePlayed(SoundSignature played)
    {
        if (!_playerInRange) return;
        if (target != null && target.IsOpened) return;
        if (expectedSequence.Count == 0) return;

        if (played == expectedSequence[_index])
        {
            _index++;
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
