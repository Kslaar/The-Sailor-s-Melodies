using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_Quit : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    [SerializeField] private float minLoadingSeconds = 0.5f;

    public void BackToMainMenu()
    {
        // StartCoroutine(BackRoutine());
        SLManager.Instance.BackToMainMenu(mainMenuScene, minLoadingSeconds);
    }

    // Schließt das Spiel (aber funktioniert nur im Build)
    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("QuitGame() aufgerufen (funktioniert nur im Build).");
    }
}
