using System.Collections;
using UnityEngine;

public class UIStateController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject sailingHUD;
    [SerializeField] private GameObject dockUI;
    [SerializeField] private GameObject dialogueUIRoot;
    [SerializeField] private QuestLogUI questlogUI;
    [SerializeField] private GameObject settingsUI;

    private GameStateManager gsm;
    // private GameState lastNonPausedState = GameState.Sailing;

    public DialogueUI DialogueUIComponent
        => dialogueUIRoot != null ? dialogueUIRoot.GetComponentInChildren<DialogueUI>(true) : null;

    public GameObject DialogueUIRoot => dialogueUIRoot;

    private IEnumerator Start()
    {
        int safety = 1;
        while (GameStateManager.Instance == null && safety-- > 0)
            yield return null;
        
        gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogError("[UIStateController] GameStateManager.Instance ist NULL!!!");
            yield break;
        }

        gsm.OnStateChanged += StateChanged;
        Apply(gsm.State);
    }

    private void OnDestroy()
    {
        if (gsm != null) gsm.OnStateChanged -= StateChanged;
    }

    private void StateChanged(GameState from, GameState to)
    {
        Apply(to);
    }

    private void Apply(GameState state)
    {
        SetActiveSafe(sailingHUD, false);
        SetActiveSafe(dockUI, false);
        SetActiveSafe(dialogueUIRoot, false);
        if (questlogUI != null) questlogUI.Close();
        SetActiveSafe(settingsUI, false);

        switch(state)
        {
            case GameState.Sailing:
                SetActiveSafe(sailingHUD, true);
                break;
            case GameState.Docked:
                SetActiveSafe(dockUI, true);
                break;
            case GameState.Dialogue:
                SetActiveSafe(dialogueUIRoot, true);
                break;
            case GameState.QuestLog:
                if (questlogUI != null) questlogUI.Open();
                break;
            case GameState.Paused:
                SetActiveSafe(settingsUI, true);
                break;
        }

        // Ist DialoguePanel jetzt endlich aktiv?!
        if (dialogueUIRoot != null)
        {
            Debug.Log($"[UIStateController] Apply {state} | DialogueRoot activeInHierarchy={dialogueUIRoot.activeInHierarchy}");
        }
    }
    /*
    private void OnEnable()
    {
        TryBind();
    }
    private void Start()
    {
        TryBind();
        ApplyNow();
    }
    private void OnDisable()
    {
        Unbind();
    }
    private void OnDestroy()
    {
        Unbind();
    }

    private void TryBind()
    {
        if (gsm != null) return;

        gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogError("[UIStateController] GameStateManager.Instance ist NULL!!!");
            return;
        }

        gsm.OnStateChanged -= StateChanged;
        gsm.OnStateChanged += StateChanged;
    }

    private void Unbind()
    {
        if (gsm == null) return;
        gsm.OnStateChanged -= StateChanged;
    }

    private void ApplyNow()
    {
        if (gsm == null) return;

        if (gsm.State == GameState.Paused)
            lastNonPausedState = gsm.State;

        ApplyBase(gsm.State == GameState.Paused ? lastNonPausedState : gsm.State);
        ApplyOverlay(gsm.State);
    }

    private void StateChanged(GameState from, GameState to)
    {
        Debug.Log($"[UIStateController] Apply {from} -> {to}");

        if (to != GameState.Paused)
            lastNonPausedState = to;

        ApplyBase(to == GameState.Paused ? lastNonPausedState : to);

        ApplyOverlay(to);
    }

    private void ApplyBase(GameState baseState)
    {
        SetActiveSafe(sailingHUD, false);
        SetActiveSafe(dockUI, false);
        SetActiveSafe(dialogueUI, false);
        if (questlogUI != null) questlogUI.Close();

        switch (baseState)
        {
            case GameState.Sailing:
                SetActiveSafe(sailingHUD, true);
                break;
            case GameState.Docked:
                SetActiveSafe(dockUI, true);
                break;
            case GameState.Dialogue:
                SetActiveSafe(dialogueUI, true);
                break;
            case GameState.QuestLog:
                if (questlogUI != null) questlogUI.Open();
                break;
        }
    }

    private void ApplyOverlay(GameState currentState)
    {
        SetActiveSafe(settingsUI, false);

        if (currentState == GameState.Paused)
            SetActiveSafe(settingsUI, true);
    }
    */
    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go == null) return;
        if (go.activeSelf == active) return;
        go.SetActive(active);
    }
}
