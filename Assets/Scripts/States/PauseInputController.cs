using UnityEngine;
using UnityEngine.InputSystem;
using AK.Wwise;

public class PauseInputController : MonoBehaviour
{
    private void OnEnable()
    {
        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.OnStateChanged += OnStateChanged;

        ApplyPauseEffects(GameStateManager.Instance != null && GameStateManager.Instance.State == GameState.Paused);
    }

    private void OnDisable()
    {
        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.OnStateChanged -= OnStateChanged;
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (!kb.escapeKey.wasPressedThisFrame) return;

        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        if (gsm.State != GameState.Paused)
            gsm.TryPause("Escape open pause");
    }

    private void OnStateChanged(GameState from, GameState to)
    {
        if (to == GameState.Paused) ApplyPauseEffects(true);
        else if (from == GameState.Paused) ApplyPauseEffects(false);
    }

    private void ApplyPauseEffects(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;

       AkUnitySoundEngine.SetRTPCValue("PauseDuck", paused ? 1f : 0f);  
    }
}
