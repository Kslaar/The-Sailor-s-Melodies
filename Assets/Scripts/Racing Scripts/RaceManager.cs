using System.Collections;
using AK.Wwise;
using NUnit.Framework;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    public bool IsRacing => currentCourse != null && state == RaceState.Racing;
    public float CurrentTime => timePassed;

    public event System.Action<float> OnTimeChanged;
    public event System.Action<int> OnCountdownChanged;
    public event System.Action<string> OnWarning;
    public event System.Action<string, float> OnRaceFinished;
    public event System.Action<string> OnRaceFailed;

    private enum RaceState { Idle, Countdown, Racing, Finished, Failed }
    private RaceState state = RaceState.Idle;

    private RaceCourse currentCourse;
    private int nextCheckpoint = 0;
    private float timePassed;
    private float outOfBoundsTimer;

    // Wichtig für den Playerfreeze
    private BoatControl boat;
    private Rigidbody rb;
    private bool boatEnabled;
    private bool rbKinematic;


    [Header("Checkpoint FX")]
    [SerializeField] private GameObject checkpointArrowFxPrefab;
    [SerializeField] private Vector3 fxOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool faceBoat = true;

    private GameObject checkpointArrowFxInstance;
    private ParticleSystem[] fxParticles;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
    }

    public bool StartRace(RaceCourse course)
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return false;
        if (!gsm.TryEnterRace("StartRace")) return false;

        if (course == null || course.startPoint == null)
        {
            Debug.LogWarning("[RaceManager] StartRace failed: course/startPoint missing");
            return false;
        }

        if (state != RaceState.Idle)
        {
            Debug.LogWarning("[RaceManager] StartRace blocked: already running");
            return false;
        }

        currentCourse = course;
        currentCourse.SetRaceTriggersActive(true);
        nextCheckpoint = 0;
        timePassed = 0f;
        outOfBoundsTimer = 0f;

        boat = FindFirstObjectByType<BoatControl>();
        if (boat == null)
        {
            Debug.LogWarning("[RaceManager] BoatControl not found...");
            ResetRace();
            return false;
        }
        rb = boat.GetComponent<Rigidbody>();
        
        var docking = FindFirstObjectByType<BoatDockingController>();
        if (docking != null && docking.IsDocked)
            docking.UndockForRace();
        
        // Teleport
        TeleportToStart();
        EnsureCheckpointFx();
        UpdateCheckpointFx();
        StartCoroutine(CountdownThenStartRace());

        return true;
    }

    private void TeleportToStart()
    {
        FreezePlayer(true);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = currentCourse.startPoint.position;
            rb.rotation = currentCourse.startPoint.rotation;
        }
        else
        {
            boat.transform.SetPositionAndRotation(currentCourse.startPoint.position, currentCourse.startPoint.rotation);
        }
    }

    private IEnumerator CountdownThenStartRace()
    {
        state = RaceState.Countdown;
        
        //Wwise: World Music stoppen
        GlobalMusicManager.Instance.StopWorldMusic();


        //Wwise MusicManager
        GlobalMusicManager.Instance.SetRaceState("Countdown");

        for (int t = 3; t >= 1; t--)
        {
            OnCountdownChanged?.Invoke(t);
            yield return new WaitForSeconds(1f);
        }

        OnCountdownChanged?.Invoke(0);
        FreezePlayer(false);

        state = RaceState.Racing;

        //Wwise MusicManager
        GlobalMusicManager.Instance.SetRaceState("Racing");
    }

    private void Update()
    {
        if (state != RaceState.Racing || currentCourse == null) return;

        timePassed += Time.deltaTime;
        OnTimeChanged?.Invoke(timePassed);

        // Maximale Zeit, damit das Spiel auch Idiotensicher ist 
        if (timePassed > currentCourse.maxTimeSeconds)
        {
            Fail("MaxTime");
            return;
        }

        // Maximale Distanz zur Rennstrecke, damit das SPiel auc Idiotensicher ist
        float dist = DistanceToCourseAnchors(boat.transform.position, currentCourse);
        if (dist > currentCourse.maxDistanceFromIsland)
        {
            outOfBoundsTimer += Time.deltaTime;
            OnWarning?.Invoke("You are too far away! Return to the Course now!");
            if (outOfBoundsTimer >= currentCourse.outOfBoundsGracePeriod)
            {
                Fail("OutOfBounds");
                return;
            }
        }
        else
        {
            outOfBoundsTimer = 0f;
        }
    }

    private float DistanceToCourseAnchors(Vector3 pos, RaceCourse course)
    {
        float minDistance = float.MaxValue;
        foreach (var a in course.GetAnchorPositions())
        {
            float d = Vector3.Distance(pos, a);

            if (d < minDistance)
                minDistance = d;
        }
        return minDistance;
    }

    public void OnTriggerHit(string courseID, RaceTrigger.TriggerType trigger, int checkpointIndex)
    {
        Debug.Log($"[RaceManager] OnTriggerHit: courseID={courseID} trigger={trigger} idx={checkpointIndex} state={state} next={nextCheckpoint} current={(currentCourse? currentCourse.courseID:"NULL")}");
        
        if (currentCourse == null) return;
        if (state != RaceState.Racing) return;
        if (courseID != currentCourse.courseID) return;

        if (trigger == RaceTrigger.TriggerType.Checkpoint)
        {
            if (checkpointIndex != nextCheckpoint) return;
            nextCheckpoint++;
            UpdateCheckpointFx();

            //Wwise SFX
            AkUnitySoundEngine.PostEvent("Play_Checkpoint", gameObject);
            return;

           
        }

        if (trigger == RaceTrigger.TriggerType.Finish)
        {
            // Muss ja auch schon logisch sein: Finish zählt nur, wenn ALLE Checkpoints getriggered wurden
            if (currentCourse.checkpoints != null && currentCourse.checkpoints.Count > 0)
            {
                if (nextCheckpoint < currentCourse.checkpoints.Count) return;
            }

            //Wwise SFX
            AkUnitySoundEngine.PostEvent("Play_Finish", gameObject);

            Finish();
        }
    }

    private void Finish()
    {
        state = RaceState.Finished;

        //wwise World Music wieder Starten
        GlobalMusicManager.Instance.StartWorldMusic();
        GlobalMusicManager.Instance.SetRaceState("Finished");
        FreezePlayer(true);

        bool success = timePassed <= currentCourse.successTimeSeconds;

        if (success)
            OnRaceFinished?.Invoke(currentCourse.courseID, timePassed);
        else
            OnRaceFailed?.Invoke("TooSlow");
        // ResetRaceAfterDelay();
        // GameStateManager.Instance?.TryExitRace("Race finished");
        StartCoroutine(CoroutineReturnToQuestgiver(success));
    }

    private void Fail(string reason)
    {
        Debug.LogWarning($"[RaceManager] Race failed: {reason}");
        state = RaceState.Failed;
        //wwise World Music wieder starten
        GlobalMusicManager.Instance.StartWorldMusic();
        GlobalMusicManager.Instance.SetRaceState("Idle");
        FreezePlayer(true);
        OnRaceFailed?.Invoke(reason);
        // ResetRaceAfterDelay();
        // GameStateManager.Instance?.TryExitRace("Race failed");
        StartCoroutine(CoroutineReturnToQuestgiver(success: false));
    }

    private IEnumerator CoroutineReturnToQuestgiver(bool success)
    {
        yield return new WaitForSeconds(0.1f);

        // Teleport zum ReturnPoint (optional)
        if (currentCourse != null && currentCourse.returnPoint != null && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = currentCourse.returnPoint.position;
            rb.rotation = currentCourse.returnPoint.rotation;
        }

        // Quest-State updaten
        if (success && currentCourse != null && !string.IsNullOrWhiteSpace(currentCourse.questID))
            QuestManager.Instance?.ForceSetState(currentCourse.questID, QuestState.ReadyToTurnIn);

        DialogueAsset d = null;
        if (currentCourse != null && currentCourse.questGiverDialogue != null)
            d = currentCourse.questGiverDialogue.GetDialogue();

        var docking = FindFirstObjectByType<BoatDockingController>();

        if (docking != null && currentCourse != null && currentCourse.returnDock != null && d != null)
        {
            FreezePlayer(false);

            docking.AutoDockForDialogue(
                dockZone: currentCourse.returnDock,
                dialogue: d,
                reason: success ? "Race success" : "Race failed",
                showDockUI: false
            );
        }
        else
        {
            Debug.LogWarning("[RaceManager] AutoDockForDialogue failed (missing docking/returnDock/dialogue). Fallback: Docked only.");
            GameStateManager.Instance?.ForceUnpause(GameState.Docked, "RaceReturn fallback");
        }

        ResetRace();
    }


    private void ResetRace()
    {
        if (currentCourse != null) 
            currentCourse.SetRaceTriggersActive(false);
            
        currentCourse = null;
        nextCheckpoint = 0;
        timePassed = 0f;
        outOfBoundsTimer = 0f;
        state = RaceState.Idle;

        if (checkpointArrowFxInstance != null)
            checkpointArrowFxInstance.SetActive(false);
    }

    private void FreezePlayer(bool freeze)
    {
        if (boat == null) return;

        if (freeze)
        {
            boatEnabled = boat.enabled;
            boat.enabled = false;

            if (rb != null)
            {
                rbKinematic = rb.isKinematic;
                rb.isKinematic = true;
            }
        }
        else
        {
            boat.enabled = boatEnabled;

            if (rb != null) rb.isKinematic = rbKinematic;
        }
    }

    /////////////////////////////////////////////////////////////
    
    private void EnsureCheckpointFx()
    {
        if (checkpointArrowFxPrefab == null) return;

        if (checkpointArrowFxInstance == null)
        {
            checkpointArrowFxInstance = Instantiate(checkpointArrowFxPrefab);
            checkpointArrowFxInstance.name = "[Race] CheckpointArrowFX";

            // Particles cachen (falls ParticleSystem)
            fxParticles = checkpointArrowFxInstance.GetComponentsInChildren<ParticleSystem>(true);
        }
    }

    private void UpdateCheckpointFx()
    {
        if (checkpointArrowFxInstance == null) return;
        if (currentCourse == null) { checkpointArrowFxInstance.SetActive(false); return; }

        // Wenn alle CPs durch sind, optional auf Finish zeigen oder ausblenden
        bool hasCheckpoints = currentCourse.checkpoints != null && currentCourse.checkpoints.Count > 0;
        bool allCheckpointsDone = hasCheckpoints && nextCheckpoint >= currentCourse.checkpoints.Count;

        Vector3 targetPos;
        if (!allCheckpointsDone && hasCheckpoints)
        {
            var cp = currentCourse.checkpoints[nextCheckpoint];
            if (cp == null) { checkpointArrowFxInstance.SetActive(false); return; }
            targetPos = cp.bounds.center;
        }
        else
        {
            // Nach letztem Checkpoint: auf Finish (oder returnPoint) zeigen – oder ausmachen
            if (currentCourse.finishTrigger == null)
            {
                checkpointArrowFxInstance.SetActive(false);
                return;
            }
            targetPos = currentCourse.finishTrigger.bounds.center;
        }

        checkpointArrowFxInstance.transform.position = targetPos + fxOffset;

        if (faceBoat && boat != null)
        {
            Vector3 look = boat.transform.position - checkpointArrowFxInstance.transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
                checkpointArrowFxInstance.transform.rotation = Quaternion.LookRotation(-look.normalized, Vector3.up);
            // (-look) => Pfeil “zeigt weg vom Spieler” Richtung Ziel; wenn dein Pfeil anders herum ist, nimm look.normalized
        }

        checkpointArrowFxInstance.SetActive(true);

        if (fxParticles != null)
        {
            foreach (var ps in fxParticles)
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }
    }
}
