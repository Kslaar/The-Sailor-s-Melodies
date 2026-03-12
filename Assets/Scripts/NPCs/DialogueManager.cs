using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public bool AllowBack { get; set; } = true;

    [SerializeField] private DialogueUI ui;
    private DialogueAsset current;
    private DialogueAsset.DialogueNode node;

    private void Awake()
    {
        if (Instance != null)  { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nach jedem SceneLoad UI neu zuweisen
        StartCoroutine(RebindUINextFrame());
    }

    private IEnumerator RebindUINextFrame()
    {
        yield return null; // 1 Frame warten, UIStateController da ist
        BindUIFromScene();
    }

    private void BindUIFromScene()
    {
        var usc = FindFirstObjectByType<UIStateController>(FindObjectsInactive.Include);
        if (usc != null)
        {
            var comp = usc.DialogueUIComponent;
            if (comp != null)
            {
                ui = comp;
                return;
            }
        }

        ui = FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
    }

    public void StartDialogue(DialogueAsset asset)
    {
        AllowBack = true;

        if (asset == null)
        {
            Debug.Log($"[DialogueManager] StartDialogue called: mit NULL asset.");
            return;
        }
        
        var gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogWarning("[DialogueManager] GameStateManager.Instance is NULL.");
            return;
        }

        if (!gsm.TryEnterDialogue("StartDialogue")) return;

        current = asset;
        node = current.nodes.FirstOrDefault(n => n.id == current.startNodeID);

        StartCoroutine(CoruShowUIWhenReady());
    }

    // UIStateController kann so rechtzeitig Dialogue Panel aktivieren
    private IEnumerator CoruShowUIWhenReady()
    {
        if (ui == null) BindUIFromScene();

        for (int i = 0; i < 30 && ui == null; i++) // 30 Frames warten wegen Asyncload
        {
            BindUIFromScene();
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
            EndDialogue();
            return;
        }

        foreach (var act in node.actionsOnEnter) act?.Execute();

        ui.Show(current.npcName, current.npcAvatar, node.text, node.choices, OnChoice);

        //Wwise Event für die Dialogzeile
        if (node.wwiseEvent != null)
            node.wwiseEvent.Post(gameObject);
    }

    private void OnChoice(DialogueAsset.Choice choice)
    {
        foreach (var act in choice.actionsOnChoose) act?.Execute();

        if (choice.wwiseEvent != null)
            choice.wwiseEvent.Post(gameObject);

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

        if (gsm.State == GameState.Racing)
        {
            return;
        }
        if (gsm.State == GameState.Dialogue)
        {
            gsm.TryExitDialogue("Dialogue ended");
            return;    
        }

        if (gsm.State != GameState.Docked)
            gsm.TrySetState(GameState.Sailing, "Dialogue ended (Fallback)");
    }
}
