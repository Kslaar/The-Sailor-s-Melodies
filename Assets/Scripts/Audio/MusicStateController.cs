using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class GameSceneMusicManager : MonoBehaviour
{
    public static GameSceneMusicManager Instance;

    [Header("Wwise Events")]
    public AK.Wwise.Event playWorldMusic;   // Play_Music_World Event
    public AK.Wwise.Switch explorationSwitch;
    public AK.Wwise.Switch islandSwitch;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Musik nur starten, wenn sie noch nicht läuft
        uint id = playWorldMusic.Post(gameObject);

        if (id == 0)
        {
            Debug.Log("Music already playing – skipping start.");
        }

        SetExplorationMusic();
    }


    public void SetExplorationMusic()
    {
        explorationSwitch.SetValue(gameObject);
    }

    public void SetIslandMusic()
    {
        islandSwitch.SetValue(gameObject);
    }
}

