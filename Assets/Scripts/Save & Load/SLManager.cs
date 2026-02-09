using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class SLManager : MonoBehaviour
{
    public static SLManager Instance;

    [Header("Scene")]
    [SerializeField] string gameScene = "GameScene";

    [Header("Runtime")]
    [SerializeField] BoatControl player;

    [Header("Loading/Audio")]
    [SerializeField] float minLoadingSecondsTotal = 5f;   
    [SerializeField] float audioFadeSeconds = 0.5f;
    [SerializeField] string masterVolumeRtpc = "MasterVolume";
    [SerializeField] bool useSuspendAfterFadeOut = true;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);  
    }

    public void NewGame()
    {
        StartCoroutine(LoadSceneOrNot(loadSave: false));
    }
    public void LoadGameFromMenu()
    {
        StartCoroutine(LoadSceneOrNot(loadSave: true));
    }

    public void SavePlayer()
    {
        EnsurePlayer();
        if (player == null)
        {
            Debug.LogError("Player reference is missing in SLManager!");
            return;
        }
        SaveSystem.SaveGame(player);
    }

    public void LoadPlayer()
    {
        EnsurePlayer();
        if (player == null)
        {
            Debug.LogError("Player reference is missing in SLManager!");
            return;
        }
        ApplySavedData();
    }

    private IEnumerator LoadSceneOrNot(bool loadSave)
    {
        float startRealTime = Time.realtimeSinceStartup;
        
        if (Loadingscreen.Instance != null)
            yield return Loadingscreen.Instance.FadeToBlack();

        Time.timeScale = 1f;

        // Sound Fadeout und dann stumm
        yield return FadeWwiseRtpc(masterVolumeRtpc, 1f, 0f, audioFadeSeconds);
        if (useSuspendAfterFadeOut) AkUnitySoundEngine.Suspend();

        // Wir laden die Szene
        var load = SceneManager.LoadSceneAsync(gameScene);
        load.allowSceneActivation = true;

        while (!load.isDone)
            yield return null;

        yield return null; // Nochmal warten bis wirklich alles initialisiert ist

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.ForceUnpause(GameState.Sailing, "Enter GameScene");

        EnsurePlayer(forceRefresh: true); // Wir gehen sicher, dass der Spieler existiert
        if (player != null) 
        {
            if (loadSave) ApplySavedData();
            else player.transform.position = new Vector3(12, 0, 6);
        }
        yield return null; // Nochmal warten zur Sicherheit wegen Apply() (UIStateController)

        float passedTime = Time.realtimeSinceStartup - startRealTime;
        float remaining = minLoadingSecondsTotal - passedTime;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        // Audio wieder an
        if (useSuspendAfterFadeOut) AkUnitySoundEngine.WakeupFromSuspend();

        yield return FadeWwiseRtpc(masterVolumeRtpc, 0f, 1f, audioFadeSeconds);

        if (Loadingscreen.Instance != null)
            yield return Loadingscreen.Instance.FadeFromBlack();
    }

    private void EnsurePlayer(bool forceRefresh = false)
    {
        if (!forceRefresh && player != null) 
            return;
        player = FindFirstObjectByType<BoatControl>();
        if (player == null)
            Debug.LogError("BoatControl (Player) not found. Exisitng in GameScene?");
    }
    private void ApplySavedData()
    {
        SaveGameData data = SaveSystem.LoadGame();
        if (data == null) return;

        // 1. Items importieren
        if (ItemCollectionRegistry.Instance != null)
            ItemCollectionRegistry.Instance.Import(data.itemCounts);

        // 2. Queststates importieren
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ImportStates(data.questStates);
            QuestManager.Instance.ReRegisterActiveObjectives();
        }

        // 3. Wir setzen den Spieler
        Vector3 position = new Vector3(
        data.playerPosition[0],
        data.playerPosition[1],
        data.playerPosition[2]
        );

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rb.rotation;
        }
        else
        {
            player.transform.position = position;
        }

        Debug.Log("Applied saved position: " + position);
    }

    public void BackToMainMenu(string mainMenuScene, float minSeconds = 0.5f)
    {
        StartCoroutine(BackRoutine(mainMenuScene, minSeconds));
    }

    private IEnumerator BackRoutine(string mainMenuScene, float minSeconds)
{
    float startRealtime = Time.realtimeSinceStartup;

    if (Loadingscreen.Instance != null)
        yield return Loadingscreen.Instance.FadeToBlack();

    Time.timeScale = 1f;

    yield return FadeWwiseRtpc("MasterVolume", 1f, 0f, 0.35f);

    AkUnitySoundEngine.Suspend();

    var op = SceneManager.LoadSceneAsync(mainMenuScene);
    op.allowSceneActivation = true;
    while (!op.isDone) yield return null;

    yield return null;

    float elapsed = Time.realtimeSinceStartup - startRealtime;
    float remaining = minSeconds - elapsed;
    if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);

    AkUnitySoundEngine.WakeupFromSuspend();

    yield return FadeWwiseRtpc("MasterVolume", 0f, 1f, 0.35f);

    if (Loadingscreen.Instance != null)
        yield return Loadingscreen.Instance.FadeFromBlack();
}


    // Audio Fade Helper
    private IEnumerator FadeWwiseRtpc(string rtpcName, float from, float to, float duration)
    {
        if (string.IsNullOrEmpty(rtpcName))
            yield break;

        if (duration <= 0f)
        {
            AkUnitySoundEngine.SetRTPCValue(rtpcName, to);
            yield break;
        }

        float t = 0f;
        AkUnitySoundEngine.SetRTPCValue(rtpcName, from);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(from, to, t / duration);
            AkUnitySoundEngine.SetRTPCValue(rtpcName, v);
            yield return null;
        }

        AkUnitySoundEngine.SetRTPCValue(rtpcName, to);
    }
}
