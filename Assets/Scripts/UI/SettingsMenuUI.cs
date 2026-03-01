using UnityEngine;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenuScene";
    [SerializeField] private float minLoadingSeconds = 0.5f;

    public void Resume()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        gsm.Unpause("Resume button");
    }

    public void BackToMainMenu()
    {
        RaceManager.Instance?.ResetRaceState();
        SLManager.Instance.BackToMainMenu(mainMenuScene, minLoadingSeconds);
    }
}