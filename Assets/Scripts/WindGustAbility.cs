using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WindGustAbility : MonoBehaviour
{
    [SerializeField] private BoatControl boat;
    [SerializeField] private WindGustData data;

    [Header("Debug")]
    [SerializeField] private bool logTrigger = true;
    [SerializeField] private bool logBoost = true;

    private float lastUseTime = -999f;
    private bool wasAbove;
    private bool isBoosting;

    public bool IsBoosting => isBoosting;
    public float cooldownRemaining => Mathf.Max(0f, (lastUseTime + data.cooldown) - Time.time);
    public float threshold => data.minLoudness;

    //////////////////////////////////////////////////////

    private float _runtimeSpeedMult = -1f;
    private float _runtimeThrustMult = -1f;

    public void SetRuntimeSpeedMultiplier(float value) => _runtimeSpeedMult = Mathf.Max(0.01f, value);
    public void SetRuntimeThrustMultiplier(float value) => _runtimeThrustMult = Mathf.Clamp01(value);

    private float CurrentSpeedMult => _runtimeSpeedMult >= 0 ? _runtimeSpeedMult : data.speedMultiplier;
    private float CurrentThrustMult => _runtimeThrustMult >= 0f ? _runtimeThrustMult : data.thrustMultiplier;

    //////////////////////////////////////////////////////

    private void Reset()
    {
        boat = GetComponent<BoatControl>();
    }

    private void Update()
    {
        if (boat == null || data == null) return;

        // Wenn Spieler mit Tasteneingabe spielt
        bool held = Keyboard.current.eKey.isPressed;
        if (!held)
        {
            wasAbove = false;
            return;
        }
        if (isBoosting) return;
        if (Time.time < lastUseTime + data.cooldown) return;

        bool useButton = SettingsManager.Instance != null && SettingsManager.Instance.Data.windGustButtonUse;

        if (useButton)
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                StartCoroutine(DoBoost(data.minLoudness));
            }
            return;
        }

        // Wenn Spieler mit Mic spielt
        if (MicrophoneInput.Instance == null) return;

        float loud = MicrophoneInput.Instance.loudness;

        float onThreshold = data.minLoudness;
        float offThreshold = Mathf.Max(0f, data.minLoudness - data.triggerHysteresis);

        bool above = loud >= onThreshold;

        if (!above && loud <= offThreshold)
            wasAbove = false;

        // Rising edge: unter -> über
        if (above && !wasAbove)
        {
            wasAbove = true;

            if (logTrigger)
                Debug.Log($"[WindGust] Loudness TRIGGER accepted! loud={loud:0.000} (threshold={onThreshold:0.000})");

            StartCoroutine(DoBoost(loud));
        }
    }

    private IEnumerator DoBoost(float loudAtTrigger)
    {
        isBoosting = true;
        lastUseTime = Time.time;


        float loudFactor = Mathf.Lerp(1f, Mathf.Clamp(loudAtTrigger / Mathf.Max(0.0001f, data.minLoudness), 0.5f, 2f), data.loudnessScaling);

        boat.PushBoost(this);
        boat.SetSpeedMultiplier(this, CurrentSpeedMult * loudFactor);
        boat.SetThrustMultiplier(this, CurrentThrustMult);

        if (logBoost)
        {
            Debug.Log
            (
                $"[WindGust] BOOST activated for {data.duration:0.00}s | " +
                $"speedMul={data.speedMultiplier:0.00} loudFactor={loudFactor:0.00}"
            );
        }

        yield return new WaitForSeconds(data.duration);

        boat.ClearMultipliers(this);
        boat.ReductBoost(this);

        isBoosting = false;
    }
}
