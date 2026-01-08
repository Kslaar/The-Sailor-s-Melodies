using System.Collections;
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

    private void Reset()
    {
        boat = GetComponent<BoatControl>();
    }

    private void Update()
    {
        if (boat == null || data == null) return;
        if (MicrophoneInput.Instance == null) return;

        bool held = Keyboard.current.eKey.isPressed;

        if (!held)
        {
            wasAbove = false;
            return;
        }

        if (isBoosting) return;
        if (Time.time < lastUseTime + data.cooldown) return;

        float loud = MicrophoneInput.Instance.Loudness;

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
        boat.SetBoostActive(true);
        lastUseTime = Time.time;

        float oldSpeed = boat.speedMultiplier;
        float oldThrust = boat.thrustMultiplier;

        float loudFactor = Mathf.Lerp(1f, Mathf.Clamp(loudAtTrigger / Mathf.Max(0.0001f, data.minLoudness), 0.5f, 2f), data.loudnessScaling
        );

        float newSpeed = oldSpeed * data.speedMultiplier * loudFactor;
        float newThrust = oldThrust * data.thrustMultiplier;

        boat.speedMultiplier = newSpeed;
        boat.thrustMultiplier = newThrust;

        if (logBoost)
        {
            Debug.Log(
                $"[WindGust] BOOST activated for {data.duration:0.00}s | " +
                $"speedMul={data.speedMultiplier:0.00} loudFactor={loudFactor:0.00} => SpeedMultiplier={newSpeed:0.00}, " +
                $"ThrustMultiplier={newThrust:0.00}"
            );
        }

        yield return new WaitForSeconds(data.duration);

        boat.speedMultiplier = oldSpeed;
        boat.thrustMultiplier = oldThrust;

        boat.SetBoostActive(false);
        isBoosting = false;
    }
}
