using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_Quit : MonoBehaviour
{
    // public static MainMenu_Quit Instance { get; private set;}
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    [SerializeField] private float minLoadingSeconds = 0.5f;

    /*
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    */
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
