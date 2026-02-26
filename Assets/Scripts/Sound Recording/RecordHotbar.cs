using System.Collections.Generic;
using UnityEngine;
using System;

public class RecordHotbar : MonoBehaviour
{
    [Header("Slots im UI")]
    public int maxSlots = 3;
    public GameObject playbackEmitter;
    public bool stopPreviousOnPlay = false;

    public event Action<SoundSignature> OnSoundPlayed;
    public event Action<IReadOnlyList<SoundSignature>> OnHotbarChanged;
    public event Action<int> OnSelectionChanged;

    private readonly List<SoundSignature> _slots = new();
    private int _selectedIndex = 0;

    private SoundSignature _lastPlayed;

    public IReadOnlyList<SoundSignature> Slots => _slots;
    public int SelectedIndex => _selectedIndex;

    public void Record(SoundSignature sig)
    {
        if (sig == null) { Debug.Log("[Hotbar] Record: sig NULL"); return; }

        Debug.Log($"[Hotbar] Record request: {sig.displayName} ({sig.id})");

        _slots.Remove(sig); // Derselbe Sound soll nicht mehrfach aufgenommen werden können

        if (_slots.Count >= maxSlots)
        {
            Debug.Log($"[Hotbar] Full -> removing oldest {_slots[0].displayName}");
            _slots.RemoveAt(0); // Bei maximum an tragbaren Sounds wir der älteste Sound emtfernt
        }

        _slots.Add(sig);

        // Letzter aufgenommer Sound wird ausgewählt
        _selectedIndex = Mathf.Clamp(_slots.Count - 1, 0, Mathf.Max(0, _slots.Count - 1));
        OnSelectionChanged?.Invoke(_selectedIndex);

        OnHotbarChanged?.Invoke(_slots);
        Debug.Log($"[Hotbar] Recorded. Slots now: {string.Join(", ", _slots.ConvertAll(s => s.displayName))} | selected={_selectedIndex}");

        // REMINDER: Effekte noch einbauen !!!
    }

    public void ClearSelected()
    {
        if (_slots.Count == 0) return;
        Debug.Log($"[Hotbar] ClearSelected index={_selectedIndex} sound={_slots[_selectedIndex].displayName}");
        _slots.RemoveAt(_selectedIndex);

        if (_slots.Count == 0) _selectedIndex = 0;
        else _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _slots.Count - 1);

        OnSelectionChanged?.Invoke(_selectedIndex);
        OnHotbarChanged?.Invoke(_slots);
    }

    public void SelectNext()
    {
        if (_slots.Count == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _slots.Count;
        Debug.Log($"[Hotbar] SelectNext -> {_selectedIndex}");
        OnSelectionChanged?.Invoke(_selectedIndex);
    }
    public void SelectPrevious()
    {
        if (_slots.Count == 0) return;
        _selectedIndex = (_selectedIndex - 1 + _slots.Count) % _slots.Count; // Modulo kann negativ daher "+ _slots.Count"
        Debug.Log($"[Hotbar] SelectPrevious -> {_selectedIndex}");
        OnSelectionChanged?.Invoke(_selectedIndex);
    }

    public void PlaySelected() => PlaySlot(_selectedIndex);

    public void PlaySlot(int index)
    {
        if (index < 0 || index >= _slots.Count)
        {
            Debug.Log($"[Hotbar] PlaySlot invalid index={index}, count={_slots.Count}");
            return;
        }

        var sig = _slots[index];
        if (sig == null) 
        { 
            Debug.Log("[Hotbar] PlaySlot: sig NULL"); 
            return; 
        }

        if (playbackEmitter == null)
        {
            Debug.LogWarning("[Hotbar] playbackEmitter is NULL (set it to PlayerBoat)");
            return;
        }

        Debug.Log($"[Hotbar] Play '{sig.displayName}' on emitter '{playbackEmitter.name}'");

        if (stopPreviousOnPlay && _lastPlayed != null && _lastPlayed.hintEvent != null)
        {
            Debug.Log($"[Hotbar] Stopping previous '{_lastPlayed.displayName}'");
            _lastPlayed.hintEvent.Post(playbackEmitter);
        }
        
        if (sig.playEvent != null)
        {
            sig.playEvent.Post(playbackEmitter);
            _lastPlayed = sig;
        }
        else
        {
            Debug.LogWarning($"SoundSignature '{sig.name}' hat kein playEvent.");
        }

        OnSoundPlayed?.Invoke(sig);

        // UI FEEDBACK nicht vergessen !!!
    }
}
