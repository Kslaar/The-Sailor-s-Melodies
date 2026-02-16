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

        bool playPressed = 
            kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame ||
            kb.leftMetaKey.wasPressedThisFrame || kb.rightMetaKey.wasPressedThisFrame; // Meta = Command (mac)

        if (playPressed)
        {
            Debug.Log("[Input] Play pressed (Ctrl/Command)");
            hotbar.PlaySelected();
        }

        if (kb.cKey.wasPressedThisFrame)
        {
            Debug.Log("[Input] Clear pressed (C)");
            hotbar.ClearSelected();
        }

        if (kb.rKey.wasPressedThisFrame)
        {
            Debug.Log("[Input] Record pressed (R)");
            if (recorderInteractor != null && recorderInteractor.CurrentSource != null)
            {
                recorderInteractor.CurrentSource.RecordTo(hotbar);
            }
        }
    }
}
