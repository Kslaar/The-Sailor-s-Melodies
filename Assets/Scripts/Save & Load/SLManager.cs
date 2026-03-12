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

    [Header("New Game")]
    [SerializeField] private DockZone afterMainMenuDockZone;
    [SerializeField] private DialogueAsset introPastorDialogue;

    private bool _pendingNewGameIntro;

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
        float userMasterVolume = SettingsManager.Instance != null ? SettingsManager.Instance.Data.masterVolume : 1f;
        float userMasterRtpc = userMasterVolume * 100f;

        if (Loadingscreen.Instance != null)
            yield return Loadingscreen.Instance.FadeToBlack();

        Time.timeScale = 1f;

        // Sound Fadeout und dann stumm
        yield return FadeWwiseRtpc(masterVolumeRtpc, userMasterRtpc, 0f, audioFadeSeconds);
        if (useSuspendAfterFadeOut) AkUnitySoundEngine.Suspend();

        // Wir laden die Szene
        var load = SceneManager.LoadSceneAsync(gameScene);
        load.allowSceneActivation = true;

        while (!load.isDone)
            yield return null;

        yield return null; // Warten bis alles initialisiert ist

        if (GameStateManager.Instance != null)
        {
            if (loadSave) GameStateManager.Instance.ForceUnpause(GameState.Sailing, "Enter GameScene after Load Game");
            else GameStateManager.Instance.ForceUnpause(GameState.Docked, "Enter GameScene on New Game");
        }

        EnsurePlayer(forceRefresh: true);
        if (player != null)
        {
            if (loadSave) ApplySavedData();
            else _pendingNewGameIntro = true;
        }

        yield return null;

        float passedTime = Time.realtimeSinceStartup - startRealTime;
        float remaining = minLoadingSecondsTotal - passedTime;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        // Audio wieder an
        if (useSuspendAfterFadeOut) AkUnitySoundEngine.WakeupFromSuspend();

        yield return FadeWwiseRtpc(masterVolumeRtpc, 0f, userMasterRtpc, audioFadeSeconds);

        if (!loadSave && _pendingNewGameIntro)
        {
            _pendingNewGameIntro = false;
            StartCoroutine(StartNewGameIntro());
        }
        
        if (Loadingscreen.Instance != null)
            yield return Loadingscreen.Instance.FadeFromBlack();
    }

    private void EnsurePlayer(bool forceRefresh = false)
    {
        if (!forceRefresh && player != null)
            return;

        player = FindFirstObjectByType<BoatControl>();
        if (player == null)
            Debug.LogError("BoatControl (Player) not found. Existing in GameScene?");
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

        // 3. Upgrade importieren
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.Import(data.boatUpgrades);

        // 4. Spieler setzen
        Vector3 position = new Vector3(
            data.playerPosition[0],
            data.playerPosition[1],
            data.playerPosition[2]
        );

        // 5. Rotation
        Quaternion rotation = Quaternion.identity;
        if (data.playerRotation != null && data.playerRotation.Length == 4)
        {
            rotation = new Quaternion(
                data.playerRotation[0],
                data.playerRotation[1],
                data.playerRotation[2],
                data.playerRotation[3]
            );
        }

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
            player.transform.SetPositionAndRotation(position, rotation);
        }

        if (RecordHotbar.Instance != null)
        {
            RecordHotbar.Instance.ImportIDs(data.recordedSoundIDs, data.selectedHotbarIndex);
        }

        Debug.Log("Applied saved position: " + position + "rotation: " + rotation.eulerAngles);
    }

    public void BackToMainMenu(string mainMenuScene, float minSeconds = 0.5f)
    {
        StartCoroutine(BackRoutine(mainMenuScene, minSeconds));
    }

    private IEnumerator BackRoutine(string mainMenuScene, float minSeconds)
    {
        float startRealtime = Time.realtimeSinceStartup;
        float userMasterVolume = SettingsManager.Instance != null ? SettingsManager.Instance.Data.masterVolume : 1f;
        float userMasterRtpc = userMasterVolume * 100f;

        if (Loadingscreen.Instance != null)
            yield return Loadingscreen.Instance.FadeToBlack();

        Time.timeScale = 1f;

        yield return FadeWwiseRtpc(masterVolumeRtpc, userMasterRtpc, 0f, 0.35f);

        AkUnitySoundEngine.Suspend();

        var op = SceneManager.LoadSceneAsync(mainMenuScene);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        yield return null;

        float elapsed = Time.realtimeSinceStartup - startRealtime;
        float remaining = minSeconds - elapsed;
        if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);

        AkUnitySoundEngine.WakeupFromSuspend();

        AkUnitySoundEngine.SetRTPCValue("PauseDuck", 0f);
        if (SettingsManager.Instance != null)
                SettingsManager.Instance.ApplyAll();

        yield return FadeWwiseRtpc(masterVolumeRtpc, 0f, userMasterRtpc, 0.35f);

        if (Loadingscreen.Instance != null)
            yield return Loadingscreen.Instance.FadeFromBlack();
    }

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

    private IEnumerator StartNewGameIntro()
    {
        // Intro-DockZone
        DockZone pastorDock = null;
        foreach (var dock in FindObjectsByType<DockZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (dock != null && dock.CompareTag("IntroDock"))
            {
                pastorDock = dock;
                break;
            }
        }
        if (pastorDock == null)
        {
            Debug.LogWarning("[SLManager] IntroDock not found.");
            yield break;
        }

        EnsurePlayer(forceRefresh: true);
        if (player == null) yield break;

        // DockingController vom Spieler
        var docking = FindFirstObjectByType<BoatDockingController>();
        if (docking == null)
        {
            Debug.LogWarning("[SLManager] BoatDockingController not found.");
            yield break;
        }

        if (introPastorDialogue == null)
        {
            Debug.LogWarning("[SLManager] introPastorDialogue not assigned.");
            yield break;
        }

        docking.AutoDockForDialogue(
            dockZone: pastorDock,
            dialogue: introPastorDialogue,
            reason: "New Game Intro",
            showDockUI: false   // false, weil Dialog sofort starten soll
        );

        yield return null;
        yield return null;

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.AllowBack = false;

        if (GameStateManager.Instance != null && GameStateManager.Instance.State == GameState.Docked)
            DialogueManager.Instance?.StartDialogue(introPastorDialogue);
    }
}