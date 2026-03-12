using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quests/Objectives/Drive Through Sequence")]
public class SecretSequenceObjective : QuestObjective
{
    [Header("Sequence")]
    public string sequenceID = "secret_sequence";
    public List<int> requiredOrder = new() { 0, 1, 2, 3, 4, 5 }; 

    [Header("Rules")]
    public bool resetOnWrong = true;

    [Header("Timer")] // Weiß ich noch nicht ob ich den drin lasse
    public bool useStepTimeout = true;
    public float stepTimeoutSeconds = 6f;

    [NonSerialized] private int _index;
    [NonSerialized] private float _lastStepTime;
    [NonSerialized] private bool _done;

    public static event Action<string> OnSequenceObjectiveCompleted; // schickt sequenceID

    public override void Register()
    {
        _index = 0;
        _done = false;
        _lastStepTime = -999f;

        SequenceEvent.OnStepTriggered -= OnStep;
        SequenceEvent.OnStepTriggered += OnStep;

        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
        Debug.Log($"[SecretSequenceObjective] Register sequenceID={sequenceID}");
    }

    public override void Unregister()
    {
        SequenceEvent.OnStepTriggered -= OnStep;
    }

    private void OnStep(string seq, int step)
    {
        if (_done) return;
        if (!string.Equals(seq, sequenceID, StringComparison.OrdinalIgnoreCase)) return;
        if (requiredOrder == null || requiredOrder.Count == 0) return;

        // Timeout = reset
        if (useStepTimeout && _index > 0 && Time.time - _lastStepTime > stepTimeoutSeconds)
            _index = 0;

        int expected = requiredOrder[_index];

        Debug.Log($"got step={step} expected={expected} idx={_index} seq={seq}");

        if (step == expected)
        {
            _index++;
            _lastStepTime = Time.time;

            if (_index >= requiredOrder.Count)
            {
                _done = true;
                Debug.Log($"COMPLETE sequenceID={sequenceID}");

                QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
                // Event los, Scene reagiert
                OnSequenceObjectiveCompleted?.Invoke(sequenceID);
            }
            else
            {
                QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
            }
        }
        else
        {
            Debug.Log($"COMPLETE sequenceID={sequenceID}");
            if (resetOnWrong) _index = 0;
            QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
        }
    }

    public override bool IsComplete => _done;

    public override string ProgressText
        => requiredOrder == null || requiredOrder.Count == 0 ? "Done" : $"{Mathf.Clamp(_index, 0, requiredOrder.Count)}/{requiredOrder.Count}";

    public override float Progress01
        => requiredOrder == null || requiredOrder.Count == 0 ? 1f : Mathf.Clamp01((float)_index / requiredOrder.Count);
}