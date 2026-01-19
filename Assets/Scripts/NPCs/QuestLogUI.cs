using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Text")]
    [SerializeField] private  TMP_Text activeQuest;
    [SerializeField] private  TMP_Text completedQuest;

    // [Header("Input")]
    // [SerializeField] private  Key toggleKey = Key.T; 

    [Header("Cursor")]
    [SerializeField] private CursorLockController cursorLock;

    private GameState previousState = GameState.Sailing;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.tKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        bool newState = !(root != null && root.activeSelf);
        if (root != null) root.SetActive(newState);

        if (newState)
        {
            previousState = GameStateManager.Instance != null ? GameStateManager.Instance.State : GameState.Sailing;
            GameStateManager.Instance?.SetState(GameState.QuestLog);

            if (cursorLock != null) cursorLock.UnlockCursor();
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Refresh();
        }
        else
        {
            GameStateManager.Instance?.SetState(previousState);

            bool shouldLock = previousState == GameState.Sailing;

            if (cursorLock != null)
            {
                if (shouldLock) cursorLock.LockCursor();
                else cursorLock.UnlockCursor();
            }
            else
            {
                Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !shouldLock;
            }
        }
    }

    public void Refresh()
    {
        var qm = QuestManager.Instance;
        if (qm == null) return;

        if (activeQuest != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Active Quest<b>");
            foreach (var id in qm.ActiveQuestIDs)
            {
                var q = qm.GetQuest(id);
                sb.AppendLine(q != null ? $"• {q.title}" : $"• {id}");
            }
            activeQuest.text = sb.ToString();
        }

        if (completedQuest != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Completed Quest<b>");
            foreach (var id in qm.CompletedQuestIDs)
            {
                var q = qm.GetQuest(id);
                sb.AppendLine(q != null ? $"• {q.title}" : $"• {id}");
            }
            completedQuest.text = sb.ToString();
        }
    }
}
