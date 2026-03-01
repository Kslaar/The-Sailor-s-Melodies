using UnityEngine;

public class BoatProgressionApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatControl boat;
    [SerializeField] private BoatFuel fuel;
    [SerializeField] private MotorBoostAbility motorAbility;
    [SerializeField] private MotorBoostData motorData;
    [SerializeField] private WindGustAbility windAbility;
    [SerializeField] private WindGustData windData;

    [Header("Engine Start Chance Tiers")]
    [SerializeField] private float[] engineStartChance = { 0.33f, 0.55f, 0.77f, 1.00f };

    [Header("Engine Fuel Tiers (MaxFuelSeconds)")]
    [SerializeField] private float[] engineFuelSeconds = { 30f, 35f, 40f, 45f };

    [Header("Engine Speed Tiers (Multiplier)")]
    [SerializeField] private float[] engineSpeedMultiplier = { 2.0f, 2.2f, 2.4f, 2.6f };

    [Header("Boat Base Speed Tiers (ForwardSpeed)")]
    [SerializeField] private float[] boatForwardSpeed = { 6f, 6.5f, 7f, 7.5f };

    [Header("WindGust Tiers (Multiplier)")]
    [SerializeField] private float[] windSpeedMultiplier = { 3.0f, 3.2f, 3.4f, 3.6f };
    [SerializeField] private float[] windThrustMultiplier = { 3.0f, 3.2f, 3.4f, 3.6f };


    private void Reset()
    {
        boat = GetComponent<BoatControl>();
        fuel = GetComponent<BoatFuel>();
        motorAbility = GetComponent<MotorBoostAbility>();
        windAbility = GetComponent<WindGustAbility>();
    }

    void OnEnable()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged += Apply;
    }

    void OnDisable()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged -= Apply;
    }

    void Start()
    {
        Apply();
    }

    public void Apply()
    {
        var prog = ProgressionManager.Instance;
        if (prog == null) return;

        var st = prog.State;
        st.Clamp();

        // Freischalten der Fähigkeiten
        if (motorAbility != null) motorAbility.enabled = st.engineUnlocked;
        if (windAbility != null) windAbility.enabled = st.windGustUnlocked;

        // Motor-Fähigkeit
        if (motorAbility != null)
        {
            float chance = engineStartChance[Mathf.Clamp(st.engineStartChanceTier, 0, engineStartChance.Length - 1)];
            float speed = engineSpeedMultiplier[Mathf.Clamp(st.engineSpeedTier, 0, engineSpeedMultiplier.Length - 1)];

            motorAbility.SetRuntimeStartChance(chance);
            motorAbility.SetRuntimeSpeedMultiplier(speed);
        }

        // Fuelmax upgrade
        if (fuel != null)
        {
            float maxFuel = engineFuelSeconds[Mathf.Clamp(st.engineFuelTier, 0, engineFuelSeconds.Length - 1)];
            fuel.SetMaxFuel(maxFuel, fill: true);
        }

        // Boat basespeed
        if (boat != null)
        {
            float fwdSpeed = boatForwardSpeed[Mathf.Clamp(st.boatBaseSpeedTier, 0, boatForwardSpeed.Length - 1)];
            boat.SetBaseForwardSpeed(fwdSpeed);
        }

        // Windgust-Fähigkeit
        if (windAbility != null)
        {
            float ws = windSpeedMultiplier[Mathf.Clamp(st.windGustTier, 0, windSpeedMultiplier.Length - 1)];
            float wt = windThrustMultiplier[Mathf.Clamp(st.windGustTier, 0, windThrustMultiplier.Length - 1)];

            windAbility.SetRuntimeSpeedMultiplier(ws);
            windAbility.SetRuntimeThrustMultiplier(wt);
        }
    }
}
