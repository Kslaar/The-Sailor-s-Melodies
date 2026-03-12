using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalMusicManager : MonoBehaviour
{
    public static GlobalMusicManager Instance;

    [Header("Wwise Events")]
    public AK.Wwise.Event playWorldMusic;
    public AK.Wwise.Event stopMusic;

    [Header("Switches")]
    public AK.Wwise.Switch menuSwitch;
    public AK.Wwise.Switch explorationSwitch;
    public AK.Wwise.Switch islandSwitch;

    private bool musicStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        ApplyInitialSceneState(SceneManager.GetActiveScene().name);
        StartCoroutine(StartMusicDelayed());
    }

    private IEnumerator StartMusicDelayed()
    {
        // Wichtig: Im Build Wwise/Scene erst ganz kurz "settlen" lassen
        yield return null;
        yield return null;

        uint id = playWorldMusic.Post(gameObject);
        musicStarted = id != 0;

        Debug.Log("[Music] Initial Post ID: " + id);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyInitialSceneState(scene.name);
    }

    private void ApplyInitialSceneState(string sceneName)
    {
        if (sceneName == "MainMenuScene")
        {
            menuSwitch.SetValue(gameObject);
            AkUnitySoundEngine.SetState("GameState", "Docked");
        }
        else if (sceneName == "GameScene")
        {
            explorationSwitch.SetValue(gameObject);
            AkUnitySoundEngine.SetState("GameState", "Sailing");
        }

        AkUnitySoundEngine.SetSwitch("RaceState", "Idle", gameObject);
    }

    public void SetExploration()
    {
        explorationSwitch.SetValue(gameObject);
    }

    public void SetIsland()
    {
        islandSwitch.SetValue(gameObject);
    }

    public void SetRaceState(string stateName)
    {
        AkUnitySoundEngine.SetSwitch("RaceState", stateName, gameObject);
    }

    public void StopWorldMusic()
    {
        stopMusic.Post(gameObject);
        musicStarted = false;
    }

    public void StartWorldMusic()
    {
        uint id = playWorldMusic.Post(gameObject);
        musicStarted = id != 0;

        Debug.Log("[Music] StartWorldMusic Post ID: " + id);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}