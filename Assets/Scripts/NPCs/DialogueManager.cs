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
        current = asset;
        node = current.nodes.FirstOrDefault(n => n.id == current.startNodeID);

        GameStateManager.Instance?.SetState(GameState.Dialogue);

        ShowNode();
    }

    private void ShowNode()
    {
        if (node == null) { EndDialogue(); return; }

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
