using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quests/Objectives/Play Sounds Near NPC")]
public class PlaySoundsNearNPCObjective : QuestObjective
{
    [Header("Required (any order)")]
    public List<SoundSignature> required = new();

    [Header("Listener Identity")]
    public string requiredListenerID = ""; 

    [NonSerialized] private HashSet<string> _playedIDs;   
    [NonSerialized] private HashSet<string> _requiredIDs;

    public override void Register()
    {
        _playedIDs = new HashSet<string>();
        _requiredIDs = new HashSet<string>();

        if (required != null)
        {
            foreach (var s in required)
                if (s != null && !string.IsNullOrWhiteSpace(s.id))
                    _requiredIDs.Add(s.id);
        }

        QuestPlaybackEvents.OnPlayedNearListener -= OnPlayedNearListener;
        QuestPlaybackEvents.OnPlayedNearListener += OnPlayedNearListener;

        QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
    }

    public override void Unregister()
    {
        QuestPlaybackEvents.OnPlayedNearListener -= OnPlayedNearListener;
    }

    private void OnPlayedNearListener(string listenerID, SoundSignature sig)
    {
        if (sig == null) return;
        if (_requiredIDs == null || _requiredIDs.Count == 0) return;

        if (!string.IsNullOrWhiteSpace(requiredListenerID) &&
            !string.Equals(requiredListenerID, listenerID, StringComparison.OrdinalIgnoreCase))
            return;

        if (string.IsNullOrWhiteSpace(sig.id)) return;

        if (!_requiredIDs.Contains(sig.id)) return;

        if (_playedIDs.Add(sig.id))
            QuestManager.Instance?.NotifyObjectiveProgressHasChanged();
    }

    public override bool IsComplete
    {
        get
        {
            if (_requiredIDs == null || _requiredIDs.Count == 0) return true;
            if (_playedIDs == null) return false;

            foreach (var id in _requiredIDs)
                if (!_playedIDs.Contains(id)) return false;

            return true;
        }
    }

    public override string ProgressText
    {
        get
        {
            int done = (_playedIDs != null) ? _playedIDs.Count : 0;
            int total = (_requiredIDs != null) ? _requiredIDs.Count : 0;
            return $"{Mathf.Min(done, total)}/{total}";
        }
    }

    public override float Progress01
    {
        get
        {
            int total = (_requiredIDs != null) ? _requiredIDs.Count : 0;
            if (total <= 0) return 1f;
            int done = (_playedIDs != null) ? _playedIDs.Count : 0;
            return Mathf.Clamp01((float)done / total);
        }
    }
}