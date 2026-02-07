using UnityEngine;

[CreateAssetMenu(fileName = "UIAudioDatabase", menuName = "Audio/UI Audio Database")]
public class UIAudioDatabase : ScriptableObject
{
    public AK.Wwise.Event clickEvent;
    public AK.Wwise.Event hoverEvent;
    public AK.Wwise.Event backEvent;
    public AK.Wwise.Event confirmEvent;
}
