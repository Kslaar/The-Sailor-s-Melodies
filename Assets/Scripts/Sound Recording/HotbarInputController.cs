using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarInputController : MonoBehaviour
{
    public RecordHotbar hotbar;
    public PlayerSoundRecorderInteractor recorderInteractor;

    [SerializeField] private float scrollLimit = 0.01f;

    void Update()
    {
        if (hotbar == null) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        var mouse = Mouse.current;

        if (mouse != null)
        {
            float scrollY = mouse.scroll.ReadValue().y;
            if (scrollY > scrollLimit) hotbar.SelectNext();
            if (scrollY < -scrollLimit) hotbar.SelectPrevious();
        }

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
