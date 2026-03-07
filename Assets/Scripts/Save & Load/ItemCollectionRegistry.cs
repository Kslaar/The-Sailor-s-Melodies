using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ItemCollectionRegistry : MonoBehaviour
{
    [SerializeField] private List<ItemCountCap> caps = new();
    private Dictionary<string, int> capID;
    public static event Action OnRegistryLoaded;
    public static ItemCollectionRegistry Instance { get; private set; }

    private readonly Dictionary<string, int> counts = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }   
        Instance = this;
        DontDestroyOnLoad(gameObject);

        capID = new Dictionary<string, int>();
        foreach (var entry in caps)
        {
            if (entry == null) continue;
            if (string.IsNullOrWhiteSpace(entry.itemID)) continue;
            capID[entry.itemID] = Mathf.Max(1, entry.maxCount);
        }
    }

    private void OnEnable()
    {
        ItemPickupEvents.OnItemCollected -= OnCollected;
        ItemPickupEvents.OnItemCollected += OnCollected;
    }

    private void OnDisable()
    {
        ItemPickupEvents.OnItemCollected -= OnCollected;
    }

    private void OnCollected(string itemID)
    {
        if (string.IsNullOrWhiteSpace(itemID))
            return;
        if (!counts.ContainsKey(itemID)) 
            counts[itemID] = 0;

        int next = counts[itemID] + 1;

        if (capID != null && capID.TryGetValue(itemID, out int cap))
            next = Mathf.Min(next, cap);

        counts[itemID] = next;
    }

    public int GetCount(string itemID)
    {
        if (string.IsNullOrWhiteSpace(itemID))
            return 0;
        return counts.TryGetValue(itemID, out var c) ? c : 0;
    } 

    public bool HasAtLeast(string itemID, int required) => GetCount(itemID) >= required;

    // Hier für den Save (= export)
    public List<ItemCountEntry> Export()
    {
        var list = new List<ItemCountEntry>(counts.Count);
        foreach (var keyvalue in counts)
            list.Add(new ItemCountEntry { itemID = keyvalue.Key, count = keyvalue.Value});
        return list;
    }

    // Und dann hier der Load (also der Import)
    public void Import(List<ItemCountEntry> list)
    {
        counts.Clear();
        if (list != null)
        {
            foreach (var entry in list)
            {
                if (string.IsNullOrWhiteSpace(entry.itemID)) continue;
                counts[entry.itemID] = Mathf.Max(0, entry.count);
            }
        }

        Debug.Log($"[ItemRegistry] Imported {counts.Count} item IDs");
        OnRegistryLoaded?.Invoke();
    }
}

[System.Serializable]
public class ItemCountEntry
{
    public string itemID;
    public int count;
}

[System.Serializable]
public class ItemCountCap
{
    public string itemID;
    public int maxCount = 3;
}
