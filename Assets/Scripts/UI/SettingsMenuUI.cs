using UnityEngine;

public class SettingsMenuUI : MonoBehaviour
{
    public void Resume()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        gsm.Unpause("Resume button");
    }
}
