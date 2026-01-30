using System.Collections;
using System.Linq;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI ui;

    private DialogueAsset current;
    private DialogueAsset.DialogueNode node;

    private void Awake()
    {
        if (Instance != null)  { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartDialogue(DialogueAsset asset)
    {
        if (asset == null)
        {
            Debug.Log($"[DialogueManager] StartDialogue called mit NULL asset. :(");
            return;
        }

        if (ui == null)
            ui = FindFirstObjectByType<DialogueUI>();

        current = asset;

        if (current.nodes == null || current.nodes.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] DialogueAsset hat keine nodes...");
            EndDialogue();
            return;
        }
        node = current.nodes.FirstOrDefault(n => n.id == current.startNodeID);
        ShowNode();
    }

    private void ShowNode()
    {
        Debug.Log($"[DialogueManager] ShowNode. current={(current != null ? current.name : "NULL")} node={(node != null ? node.id : "NULL")} startId={(current != null ? current.startNodeID : "NULL")}");

        if (node == null)
        {
            Debug.LogWarning("[DialogueManager] Node is NULL. Check startNodeId and node ids in the DialogueAsset.");
            EndDialogue();
            return;
        }

        foreach (var act in node.actionsOnEnter) act?.Execute();

        ui.Show(current.npcName, current.npcAvatar, node.text, node.choices, OnChoice);
    }

    private void OnChoice(DialogueAsset.Choice choice)
    {
        foreach (var act in choice.actionsOnChoose) act?.Execute();

        if (string.IsNullOrEmpty(choice.nextNodeID))
        {
            EndDialogue();
            return;
        }

        node = current.nodes.FirstOrDefault(n => n.id == choice.nextNodeID);
        ShowNode();
    }

    public void EndDialogue()
    {
        if (ui != null) ui.Hide();

        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        // Falls Inseln erstellt werden mit weiteren Dialogoptionen, muss ich hier nochmal ran
        if (!gsm.TryExitDialogue("Dialog beendet"))
        {
            gsm.TrySetState(GameState.Sailing, "Dialog-beenden-Fallback");
        }
    }
}
