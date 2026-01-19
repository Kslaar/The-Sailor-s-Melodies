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

        if (ui == null) ui = FindFirstObjectByType<DialogueUI>();
    }

    public void StartDialogue(DialogueAsset asset)
    {
        Debug.Log($"[DialogueManager] StartDialogue called. asset={(asset != null ? asset.name : "NULL")} ui={(ui != null ? ui.name : "NULL")}");

        if (ui == null)
        {
            Debug.LogWarning("[DialogueManager] DialogueUI reference is NULL. Assign it in the Inspector.");
            return;
        }
        current = asset;
        node = current.nodes.FirstOrDefault(n => n.id == current.startNodeID);

        GameStateManager.Instance?.SetState(GameState.Dialogue);

        ShowNode();
    }

    private void ShowNode()
    {
        Debug.Log($"[DialogueManager] ShowNode. current={(current != null ? current.name : "NULL")} node={(node != null ? node.id : "NULL")} startId={(current != null ? current.startNodeID : "NULL")}");

        if (ui == null) return;
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
        ui.Hide();
        GameStateManager.Instance?.SetState(GameState.Docked);
    }
}
