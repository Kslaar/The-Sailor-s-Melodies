using UnityEngine;

public class AudioStateController : MonoBehaviour
{
    public AK.Wwise.State SailingState;
    public AK.Wwise.State DockedState;

    private void OnEnable()
    {
        GameStateManager.Instance.OnStateChanged += HandleStateChanged;

    }
    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChanged;

    }
    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Sailing:
                Debug.Log("[Audio Switchign to Wwise State: Sailing");
                SailingState.SetValue();
                break;
            case GameState.Docked:
                Debug.Log("[Audio Switching to Wwise State; Docked");
                DockedState.SetValue();
                break;
        }


    }
}

