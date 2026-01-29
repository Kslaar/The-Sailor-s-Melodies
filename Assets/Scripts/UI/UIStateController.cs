using UnityEngine;

public class UIStateController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject sailingHUD;
    [SerializeField] private GameObject dockUI;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject questlogUI;
    [SerializeField] private GameObject settingsUI;

    private GameStateManager gsm;

    private void Start()
    {
        gsm = GameStateManager.Instance;

        if (gsm == null)
        {
            Debug.LogError("[UIStateController] GameStateManager.Instance ist NULL in Start!");
            return;
        }

        gsm.OnStateChanged += Apply;

        Debug.Log("[UIStateController] subscr. Current State: " + gsm.State);
        Apply(gsm.State);
    }
    private void OnDestroy()
    {
        if (gsm != null)
            gsm.OnStateChanged -= Apply;
    }

    private void Apply(GameState state)
    {
        Debug.Log("[UIStateController] Apply " + state);

        SetActiveSafe(sailingHUD, false);
        SetActiveSafe(dockUI, false);
        SetActiveSafe(dialogueUI, false);
        SetActiveSafe(questlogUI, false);
        SetActiveSafe(settingsUI, false);

        switch (state)
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
                SetActiveSafe(questlogUI, true);
                break;

            case GameState.Paused:
                SetActiveSafe(settingsUI, true);
                break;
        }
    }

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go == null) return;
        if (go.activeSelf == active) return;
        go.SetActive(active);
    }
}
