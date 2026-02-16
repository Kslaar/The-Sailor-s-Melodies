using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarInputController : MonoBehaviour
{
    public RecordHotbar hotbar;
    public PlayerSoundRecorderInteractor recorderInteractor;

    void Update()
    {
        if (hotbar == null) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftArrowKey.wasPressedThisFrame) hotbar.SelectPrevious();
        if (kb.rightArrowKey.wasPressedThisFrame) hotbar.SelectNext();

        // Play mit Ctrl (Strg)
        if (kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame)
            hotbar.PlaySelected();

        if (kb.cKey.wasPressedThisFrame)
            hotbar.ClearSelected();

        if (kb.rKey.wasPressedThisFrame)
        {
            var src = recorderInteractor != null ? recorderInteractor.CurrentSource : null;
            if (src != null) src.RecordTo(hotbar);
        }
    }
}
