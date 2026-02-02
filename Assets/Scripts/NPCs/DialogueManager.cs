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

        if (ui == null) ui = FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
    }

    public void StartDialogue(DialogueAsset asset)
    {
        if (asset == null)
        {
            Debug.Log($"[DialogueManager] StartDialogue called mit NULL asset. :(");
            return;
        }
        
        var gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogWarning("[DialogueManager] GameStateManager.Instance is NULL.");
            return;
        }

        /*
        current = asset;
        if (current.nodes == null || current.nodes.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] DialogueAsset hat keine nodes...");
            EndDialogue();
            return;
        }
        */
        if (!gsm.TryEnterDialogue("StartDialogue")) return;

        current = asset;
        node = current.nodes.FirstOrDefault(n => n.id == current.startNodeID);
        //ShowNode();

        StartCoroutine(CoruShowUIWhenReady());
    }

    // UIStateController kann so rechtzeitig Dialogue Panel aktivieren
    private IEnumerator CoruShowUIWhenReady()
    {
        var usc = FindFirstObjectByType<UIStateController>(FindObjectsInactive.Include);
        
        // Frames auf den State Change warten 
        for (int i = 0; i < 10; i++)
        {
            if (usc != null && usc.DialogueUIRoot != null && usc.DialogueUIRoot.activeInHierarchy)
            {
                ui = usc.DialogueUIComponent;
                break;
            }
            yield return null;
        }

        if (ui == null)
        {
            Debug.LogWarning("[DialogueManager] DialogueUI not found in scene.");
            yield break;
        }
        
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

        var dock = FindFirstObjectByType<BoatDockingController>();
        if (dock != null && dock.IsDocked)
            GameStateManager.Instance.TryExitDialogue("Dialogue ended");
        else
            GameStateManager.Instance.TrySetState(GameState.Sailing);
        /*
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        // Falls Inseln erstellt werden mit weiteren Dialogoptionen, muss ich hier nochmal ran
        if (!gsm.TryExitDialogue("Dialog beendet"))
        {
            gsm.TrySetState(GameState.Sailing, "Dialog-beenden-Fallback");
        }
        */
    }
}
