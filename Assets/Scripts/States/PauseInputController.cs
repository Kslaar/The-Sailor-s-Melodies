using UnityEngine;
using UnityEngine.InputSystem;
using AK.Wwise;

public class PauseInputController : MonoBehaviour
{
    [SerializeField] private CursorLockController cursorLock;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            var gsm = GameStateManager.Instance;
            if (gsm == null) return;

            if (gsm.State == GameState.Paused)
                gsm.Unpause("Escape");
            else    
                gsm.TryPause("Escape");
        }
    }

    private void OnEnable()
    {
        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.OnStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState from, GameState to)
    {
        if (to == GameState.Paused)
        {
            Time.timeScale = 0f;

            AkUnitySoundEngine.Suspend(); // Hab hier Musik im pausenState pausiert BJÖRN
            if (cursorLock != null)
            {
                cursorLock.UnlockCursor();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } 
        }
        else if (from == GameState.Paused)
        {
            Time.timeScale = 1f;

            AkUnitySoundEngine.WakeupFromSuspend(); // BJÖRN hier geht's wieder los
            if (cursorLock != null)
            {
                cursorLock.UnlockCursor();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
