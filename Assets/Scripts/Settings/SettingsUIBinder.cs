using UnityEngine;
using UnityEngine.UI;

public class SettingsUIBinder : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle invertToggle;
    [SerializeField] private Toggle windGustToggle;

    private void OnEnable()
    {
        var sm = SettingsManager.Instance;
        if (sm == null) return;

        // Initialwerte setzen
        if (masterSlider) masterSlider.SetValueWithoutNotify(sm.Data.masterVolume);
        if (musicSlider)  musicSlider.SetValueWithoutNotify(sm.Data.musicVolume);
        if (sfxSlider)    sfxSlider.SetValueWithoutNotify(sm.Data.sfxVolume);
        if (invertToggle) invertToggle.SetIsOnWithoutNotify(sm.Data.invertControls);
        if (windGustToggle) windGustToggle.SetIsOnWithoutNotify(sm.Data.windGustButtonUse);

        HookListeners();
    }

    private void HookListeners()
    {
        if (masterSlider)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetMaster(v));
        }

        if (musicSlider)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetMusic(v));
        }

        if (sfxSlider)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetSfx(v));
        }

        if (invertToggle)
        {
            invertToggle.onValueChanged.RemoveAllListeners();
            invertToggle.onValueChanged.AddListener(b => SettingsManager.Instance.SetInvertWASD(b));
        }

        if (windGustToggle)
        {
            windGustToggle.onValueChanged.RemoveAllListeners();
            windGustToggle.onValueChanged.AddListener(b => SettingsManager.Instance.SetWindGustButtonUse(b));
        }
    }
}