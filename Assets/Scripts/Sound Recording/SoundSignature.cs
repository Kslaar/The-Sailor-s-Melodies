using UnityEngine;

[CreateAssetMenu(menuName = "AudioPuzzle/Sound Signature")]
public class SoundSignature : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public AudioClip previewClip;
}
