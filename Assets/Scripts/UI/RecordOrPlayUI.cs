using TMPro;
using UnityEngine;

public class RecordOrPlayUI : MonoBehaviour
{
    public PlayerSoundRecorderInteractor interactor;
    public RecordHotbar hotbar;
    public TMP_Text text;

    [Header("Texts")]
    public string playTemplate = "{KEY}: Play \"{NAME}\"";
    public string recordTemplate = "R: Record \"{NAME}\"";

    void Update()
    {
        if (text == null || hotbar == null || interactor == null) return;

        // 1) Record-UItipp hat Priorität, aber nur wenn Sound NICHT schon in Hotbar ist
        var source = interactor.CurrentSource;
        if (source != null && source.signature != null)
        {
            bool alreadyInHotbar = HotbarContains(source.signature);
            if (!alreadyInHotbar)
            {
                text.enabled = true;
                text.text = recordTemplate.Replace("{NAME}", source.signature.displayName);
                return;
            }
        }

        // 2) Sonst: Play-UITipp wenn selektierter Slot belegt ist
        var selectedSig = GetSelectedSignatureOrNull();
        if (selectedSig != null)
        {
            text.enabled = true;
            text.text = playTemplate.Replace("{KEY}", CtrlLabel()).Replace("{NAME}", selectedSig.displayName);
            return;
        }

        // 3) Sonst: Text aus
        text.enabled = false;
    }

    bool HotbarContains(SoundSignature sig)
    {
        var slots = hotbar.Slots;
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] == sig) return true;
        return false;
    }

    SoundSignature GetSelectedSignatureOrNull()
    {
        var slots = hotbar.Slots;
        int idx = hotbar.SelectedIndex;
        if (idx < 0 || idx >= slots.Count) return null;
        return slots[idx];
    }

    string CtrlLabel()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer ||
               Application.platform == RuntimePlatform.WindowsEditor ||
               Application.platform == RuntimePlatform.LinuxPlayer ||
               Application.platform == RuntimePlatform.LinuxEditor
            ? "Strg"
            : "Ctrl";
    }
}
