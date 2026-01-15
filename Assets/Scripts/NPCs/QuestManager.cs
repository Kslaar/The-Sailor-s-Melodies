using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<QuestAsset> allQuests = new();

    private readonly HashSet<string> active = new();
    private readonly HashSet<string> completed = new();
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
        if (completed.Contains(questID) || active.Contains(questID)) return;
        active.Add(questID);

        var q = GetQuest(questID);
        if (q == null) return;
        foreach (var obj in q.objectives) obj.Register();
    }

    public void CompleteQuest(string questID)
    {
        if (!active.Remove(questID)) return;
        completed.Add(questID);

        var q = GetQuest(questID);
        if (q == null) return;

        foreach (var obj in q.objectives) obj.Unregister();
        foreach (var r in q.rewards) r.Apply();
    }

    public IEnumerable<string> ActiveQuestIDs => active;
    public IEnumerable<string> CompletedQuestIDs => completed;

    public QuestAsset GetQuest(string id) => allQuests.Find(q => q.questID == id);
}
