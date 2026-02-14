using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;

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
        // Musik nur EINMAL starten
        uint id = playWorldMusic.Post(gameObject);

        if (id == 0)
            Debug.Log("Music already playing – skipping start.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenuScene")
        {
            menuSwitch.SetValue(gameObject);
        }
        else if (scene.name == "GameScene")
        {
            explorationSwitch.SetValue(gameObject);
        }
    }

    // Manuelle Umschaltung in der GameScene
    public void SetExploration()
    {
        explorationSwitch.SetValue(gameObject);
    }

    public void SetIsland()
    {
        islandSwitch.SetValue(gameObject);
    }
}

