using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Wwise Music Events")]
    public AK.Wwise.Event startSceneMusic;   // Musik der ersten Szene
    public AK.Wwise.Event gameplayMusic;     // Musik der nächsten Szene

    private void Awake()
    {
        // Singleton – nur eine Instanz darf existieren
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
        // Musik für die erste Szene starten
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        // Alte Musik stoppen
        AkSoundEngine.StopAll();

        // Szenenabhängig neue Musik starten
        switch (sceneName)
        {
            case "MainMenuScene":    // Name deiner ersten Szene
                startSceneMusic.Post(gameObject);
                break;

            case "Level1":        // Name deiner zweiten Szene
                gameplayMusic.Post(gameObject);
                break;

                // Weitere Szenen hier einfach ergänzen
        }
    }
}
