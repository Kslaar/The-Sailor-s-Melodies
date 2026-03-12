using UnityEngine;

[CreateAssetMenu(menuName = "Boat/Abilities/Wind Gust")]
public class WindGustData : ScriptableObject
{
    [Header("Cooldown")]
    public float cooldown = 1.5f;

    [Header("Mic Trigger")]
    [Tooltip("Ab dieser Loudness darf ausgelöst werden.")]
    public float minLoudness = 0.015f;

    [Tooltip("Verhindert Flattern am Threshold (minLoudness - hysteresis = Reset-Schwelle).")]
    public float triggerHysteresis = 0.005f;

    [Header("Boost")]
    [Tooltip("Wie lange der Windstoß-Buff aktiv ist.")]
    public float duration = 0.7f;

    [Tooltip("Multiplikator für maximale Geschwindigkeit (z.B. 3x).")]
    public float speedMultiplier = 3f;

    [Tooltip("Multiplikator für Beschleunigungskraft während des Buffs.")]
    public float thrustMultiplier = 3f;

    [Tooltip("Skaliert Einfluss der Loudness auf Stärke (0 = immer gleich stark, 1 = linear).")]
    [Range(0f, 1f)]
    public float loudnessScaling = 0.7f;
}
