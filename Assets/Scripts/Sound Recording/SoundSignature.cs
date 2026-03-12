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
    public AK.Wwise.Event playEvent;
    public AK.Wwise.Event stopEvent;
    public AK.Wwise.Event hintEvent;

    [Header("Playback")]
    public float previewDuration = 3f;
}