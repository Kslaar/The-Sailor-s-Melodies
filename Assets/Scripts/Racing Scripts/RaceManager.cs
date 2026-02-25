using System.Collections;
using AK.Wwise;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    public bool IsRacing => currentCourse != null && (state == RaceState.Countdown || state == RaceState.Racing);
    public float CurrentTime => timePassed;
    public RaceCourse CurrentCourse => currentCourse;

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

    // Player freeze
    private BoatControl boat;
    private Rigidbody rb;
    private bool boatEnabled;
    private bool rbKinematic;

    [Header("Checkpoint FX")]
    [SerializeField] private GameObject checkpointArrowFxPrefab;

    [Tooltip("Offset des FX relativ zur berechneten Zielposition (z.B. +Y damit es nicht im Wasser steckt).")]
    [SerializeField] private Vector3 fxOffset = new Vector3(0f, 1.2f, 0f);

    [Tooltip("Skalierung des FX im Worldspace. (Setze ParticleSystem ScalingMode auf Hierarchy!)")]
    [SerializeField] private float fxScale = 3.0f;

    [Tooltip("Wenn true: benutze eine feste Y-Höhe statt Collider-MinY (gut wenn Wasser immer gleiche Höhe hat).")]
    [SerializeField] private bool useFixedY = false;

    [Tooltip("Feste Y-Höhe, wenn useFixedY = true.")]
    [SerializeField] private float fixedY = 0f;

    [SerializeField] private bool faceBoat = true;

    private GameObject checkpointArrowFxInstance;
    private ParticleSystem[] fxParticles;

    private void OnEnable()
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
        // currentCourse.SetRaceTriggersActive(true);
        StartCoroutine(CoruEnableRaceTriggersNextFrame());
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

        // Wwise: World Music stoppen
        GlobalMusicManager.Instance.StopWorldMusic();
        GlobalMusicManager.Instance.SetRaceState("Countdown");

        for (int t = 3; t >= 1; t--)
        {
            OnCountdownChanged?.Invoke(t);
            yield return new WaitForSeconds(1f);
        }

        OnCountdownChanged?.Invoke(0);
        FreezePlayer(false);

        state = RaceState.Racing;
        //WWise Race Start Signal abspielen
        AkUnitySoundEngine.PostEvent("Play_RaceStart", gameObject);
        GlobalMusicManager.Instance.SetRaceState("Racing");
    }

    private void Update()
    {
        if (state != RaceState.Racing || currentCourse == null) return;

        timePassed += Time.deltaTime;
        OnTimeChanged?.Invoke(timePassed);

        if (timePassed > currentCourse.maxTimeSeconds)
        {
            Fail("MaxTime");
            return;
        }

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
            if (d < minDistance) minDistance = d;
        }
        return minDistance;
    }

    public void OnTriggerHit(string courseID, RaceTrigger.TriggerType trigger, int checkpointIndex)
    {
        Debug.Log($"[RaceManager] OnTriggerHit: courseID={courseID} trigger={trigger} idx={checkpointIndex} state={state} next={nextCheckpoint} current={(currentCourse ? currentCourse.courseID : "NULL")}");

        if (currentCourse == null) return;
        if (state != RaceState.Racing) return;
        if (courseID != currentCourse.courseID) return;

        if (trigger == RaceTrigger.TriggerType.Checkpoint)
        {
            if (checkpointIndex != nextCheckpoint) return;

            nextCheckpoint++;
            UpdateCheckpointFx();

            AkUnitySoundEngine.PostEvent("Play_Checkpoint", gameObject);
            return;
        }

        if (trigger == RaceTrigger.TriggerType.Finish)
        {
            if (currentCourse.checkpoints != null && currentCourse.checkpoints.Count > 0)
            {
                if (nextCheckpoint < currentCourse.checkpoints.Count) return;
            }

            AkUnitySoundEngine.PostEvent("Play_Finish", gameObject);
            Finish();
        }
    }

    private void Finish()
    {
        state = RaceState.Finished;

        GlobalMusicManager.Instance.StartWorldMusic();
        GlobalMusicManager.Instance.SetRaceState("Finished");
        FreezePlayer(true);

        bool success = timePassed <= currentCourse.successTimeSeconds;

        if (success)
            OnRaceFinished?.Invoke(currentCourse.courseID, timePassed);
        else
            OnRaceFailed?.Invoke("TooSlow");

        StartCoroutine(CoroutineReturnToQuestgiver(success));
    }

    private void Fail(string reason)
    {
        Debug.LogWarning($"[RaceManager] Race failed: {reason}");
        state = RaceState.Failed;

        GlobalMusicManager.Instance.StartWorldMusic();
        GlobalMusicManager.Instance.SetRaceState("Idle");
        FreezePlayer(true);

        OnRaceFailed?.Invoke(reason);
        StartCoroutine(CoroutineReturnToQuestgiver(success: false));
    }

    private IEnumerator CoroutineReturnToQuestgiver(bool success)
    {
        yield return new WaitForSeconds(0.1f);

        if (currentCourse != null && currentCourse.returnPoint != null && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = currentCourse.returnPoint.position;
            rb.rotation = currentCourse.returnPoint.rotation;
        }

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

            fxParticles = checkpointArrowFxInstance.GetComponentsInChildren<ParticleSystem>(true);
        }
    }

    private Vector3 GetFxTargetPos(Collider col)
    {
        var b = col.bounds;
        var center = b.center;

        float y = useFixedY ? fixedY : b.min.y; // <- FIX: nicht mehr bounds.center.y (zu hoch)
        return new Vector3(center.x, y, center.z) + fxOffset;
    }

    private void UpdateCheckpointFx()
    {
        if (checkpointArrowFxInstance == null) return;
        if (currentCourse == null) { checkpointArrowFxInstance.SetActive(false); return; }

        bool hasCheckpoints = currentCourse.checkpoints != null && currentCourse.checkpoints.Count > 0;
        bool allCheckpointsDone = hasCheckpoints && nextCheckpoint >= currentCourse.checkpoints.Count;

        Vector3 targetPos;

        if (!allCheckpointsDone && hasCheckpoints)
        {
            var cp = currentCourse.checkpoints[nextCheckpoint];
            if (cp == null) { checkpointArrowFxInstance.SetActive(false); return; }
            targetPos = GetFxTargetPos(cp);
        }
        else
        {
            if (currentCourse.finishTrigger == null)
            {
                checkpointArrowFxInstance.SetActive(false);
                return;
            }

            targetPos = GetFxTargetPos(currentCourse.finishTrigger);
        }

        checkpointArrowFxInstance.transform.position = targetPos;

        checkpointArrowFxInstance.transform.localScale = Vector3.one * fxScale;

        if (faceBoat && boat != null)
        {
            Vector3 look = boat.transform.position - checkpointArrowFxInstance.transform.position;
            look.y = 0f;

            if (look.sqrMagnitude > 0.001f)
                checkpointArrowFxInstance.transform.rotation = Quaternion.LookRotation(-look.normalized, Vector3.up);
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

    private IEnumerator CoruEnableRaceTriggersNextFrame()
    {
        yield return null; 

        if (currentCourse != null)
            currentCourse.SetRaceTriggersActive(true);

        EnsureCheckpointFx();
        UpdateCheckpointFx();
    }
}