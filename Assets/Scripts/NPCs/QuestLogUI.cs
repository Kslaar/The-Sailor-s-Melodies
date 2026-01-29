using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Columns")]
    [SerializeField] private Transform activeParent;
    [SerializeField] private Transform completedParent;

    [Header("Prefab")]
    [SerializeField] private Button questButtonPrefab;

    private readonly List<GameObject> spawned = new();

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestsChanged += Refresh;
            QuestManager.Instance.OnObjectiveProgressHasChanged += Refresh; // Progress neu zeichnen
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestsChanged -= Refresh;
            QuestManager.Instance.OnObjectiveProgressHasChanged -= Refresh;
        }
    }

    public void Open()
    {
        if (root != null) root.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (root != null) root.SetActive(false);
    }

    public void Toggle()
    {
        if (root == null) return;
        if (root.activeSelf) Close();
        else Open();
    }

    public void Refresh()
    {
        ClearSpawned();

        var qm = QuestManager.Instance;
        if (qm == null) return;

        // Aktive Quests links
        foreach (var id in qm.ActiveQuestIDs)
            AddQuestRow(id, activeParent, isCompleted: false);

        // Fertige Quests rechts
        foreach (var id in qm.CompletedQuestIDs)
            AddQuestRow(id, completedParent, isCompleted: true);
    }

    private void AddQuestRow(string questId, Transform parent, bool isCompleted)
    {
        if (parent == null || questButtonPrefab == null) return;

        var qm = QuestManager.Instance;
        var q = qm.GetQuest(questId);
        if (q == null) return;

        var btn = Instantiate(questButtonPrefab, parent);
        spawned.Add(btn.gameObject);

        var label = btn.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            if (isCompleted)
            {
                label.text = $"{q.title} ✅";
            }
            else
            {
                string progress = GetProgressSummary(q);
                label.text = string.IsNullOrEmpty(progress)
                    ? q.title
                    : $"{q.title} [{progress}]";
            }
        }

        /*
        btn.onClick.AddListener(() =>
        {
            Debug.Log($"[QuestLogUI] Clicked quest: {questId}");
        });
        */
    }

    private string GetProgressSummary(QuestAsset q)
    {
        if (q.objectives == null || q.objectives.Count == 0) return "";
        return q.objectives[0].ProgressText; // Erstes objective anzeigen
    }

    private void ClearSpawned()
    {
        foreach (var go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
    }
}
