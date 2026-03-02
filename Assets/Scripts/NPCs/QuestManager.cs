using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    Active,
    ReadyToTurnIn,
    Completed,
}

[System.Serializable]
public class QuestStateEntry
{
    public string questID;
    public QuestState state;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<QuestAsset> allQuests = new();

    private readonly Dictionary<string, QuestState> stateByID = new();

    public event Action OnQuestsChanged; // Quest schon fertig?
    public event Action OnObjectiveProgressHasChanged; // laufende Veränderungen in der Quest?

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        foreach (var id in new List<string>(stateByID.Keys))
        {
            if (stateByID[id] != QuestState.Active) continue;

            var q = GetQuest(id);
            if (q == null) continue;

            bool objectivesDone = true;
            foreach (var obj in q.objectives)
            {
                if (obj == null) continue;
                objectivesDone &= obj.IsComplete;
            }

            if (objectivesDone)
            {
                stateByID[id] = QuestState.ReadyToTurnIn;
                Debug.Log($"[QuestManager] Quest ready to turn in: {id}");
                OnQuestsChanged?.Invoke();
            }
        }
    }

    public void StartQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return;
        
        // Wenn wir die Quest bereist angenommen/beendet haben, kann sie nicht erneut gestarted werden
        if (stateByID.ContainsKey(questID)) return;

        var q = GetQuest(questID);
        if (q == null)
        {
            Debug.LogWarning($"[QuestManager] StartQuest: Quest '{questID}' nicht gefunden in allQuests");
            return;
        }

        stateByID[questID] = QuestState.Active;

        foreach (var obj in q.objectives)
        {
            if (obj == null) continue;
            obj.Register();
        }

        Debug.Log($"[QuestManager] Quest begonnen: {questID}");
        OnQuestsChanged?.Invoke();
    }

    public void TurnInQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return;

        if (!stateByID.TryGetValue(questID, out var st)) return;

        if (st != QuestState.ReadyToTurnIn)
        {
            return;
        }

        var q = GetQuest(questID);
        if (q == null) return;

        if (q.rewards != null)
        {
            foreach (var r in q.rewards)
            {
                if (r == null) continue; // Solange ich noch keine Questassets habe nullsicher
                r.Apply();
            }
        }

        stateByID[questID] = QuestState.Completed;
        Debug.Log($"[QuestManager] Quest eingereicht + abgeschlossen: {questID}");
        OnQuestsChanged?.Invoke();
        NotifyObjectiveProgressHasChanged();
    }

    public void ForceSetState(string questID, QuestState state)
    {
        if (string.IsNullOrWhiteSpace(questID)) return;

        stateByID[questID] = state;
        OnQuestsChanged?.Invoke();
        NotifyObjectiveProgressHasChanged();
    }

    public QuestAsset GetQuest(string id) => allQuests.Find(q => q.questID == id);
    public QuestState GetState(string questID)
        => stateByID.TryGetValue(questID, out var st) ? st : default;
    public bool IsActive(string questID) 
        => stateByID.TryGetValue(questID, out var st) && st == QuestState.Active;
    public bool IsReadyToTurnIn(string questID) 
        => stateByID.TryGetValue(questID, out var st) && st == QuestState.ReadyToTurnIn;
    public bool IsCompleted(string questID) 
        => stateByID.TryGetValue(questID, out var st) && st == QuestState.Completed;

    public IEnumerable<string> StartedQuestIDs => stateByID.Keys;

    public void NotifyObjectiveProgressHasChanged()
    {
        OnObjectiveProgressHasChanged?.Invoke();
    }

    public bool HasStarted(string questID) => stateByID.ContainsKey(questID);

    public List<QuestStateEntry> ExportStates()
    {
        var list = new List<QuestStateEntry>(stateByID.Count);
        foreach (var keyvalue in stateByID)
            list.Add(new QuestStateEntry { questID = keyvalue.Key, state = keyvalue.Value});
        return list;
    }

    public void ImportStates(List<QuestStateEntry> list)
    {
        stateByID.Clear();
        if (list == null) return;

        foreach (var entry in list)
        {
            if (string.IsNullOrWhiteSpace(entry.questID)) continue;
            stateByID[entry.questID] = entry.state;
        }

        OnQuestsChanged?.Invoke();
        NotifyObjectiveProgressHasChanged();
        Debug.Log($"[QuestManager] Imported {stateByID.Count} quest states");
    }

    public void ReRegisterActiveObjectives()
    {
        int registered = 0;
        foreach (var id in new List<string>(stateByID.Keys))
        {
            if (stateByID[id] != QuestState.Active) continue;

            var q = GetQuest(id);
            if (q == null) continue;

            foreach (var obj in q.objectives)
            {
                obj?.Register();
                registered++;
            }
            Debug.Log($"[QuestManager] ReRegisterActiveObjectives: registered {registered} objectives");
        }
        NotifyObjectiveProgressHasChanged();
    }
}
