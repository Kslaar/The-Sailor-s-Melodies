using UnityEngine;

[CreateAssetMenu(menuName = "Boat/Abilities/Motor Sprint")]
public class MotorBoostData : ScriptableObject
{
    [Header("Multipliers")]
    public float speedMultiplier = 2f;
    public float thrustMultiplier = 2f;

    [Header("Fuel")]
    [Tooltip("Verbrauch in 'Sekunden Sprit' pro Sekunde Motorlauf.")]
    public float fuelBurnPerSecond = 1f;

    [Header("Start Mechanic")]
    [Range(0f, 1f)]
    public float startSuccessChance = 0.33f;

    [Tooltip("Wie weit muss die Maus nach unten gezogen werden (Pixel), um einen Startversuch auszulösen.")]
    public float pullDistancePixels = 250f;

    [Tooltip("Wie weit muss die Maus wieder nach oben (Pixel), um einen neuen Pull zu erlauben.")]
    public float resetDistancePixels = 120f;

    [Tooltip("Kleiner Cooldown zwischen Startversuchen (verhindert doppelte Trigger in einem Frame).")]
    public float attemptCooldown = 0.15f;
}
