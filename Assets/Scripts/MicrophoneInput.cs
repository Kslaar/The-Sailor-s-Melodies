using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    public static MicrophoneInput Instance;

    [Header("Mic")]
    [Tooltip("Leer lassen = Default-Mikrofon")]
    [SerializeField] private string deviceName = null;

    [Header("Sampling")]
    [SerializeField] private int sampleWindow = 256;

    [Header("Processing")]
    [Tooltip("0..1 (höher = reagiert schneller, niedriger = glatter)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothing = 0.2f;

    [Tooltip("Unterhalb dieses Werts gilt es als Rauschen/Still")]
    [SerializeField] private float noiseFloor = 0.01f;

    [Header("Debug")]
    [SerializeField] private bool logLoudnessChanges = true;
    [Tooltip("Nur loggen, wenn sich Loudness um mindestens diesen Betrag ändert (verhindert Spam).")]
    [SerializeField] private float loudnessLogDelta = 0.02f;

    public float LoudnessRaw { get; private set; }
    public float Loudness { get; private set; }

    private AudioClip clip;
    private bool micReady;

    private float lastLoggedLoudness;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        StartMic();
    }

    private void OnDisable()
    {
        if (Microphone.IsRecording(deviceName))
            Microphone.End(deviceName);
    }

    private void StartMic()
    {
        if (Microphone.devices == null || Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone devices found.");
            micReady = false;
            return;
        }

        // Wenn deviceName leer: Default
        clip = Microphone.Start(deviceName, true, 1, 44100);
        micReady = clip != null;
    }

    private void Update()
    {
        if (!micReady || clip == null) return;

        LoudnessRaw = GetRmsLoudness();
        float gated = Mathf.Max(0f, LoudnessRaw - noiseFloor);

        // Exponentielles Glätten
        float previous = Loudness;
        Loudness = Mathf.Lerp(Loudness, gated, smoothing);

        if (logLoudnessChanges)
        {
            if (Mathf.Abs(Loudness - lastLoggedLoudness) >= loudnessLogDelta)
            {
                lastLoggedLoudness = Loudness;
                Debug.Log($"[Mic] Loudness changed -> {Loudness:0.000} (raw {LoudnessRaw:0.000}, gated {gated:0.000})");
            }
        }
    }

    private float GetRmsLoudness()
    {
        int micPos = Microphone.GetPosition(deviceName) - sampleWindow;
        if (micPos < 0) return 0f;

        float[] data = new float[sampleWindow];
        clip.GetData(data, micPos);

        // RMS (robuster als Abs-Mittel)
        float sum = 0f;
        for (int i = 0; i < data.Length; i++)
            sum += data[i] * data[i];

        return Mathf.Sqrt(sum / data.Length);
    }
}
