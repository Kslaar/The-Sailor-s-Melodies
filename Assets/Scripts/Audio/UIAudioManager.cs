using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource uiSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip hover;
    [SerializeField] private AudioClip error;
    [SerializeField] private AudioClip start;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick()
    {
        PlaySound(click);
    }
    public void PlayStart()
    {
        PlaySound(start);
        
    }

    public void PlayHover()
    {
        PlaySound(hover);
    }

    public void PlayError()
    {
        PlaySound(error);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        uiSource.PlayOneShot(clip);
    }


}
