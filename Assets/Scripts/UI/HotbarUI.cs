using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    public RecordHotbar hotbar;

    [Header("UI")]
    public Transform slotsParent;
    public HotbarSlotUI slotPrefab;

    [Header("Config")]
    public int maxSlots = 3;

    private readonly List<HotbarSlotUI> _slots = new();
    private int _selected = 0;

    void Start()
    {
        Build();
        RefreshAll();
    }

    void OnEnable()
    {
        if (hotbar == null) return;
        hotbar.OnHotbarChanged += OnHotbarChanged;
        hotbar.OnSelectionChanged += OnSelectionChanged;
    }

    void OnDisable()
    {
        if (hotbar == null) return;
        hotbar.OnHotbarChanged -= OnHotbarChanged;
        hotbar.OnSelectionChanged -= OnSelectionChanged;
    }

    void Build()
    {
        foreach (Transform c in slotsParent) Destroy(c.gameObject);
        _slots.Clear();

        for (int i = 0; i < maxSlots; i++)
        {
            var ui = Instantiate(slotPrefab, slotsParent);
            ui.SetEmpty();
            ui.SetSelected(i == 0);
            _slots.Add(ui);
        }
    }

    void OnHotbarChanged(IReadOnlyList<SoundSignature> data) => RefreshAll();

    void OnSelectionChanged(int idx)
    {
        _selected = idx;
        RefreshSelection();
    }

    void RefreshAll()
    {
        var data = hotbar != null ? hotbar.Slots : null;

        for (int i = 0; i < _slots.Count; i++)
        {
            if (data != null && i < data.Count && data[i] != null)
                _slots[i].SetIcon(data[i].icon);
            else
                _slots[i].SetEmpty();
        }

        RefreshSelection();
    }

    void RefreshSelection()
    {
        for (int i = 0; i < _slots.Count; i++)
            _slots[i].SetSelected(i == _selected);
    }
}
