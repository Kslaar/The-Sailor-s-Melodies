using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Wwise Music Events")]
    public AK.Wwise.Event startSceneMusic;   // Musik im Main Menu
    public AK.Wwise.Event stopMusic;         // Stop-Event mit Fade-Out

    [Header("Fade Settings")]
    public float fadeOutTime = 1.5f;
    public float fadeInDelay = 0.2f;

    private Coroutine musicRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Nur im MainMenu Musik starten
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
            startSceneMusic.Post(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Wenn wir das MainMenu verlassen → Fade-Out
        if (scene.name != "MainMenuScene")
        {
            if (musicRoutine != null)
                StopCoroutine(musicRoutine);

            musicRoutine = StartCoroutine(FadeOutRoutine());
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        // Fade-Out Event abfeuern
        stopMusic.Post(gameObject);

        // Warten bis Fade-Out fertig ist
        yield return new WaitForSeconds(fadeOutTime + fadeInDelay);

        // Danach zerstören wir den Manager, damit die nächste Szene ihren eigenen Manager nutzen kann
        Destroy(gameObject);
    }
}

