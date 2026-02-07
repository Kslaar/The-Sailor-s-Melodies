using System.Collections;
using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    public static MicrophoneInput Instance;

    [Header("Mic")]
    [Tooltip("Leer = Default-Mic")]
    [SerializeField] private string deviceName = "";

    [Header("Sampling")]
    [SerializeField] private int sampleWindow = 256;

    [Header("Processing")]
    [Tooltip("0...1 (höher = reagiert schneller, niedriger = glatter)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothing = 0.2f;

    [Tooltip("Unterhalb dieses Werts gilt es als Rauschen/Still wir kalibrieren Noise floor aber automatisch, Wert nur für Start...")]
    [SerializeField] private float noiseFloor = 0.01f;

    [Header("Auto Calibration")]
    [Tooltip("Beim Start für X Sekunden warten und Stille messen und noiseFloor setzen!")]
    [SerializeField] private bool autoCalibrationOnStart = true;
    [SerializeField] private float calibrationSeconds = 2f;

    [Tooltip("Sicherheitsaufschlag auf gemessene Stille (verhindert Trigger durch bspw. Grundrauschen)")]
    [SerializeField] private float calibrationMargin = 0.01f;

    [Tooltip("Glättung für noiseFloor-Anpassung (umso niedriger, desto stabiler!)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float noiseFloorSmoothing = 0.25f;

    public float loudnessRaw { get; private set; }
    public float loudness { get; private set; }
    public float NoiseFloor => noiseFloor;
    public bool isCalibrating { get; private set; }

    private AudioClip clip;
    private bool micReady;

    private float[] sampleBuffer;

    ///////////////////////////////////////////////////////////////
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        StartCoroutine(InitMicRoutine());
    }

    ///////////////////////////////////////////////////////////////
    private IEnumerator InitMicRoutine()
    {
#if UNITY_EDITOR_OSX || UNITY_ANDROID || UNITY_STANDALONE_OSX || UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }
#endif

        StartMic();

        if (autoCalibrationOnStart) StartCoroutine(CalibrateNoiseFloor());

        yield break;
    }

    ///////////////////////////////////////////////////////////////

    private void OnDisable()
    {
        if (!micReady) return;
        if (Microphone.devices == null || Microphone.devices.Length == 0) return;

        // Normaaalerweise bei null = default mic...
        if (Microphone.IsRecording(deviceName))
            Microphone.End(deviceName);
    }

    ///////////////////////////////////////////////////////////////

    private void StartMic()
    {
        if (Microphone.devices == null || Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone devices found.");
            micReady = false;
            clip = null;
            return;
        }

        if (sampleBuffer == null || sampleBuffer.Length != sampleWindow)
            sampleBuffer = new float[sampleWindow];

        // Wenn deviceName leer: Default
        clip = Microphone.Start(deviceName, true, 1, 44100);
        micReady = clip != null;

        Debug.Log($"[Mic] Started. Device='{(string.IsNullOrEmpty(deviceName) ? "Default" : deviceName)}' Ready={micReady}");
    }

    ///////////////////////////////////////////////////////////////

    private void Update()
    {
        if (!micReady || clip == null) return;

        loudnessRaw = GetRmsLoudness();

        float gated = Mathf.Max(0f, loudnessRaw - noiseFloor); // Das Gate verursacht, das alles unter noiseFloor 0 wird!

        // Exponentielles Glätten
        // float previous = loudness;
        loudness = Mathf.Lerp(loudness, gated, smoothing);

        /*
        if (logLoudnessChanges)
        {
            if (Mathf.Abs(loudness - lastLoggedLoudness) >= loudnessLogDelta)
            {
                lastLoggedLoudness = loudness;
                Debug.Log($"[Mic] Loudness changed -> {loudness:0.000} (raw {LoudnessRaw:0.000}, gated {gated:0.000})");
            }
        }
        */
    }

    ///////////////////////////////////////////////////////////////
    
    public void Recalibrate()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(CalibrateNoiseFloor());
    }

    ///////////////////////////////////////////////////////////////
    
    private IEnumerator CalibrateNoiseFloor()
    {
        if (!micReady || clip == null) yield break;

        isCalibrating = true;

        float t = 0f;
        float sum = 0f;
        int n = 0;

        yield return null;

        while (t < calibrationSeconds)
        {
            float rms = GetRmsLoudness();
            sum += rms;
            n++;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        float avg = (n > 0) ? (sum / n) : noiseFloor;
        float targetFloor = avg + calibrationMargin;

        // noiseFloor smoothen um krasse Sprünge zu vermeiden
        noiseFloor = Mathf.Lerp(noiseFloor, targetFloor, noiseFloorSmoothing);

        isCalibrating = false;

        Debug.Log($"[Mic] Calibrated noiseFloor={noiseFloor:0.000} (avg silence={avg:0.000}, margin={calibrationMargin:0.000})");
    }

    ///////////////////////////////////////////////////////////////

    private float GetRmsLoudness()
    {
        if (clip == null) return 0f;

        int micPos = Microphone.GetPosition(deviceName) - sampleWindow;
        if (micPos < 0) return 0f;

        if (sampleBuffer == null || sampleBuffer.Length != sampleWindow)
            sampleBuffer = new float[sampleWindow];

        clip.GetData(sampleBuffer, micPos);

        // RMS (robuster als Abs-Mittel)
        float sum = 0f;
        for (int i = 0; i < sampleBuffer.Length; i++)
            sum += sampleBuffer[i] * sampleBuffer[i];

        return Mathf.Sqrt(sum / sampleBuffer.Length);
    }
}
