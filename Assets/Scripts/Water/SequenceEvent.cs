using System;

public static class SequenceEvent
{
    public static event Action<string, int> OnStepTriggered;

    public static void Raise(string sequenceID, int stepIndex)
        => OnStepTriggered?.Invoke(sequenceID, stepIndex);
}