using UnityEngine;
using UnityEngine.InputSystem;

public class BoatAbilityDebugHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WindGustAbility windGust;

    [Header("UI")]
    [SerializeField] private bool show = true;
    // [SerializeField] private  Key toggleKey = Key.F3;
    // [SerializeField] private  Key recalibrateKey = Key.F4;

    [SerializeField] private  float meterMax = 0.2f;
    [SerializeField] private  int width = 320;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.f3Key.wasPressedThisFrame) show = !show;

            if (kb.f4Key.wasPressedThisFrame && MicrophoneInput.Instance != null)
                MicrophoneInput.Instance.Recalibrate();
        }

        if (windGust == null)
            windGust = FindFirstObjectByType<WindGustAbility>();
    }

    private void OnGUI()
    {
        if (!show) return;

        var mic = MicrophoneInput.Instance;

        GUILayout.BeginArea(new Rect(12, 12, width, 220), GUI.skin.box);
        GUILayout.Label("DEBUG: Mic + WindGust");

        if (mic == null)
        {
            GUILayout.Label("MicrophoneInput: MISSING");
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label($"Mic Calibrating: {mic.isCalibrating}");
        GUILayout.Label($"NoiseFloor: {mic.NoiseFloor:0.000}");
        GUILayout.Label($"LoudnessRaw: {mic.loudnessRaw:0.000}");
        GUILayout.Label($"Loudness(gated): {mic.loudness:0.000}");

        float meter = Mathf.Clamp01(mic.loudness / Mathf.Max(0.0001f, meterMax));
        Rect r = GUILayoutUtility.GetRect(width - 30, 18);
        GUI.Box(r, "");
        Rect fill = new Rect(r.x, r.y, r.width * meter, r.height);
        GUI.Box(fill, "");

        if (windGust != null)
        {
            float thresh = windGust.threshold;
            bool ready = mic.loudness >= thresh;

            GUILayout.Space(6);
            GUILayout.Label($"Wind Threshold: {thresh:0.000} | READY: {ready}");
            GUILayout.Label($"Wind Boosting: {windGust.IsBoosting}");
            GUILayout.Label($"Wind Cooldown: {windGust.cooldownRemaining:0.00}s");
            GUILayout.Label("Hold E + blow to trigger.");
        }
        else
        {
            GUILayout.Space(6);
            GUILayout.Label("Windgustability not found!");
        }

        GUILayout.Space(6);
        GUILayout.Label("F3: Toggle HUD | F4: Recalibrate (be quiet gng!)");

        GUILayout.EndArea();
    }
}
