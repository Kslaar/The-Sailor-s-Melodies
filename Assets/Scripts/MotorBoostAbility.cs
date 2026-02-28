using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotorBoostAbility : MonoBehaviour
{
    [Header("Wwise Audio")]
    public AK.Wwise.State SailingState;
    public AK.Wwise.State MotorState;

    public AK.Wwise.Event MotorStartEvent;
    public AK.Wwise.Event MotorstopEvent;
    public AK.Wwise.Event MotorLoopEvent;




    [SerializeField] private BoatControl boat;
    [SerializeField] private BoatFuel fuel;
    [SerializeField] private MotorBoostData data;

    [Header("Debug")]
    [SerializeField] private bool logAttempts = true;

    public bool engineRunning = false;
    public bool IsRunning => engineRunning;

    // Für die Pull-Mechanik
    private float pullDist = 0f;
    private float resetDist = 0f;
    private bool needsReset = false;
    private float lastAttemptTime = -999f;

    void Reset()
    {
        boat = GetComponent<BoatControl>();
        fuel = GetComponent<BoatFuel>();
    }

    void Update()
    {
        if (boat == null || fuel == null || data == null) return;

        var kb = Keyboard.current;
        var mouse = Mouse.current;

        bool held = kb != null && kb.spaceKey.isPressed;

        // Leertaste muss gedrückt gehalten bleiben, daher: Wenn sie nicht gehalten wird: 
        if (!held)
        {
            StopEngine();
            ResetPullState();
            return;
        }

        // Läuft der Motor jetzt, dann wird Kraftstoff verbraucht und der Boost geschieht
        if (engineRunning)
        {
            bool ok = fuel.UseFuel(data.fuelBurnPerSecond * Time.deltaTime);
            if (!ok || !fuel.HasFuel())
            {
                StopEngine();
                return;
            }
            return; // Boost hält an, solange wir die Leertaste drücken
        }

        // Haben wir kein Sprit mehr?
        if (!fuel.HasFuel()) return;

        if (mouse == null) return;

        // Wir nutzen das delta der Maus um die Fähigkeit zu aktivieren
        Vector2 delta = mouse.delta.ReadValue();
        float pullDown = -delta.y; // usually +y hoch und -y runter. Da wir Maus zu uns/runter ziehen -y

        if (needsReset)
        {
            float moveUp = delta.y; // Für dynamische Bewegung des ZIEHENS, runter ziehen für aktivierung und hoch für reset
            if(moveUp > 0f) 
                resetDist +=moveUp;

            if (resetDist >= data.resetDistancePixels)
            {
                needsReset = false;
                resetDist = 0f;
                pullDist = 0f;
            }
            return;
        }

        if (pullDown > 0f) pullDist += pullDown;

        if (pullDist >= data.pullDistancePixels && Time.time >= lastAttemptTime + data.attemptCooldown)
        {
            lastAttemptTime = Time.time;
            pullDist = 0f;
            needsReset = true;
            resetDist = 0f;

            TryStartEngine();
        }
    }

    private void TryStartEngine()
    {
        MotorStartEvent.Post(gameObject);

        float roll = Random.value;
        bool success = roll < data.startSuccessChance;

        if (logAttempts)
            Debug.Log($"[Motor] Start attempt: roll={roll:0.00} chance={data.startSuccessChance:0.00} => {(success ? "SUCCESS" : "FAIL")}");
        
        if (!success) return;

        StartEngine();
    }

    private void StartEngine()
    {
        if (engineRunning) return;
        if (!fuel.HasFuel()) return;

        engineRunning = true;

        boat.PushBoost(this);
        boat.SetSpeedMultiplier(this, data.speedMultiplier);
        boat.SetThrustMultiplier(this, data.thrustMultiplier);

        // Wwise Audio Start
        MotorState.SetValue();               //Setze State -> MotorActive   
        MotorStartEvent.Post(gameObject);    //Starte Motor-Start-Sound
        MotorLoopEvent.Post(gameObject);

       
    



        if (logAttempts)
            Debug.Log($"[Motor] ENGINE ON (x{data.speedMultiplier:0.0} speed) fuel={fuel.CurrentFuel:0.0}s");
    }

    private void StopEngine()
    {
        if (!engineRunning) return;

        engineRunning = false;

        boat.ClearMultipliers(this);
        boat.ReductBoost(this);

        //BOOST SOUND STOP
       MotorstopEvent.Post(gameObject);
        SailingState.SetValue();
    }

    private void ResetPullState()
    {
        pullDist = 0f;
        resetDist = 0f;
        needsReset = false;
    }
    private System.Collections.IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
{
    float startVolume = source.volume;
    float t = 0f;

    while (t < duration)
    {
        t += Time.deltaTime;
        source.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
        yield return null;
    }

    source.volume = targetVolume;
}

private System.Collections.IEnumerator FadeOutAndStop(AudioSource source, float duration)
{
    yield return FadeAudio(source, 0f, duration);
    source.Stop();
}


}

