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
    [SerializeField] private GameObject raceUI;

    private GameStateManager gsm;

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
        SetActiveSafe(dialogueUIRoot, false);
        if (questlogUI != null) questlogUI.Close();
        SetActiveSafe(settingsUI, false);
        SetActiveSafe(raceUI, false);

        HideAllDockUIs();

        switch (state)
        {
            case GameState.Sailing:
                SetActiveSafe(sailingHUD, true);
                break;

            case GameState.Docked:
                var dockCtrl = FindFirstObjectByType<BoatDockingController>();
                if (dockCtrl != null && dockCtrl.IsDocked && dockCtrl.CurrentDock != null)
                    dockCtrl.CurrentDock.dockUI?.Show(dockCtrl.CurrentDock);
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

            case GameState.Racing:
                SetActiveSafe(raceUI, true);
                break;
        }
    }

    private void HideAllDockUIs()
    {
        foreach (var ui in FindObjectsByType<DockInteractionUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            ui?.Hide();
    }

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go == null) return;
        if (go.activeSelf == active) return;
        go.SetActive(active);
    }
}
