using System;
using UnityEngine;

public static class QuestPlaybackEvents
{
    public static event Action<string, SoundSignature> OnPlayedNearListener;

    public static void RaisePlayedNearListener(string listenerID, SoundSignature sig)
    {
        OnPlayedNearListener?.Invoke(listenerID, sig);
    }
}