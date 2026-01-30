using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    Active,
    ReadyToComplete,
    Completed,
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<QuestAsset> allQuests = new();

    private readonly HashSet<string> active = new();
    private readonly HashSet<string> completed = new();

    public event Action OnQuestsChanged; // Quest schon fertig?
    public event Action OnObjectiveProgressHasChanged; // laufende Veränderungen in der Quest?

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        foreach (var id in new List<string>(active))
        {
            var q = GetQuest(id);
            if (q == null) continue;

            bool done = true;
            foreach (var obj in q.objectives)
                done &= obj.IsComplete;

            if (done) CompleteQuest(id);
        }
    }

    public void StartQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return;
        if (completed.Contains(questID) || active.Contains(questID)) return;

        var q = GetQuest(questID);
        if (q == null) return;

        active.Add(questID);
        foreach (var obj in q.objectives) obj.Register();

        Debug.Log($"[QuestManager] Quest started: {questID}");
        OnQuestsChanged?.Invoke();
    }

    public void CompleteQuest(string questID)
    {
        if (!active.Remove(questID)) return;
        completed.Add(questID);

        var q = GetQuest(questID);
        if (q != null)
        { 
            foreach (var obj in q.objectives) obj.Unregister();
            foreach (var r in q.rewards) r.Apply();
        }

        Debug.Log($"[QuestManager] Quest completed: {questID}");
        OnQuestsChanged?.Invoke();
    }

    public bool IsActive(string questID) => active.Contains(questID);
    public bool IsCompleted(string questID) => completed.Contains(questID);

    public IEnumerable<string> ActiveQuestIDs => active;
    public IEnumerable<string> CompletedQuestIDs => completed;

    public QuestAsset GetQuest(string id) => allQuests.Find(q => q.questID == id);

    public void NotifyObjectiveProgressHasChanged()
    {
        OnObjectiveProgressHasChanged?.Invoke();
    }
}
