using UnityEngine;

public class NPCClickable : MonoBehaviour
{
    public DialogueAsset dialogue;
    private void OnMouseDown()
    {
        if (GameStateManager.Instance == null || GameStateManager.Instance.State != GameState.Docked)
            return;
        
        if (dialogue == null) return;

        DialogueManager.Instance?.StartDialogue(dialogue);
    }
}
