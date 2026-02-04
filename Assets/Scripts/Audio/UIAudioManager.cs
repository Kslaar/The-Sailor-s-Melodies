using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("Wwise UI Events")]
    public AK.Wwise.Event clickEvent;
    public AK.Wwise.Event hoverEvent;
    public AK.Wwise.Event backEvent;
    public AK.Wwise.Event confirmEvent;
    public AK.Wwise.Event startEvent;

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

    public void PlayClick()
    {
        if (clickEvent != null)
            clickEvent.Post(gameObject);
    }

    public void PlayHover()
    {
        if (hoverEvent != null)
            hoverEvent.Post(gameObject);
    }

    public void PlayBack()
    {
        if (backEvent != null)
            backEvent.Post(gameObject);
    }

    public void PlayConfirm()
    {
        if (confirmEvent != null)
            confirmEvent.Post(gameObject);
    }
    public void PlayStart()
    {
        if (startEvent != null)
            startEvent.Post(gameObject);
    }

}


