using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("List")]
    [SerializeField] private RectTransform listParent;
    [SerializeField] private Button questButtonPrefab;

    private readonly List<GameObject> spawned = new();
    private Coroutine _openRoutine;

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
        if (root == null) return;
        root.SetActive(true);

        if(_openRoutine != null) StopCoroutine(_openRoutine);
        _openRoutine = StartCoroutine(OpenNextFrame());
    }

    public void Close()
    {
        if (_openRoutine != null)
        {
            StopCoroutine(_openRoutine);
            _openRoutine = null;
        }
        if (root != null) root.SetActive(false);
    }

    private IEnumerator OpenNextFrame()
    {
        yield return null;
        Refresh();

        Canvas.ForceUpdateCanvases();
        if (listParent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(listParent);
    }

    public void Refresh()
    {
        ClearSpawned();

        var qm = QuestManager.Instance;
        if (qm == null) return;

        foreach (var id in qm.StartedQuestIDs)
        {
            var q = qm.GetQuest(id);
            if (q == null) continue;

            var btn = Instantiate(questButtonPrefab, listParent);
            spawned.Add(btn.gameObject);

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                var st = qm.GetState(id);

                string progress = GetProgressSummary(q);
                string stateSuffix = 
                    st == QuestState.ReadyToTurnIn ? " (Ready to turn in)" : st == QuestState.Completed ? " (Completed)" : "";


                label.text = $"{q.title}{stateSuffix}" + (string.IsNullOrEmpty(progress) ? "" : $" [{progress}]");
                ApplyStyle(label, st);
            }
        }
    }

    private string GetProgressSummary(QuestAsset q)
    {
        if (q.objectives == null || q.objectives.Count == 0) return "";

        var obj = q.objectives[0];
        if (obj == null) return "";
        return obj.ProgressText;
    }

    private void ApplyStyle(TMP_Text label, QuestState st)
    {
        label.fontStyle &= ~FontStyles.Strikethrough;

        var c = label.color;

        if (st == QuestState.Completed)
        {
            label.fontStyle |= FontStyles.Strikethrough;
            c.a = 0.5f; // Wir setzen das Alpha auf die Hälfte
            label.color = c;
        }
        else
        {
            c.a = 1f;
            label.color = c;
        }
    }

    private void ClearSpawned()
    {
        foreach (var go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
    }
}
