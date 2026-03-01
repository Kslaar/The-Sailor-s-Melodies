using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotorBoostAbility : MonoBehaviour
{
    
    [SerializeField] private BoatControl boat;
    [SerializeField] private BoatFuel fuel;
    [SerializeField] private MotorBoostData data;
    [SerializeField] private BoatAudio audio; 

    [Header("Debug")]
    [SerializeField] private bool logAttempts = true;

    public bool engineRunning = false;
    public bool IsRunning => engineRunning;

    // Für die Pull-Mechanik
    private float _pullDist = 0f;
    private float _resetDist = 0f;
    private bool _needsReset = false;
    private float _lastAttemptTime = -999f;

    //////////////////////////////////////////////////////

    private float _runtimeStartChance = -1f;
    private float _runtimeSpeedMult = -1f;

    public void SetRuntimeStartChance(float value) => _runtimeStartChance = Mathf.Clamp01(value);
    public void SetRuntimeSpeedMultiplier(float value) => _runtimeSpeedMult = Mathf.Max(0.01f, value);

    private float CurrentStartChance => _runtimeStartChance >= 0f ? _runtimeStartChance : data.startSuccessChance;
    private float CurrentSpeedMult => _runtimeSpeedMult >= 0 ? _runtimeSpeedMult : data.speedMultiplier;

    //////////////////////////////////////////////////////

    void Reset()
    {
        boat = GetComponent<BoatControl>();
        fuel = GetComponent<BoatFuel>();
        audio = GetComponent<BoatAudio>();
    }

    void Update()
    {
        if (boat == null || fuel == null || data == null) return;

        var gsm = GameStateManager.Instance; 
        bool allowedState = (gsm == null) || (gsm.State == GameState.Sailing || gsm.State == GameState.Racing);

        if (!allowedState) // Wir wollen nicht dass man im Hafen theoretisch den Motor aktivieren kann
        {
            StopEngine();
            ResetPullState();
            return;
        }
        
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

        bool hasWOrSInput = (kb != null) && (kb.wKey.isPressed || kb.sKey.isPressed);
        if (!hasWOrSInput)
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

        if (_needsReset)
        {
            float moveUp = delta.y; // Für dynamische Bewegung des ZIEHENS, runter ziehen für aktivierung und hoch für reset
            if(moveUp > 0f) 
                _resetDist +=moveUp;

            if (_resetDist >= data.resetDistancePixels)
            {
                _needsReset = false;
                _resetDist = 0f;
                _pullDist = 0f;
            }
            return;
        }

        if (pullDown > 0f) _pullDist += pullDown;

        if (_pullDist >= data.pullDistancePixels && Time.time >= _lastAttemptTime + data.attemptCooldown)
        {
            _lastAttemptTime = Time.time;
            _pullDist = 0f;
            _needsReset = true;
            _resetDist = 0f;

            TryStartEngine();
        }
    }

    private void TryStartEngine()
    {
        audio.PlayMotorStartAttempt();
        float roll = Random.value;
        bool success = roll < CurrentStartChance;

        if (logAttempts)
            Debug.Log($"[Motor] Start attempt: roll={roll:0.00} chance={CurrentStartChance:0.00} => {(success ? "SUCCESS" : "FAIL")}");
        
        if (!success) return;

        StartEngine();
    }

    private void StartEngine()
    {
        if (engineRunning) return;
        if (!fuel.HasFuel()) return;

        engineRunning = true;

        boat.PushBoost(this);
        boat.SetSpeedMultiplier(this, CurrentSpeedMult);
        boat.SetThrustMultiplier(this, data.thrustMultiplier);

        audio.ActivateMotor();
    }

    private void StopEngine()
    {
        if (!engineRunning) return;

        engineRunning = false;

        boat.ClearMultipliers(this);
        boat.ReductBoost(this);

       audio.DeactivateMotor();
    }

    private void ResetPullState()
    {
        _pullDist = 0f;
        _resetDist = 0f;
        _needsReset = false;
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

