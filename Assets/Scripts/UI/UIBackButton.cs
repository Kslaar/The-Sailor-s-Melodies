using UnityEngine;

public class UIBackButton : MonoBehaviour
{
    public void Back()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        switch (gsm.State)
        {
            case GameState.Paused:
                gsm.Unpause("UI Back");
                break;

            case GameState.QuestLog:
                gsm.TrySetState(GameState.Sailing, "UI Back (QuestLog)");
                break;

            case GameState.Dialogue:
                if (DialogueManager.Instance != null && DialogueManager.Instance.AllowBack == false)
                    return;
                    
                if (DialogueManager.Instance != null) DialogueManager.Instance.EndDialogue();
                else gsm.TryExitDialogue("UI Back (Dialogue)");
                break;

            case GameState.Docked: // Hier müssen wir "UndockNow" wählen da sonst Kamera auf Insel gelocked bleibt und Boot immer noch immobil wäre
                var dock = FindFirstObjectByType<BoatDockingController>();
                if (dock != null && dock.IsDocked) dock.UndockNow();
                else gsm.TrySetState(GameState.Sailing, "UI Back (Docked fallback)");
                break;

            default:
                break;
        }
    }
}
