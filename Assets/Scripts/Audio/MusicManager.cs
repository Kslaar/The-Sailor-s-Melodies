using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Wwise Music Events")]
    public AK.Wwise.Event startSceneMusic;   // Musik der ersten Szene
    public AK.Wwise.Event gameplayMusic;     // Musik der n�chsten Szene

    private void Awake()
    {
        // Singleton � nur eine Instanz darf existieren
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Szene-Listener registrieren
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Musik f�r die erste Szene starten
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        // Alte Musik stoppen
        AkUnitySoundEngine.StopAll();

        // Szenenabh�ngig neue Musik starten
        switch (sceneName)
        {
            case "MainMenuScene":    // Name deiner ersten Szene
                startSceneMusic.Post(gameObject);
                break;

            case "Level1":        // Name deiner zweiten Szene
                gameplayMusic.Post(gameObject);
                break;

                // Weitere Szenen hier einfach erg�nzen
        }
    }
}
