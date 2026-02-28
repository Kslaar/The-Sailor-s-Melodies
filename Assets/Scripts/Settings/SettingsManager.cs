using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-2000)]
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private string masterRtpc = "MasterVolume";
    [SerializeField] private string musicRtpc = "MusicVolume";
    [SerializeField] private string sfxRtpc = "SFXVolume";

    [Header("Range")]
    [SerializeField] private float wwiseRtpcMax = 100;

    public GameSettingsData Data { get; private set; } = new GameSettingsData();

    private string PathSettings => System.IO.Path.Combine(Application.persistentDataPath, "settings.json");

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Laod();
    }

    private void Start()
    {
        ApplyAll();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    /////////////////////////////////////////////////////////////////

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nach Szenenwechsel nochmal checken, dass Settings wirklich übernommen werden
        ReapplyAfterSceneChange();
    }

    private void ReapplyAfterSceneChange()
    {
        // Pause-Duck killen
        AkUnitySoundEngine.SetRTPCValue("PauseDuck", 0f);

        // Volumes nochmal setzen
        ApplyAll();
    }

    //////////////////////////////////////////////////////////////////

    public void SetMaster(float volume)
    {
        Data.masterVolume = Mathf.Clamp01(volume);
        ApplyMaster();
        Save();
    }

    public void SetMusic(float volume)
    {
        Data.musicVolume = Mathf.Clamp01(volume);
        ApplyMusic();
        Save();
    }

    public void SetSfx(float volume)
    {
        Data.sfxVolume = Mathf.Clamp01(volume);
        ApplySfx();
        Save();
    }

    public void SetInvertWASD(bool invert)
    {
        Data.invertControls = invert;
        Save();
    }

    public void SetWindGustButtonUse(bool useButton)
    {
        Data.windGustButtonUse = useButton;
        Save();
    }

    public void ApplyAll()
    {
        ApplyMaster();
        ApplyMusic();
        ApplySfx();
    }

    //////////////////////////////////////////////////////////////////
    
    private void ApplyMaster()
    {
        if (!string.IsNullOrWhiteSpace(masterRtpc))
            AkUnitySoundEngine.SetRTPCValue(masterRtpc, Data.masterVolume * wwiseRtpcMax);
    }

    private void ApplyMusic()
    {
        if (!string.IsNullOrWhiteSpace(musicRtpc))
            AkUnitySoundEngine.SetRTPCValue(musicRtpc, Data.musicVolume * wwiseRtpcMax);
    }

    private void ApplySfx()
    {
        if (!string.IsNullOrWhiteSpace(sfxRtpc))
            AkUnitySoundEngine.SetRTPCValue(sfxRtpc, Data.sfxVolume * wwiseRtpcMax);
    }

    //////////////////////////////////////////////////////////////////
    
    public void Laod()
    {
        try
        {
            if (!File.Exists(PathSettings))
            {
                Data = new GameSettingsData();
                ApplyAll();
                return;
            }

            string json = File.ReadAllText(PathSettings);

            if (string.IsNullOrWhiteSpace(json)) // Falls Datei kaputt ist
            {
                Data = new GameSettingsData();
                ApplyAll();
                return;
            }

            var loaded = JsonUtility.FromJson<GameSettingsData>(json);
            Data = loaded ?? new GameSettingsData();

            Data.masterVolume = Mathf.Clamp01(Data.masterVolume);
            Data.musicVolume = Mathf.Clamp01(Data.musicVolume);
            Data.sfxVolume = Mathf.Clamp01(Data.sfxVolume);

            ApplyAll();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Settings] Load failed, using default settings. {e.Message}");
            Data = new GameSettingsData();
            ApplyAll();
        }
    }

    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, prettyPrint: true);
            File.WriteAllText(PathSettings, json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Settings] Save failed. {e.Message}");
        }
    }
}
