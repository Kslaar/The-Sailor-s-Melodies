using UnityEngine;

[CreateAssetMenu(menuName = "AudioPuzzle/Sound Signature")]
public class SoundSignature : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("UI")]
    public Sprite icon;

    [Header("Wwise")]
    [Tooltip("Event das abgespielt wird, wenn der Spieler das Recording aus der Hotbar abspielt.")]
    public AK.Wwise.Event playEvent;

    [Tooltip("Stop-Event")]
    public AK.Wwise.Event stopEvent;
}
