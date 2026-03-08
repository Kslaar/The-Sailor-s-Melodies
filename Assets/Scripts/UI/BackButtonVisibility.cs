using UnityEngine;

public class BackButtonVisibility : MonoBehaviour
{
    void Update()
    {
        if (GameStateManager.Instance == null) return;

        bool show = GameStateManager.Instance.State != GameState.Dialogue || 
                    (DialogueManager.Instance != null && DialogueManager.Instance.AllowBack);

        if (gameObject.activeSelf != show)
            gameObject.SetActive(show);
    }
}
