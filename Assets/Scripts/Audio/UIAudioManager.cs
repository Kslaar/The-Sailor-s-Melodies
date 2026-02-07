using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("Wwise UI Events")]
    public AK.Wwise.Event clickEvent;
    public AK.Wwise.Event hoverEvent;
    public AK.Wwise.Event backEvent;
    public AK.Wwise.Event confirmEvent;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClick() => clickEvent?.Post(gameObject);
    public void PlayHover() => hoverEvent?.Post(gameObject);
    public void PlayBack() => backEvent?.Post(gameObject);
    public void PlayConfirm() => confirmEvent?.Post(gameObject);
}
