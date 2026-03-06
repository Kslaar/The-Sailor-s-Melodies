using UnityEngine;

public class QuestHiddenNPCButton : MonoBehaviour
{
    public string questID;

    [SerializeField] private GameObject button;

    public bool showWhileInProgress = true;
    public bool hideWhenCompleted = true;

    private void Awake()
    {
        if (button == null) button = gameObject;
    }

    private void OnEnable()
    {
        Refresh();
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsChanged += Refresh;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsChanged -= Refresh;
    }

    private void Refresh()
    {
        var qm = QuestManager.Instance;
        if (qm == null || string.IsNullOrWhiteSpace(questID)) return;

        bool started = qm.HasStarted(questID);
        var st = qm.GetState(questID);

        bool visible = showWhileInProgress && started && (st == QuestState.Active || st == QuestState.ReadyToTurnIn);

        if (hideWhenCompleted && st == QuestState.Completed)
            visible = false;

        if (button != null && button.activeSelf != visible)
            button.SetActive(visible);
    }
}