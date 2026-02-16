using UnityEngine;

public class HotbarInputController : MonoBehaviour
{
    public RecordHotbar hotbar;
    public PlayerSoundRecorderInteractor recorderInteractor;

    [Header("Keys")]
    public KeyCode recordKey = KeyCode.R;
    public KeyCode clearKey = KeyCode.C;
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;

    private bool PlaybackHeld => Input.GetKey(KeyCode.LeftControl) ||
                                Input.GetKey(KeyCode.RightControl) ||
                                Input.GetKey(KeyCode.RightCommand) ||
                                Input.GetKey(KeyCode.LeftCommand);

    void Update()
    {
        if (hotbar == null) return;

        if (Input.GetKeyDown(leftKey)) hotbar.SelectPrevious();
        if (Input.GetKeyDown(rightKey)) hotbar.SelectNext();

        if (PlaybackHeld && (Input.GetKey(KeyCode.LeftControl) ||
                                Input.GetKey(KeyCode.RightControl) ||
                                Input.GetKey(KeyCode.RightCommand) ||
                                Input.GetKey(KeyCode.LeftCommand)))
        {
            hotbar.PlaySelected();
        }

        if (Input.GetKeyDown(clearKey))
        {
            hotbar.ClearSelected();
        }

        if (Input.GetKeyDown(recordKey))
        {
            if (recorderInteractor != null && recorderInteractor.CurrentSource != null)
            {
                recorderInteractor.CurrentSource.RecordTo(hotbar);
            }
        }
    }
}
