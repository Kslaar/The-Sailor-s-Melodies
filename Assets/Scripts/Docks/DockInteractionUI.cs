using UnityEngine;
using UnityEngine.UI;

public class DockInteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private  Button npcTalkButton;

    private DialogueAsset dialogue;

    private void Awake()
    {
        if (root != null) root.SetActive(false);

        if (npcTalkButton != null)
        {
            npcTalkButton.onClick.RemoveListener(Talk);
            npcTalkButton.onClick.AddListener(Talk);
        }
        else
        {
            Debug.LogWarning("[DockUI] talkButton is not assigned!");
        }   
    }
    public void Show(DialogueAsset dialogueAsset)
    {
        dialogue = dialogueAsset;
        Debug.Log($"[DockUI] Show() called. Dialogue={(dialogue != null ? dialogue.name : "NULL")}");
        if (root != null) root.SetActive(true);
    }

    public void Hide()
    {
        Debug.Log("[DockUI] Hide()");
        if (root != null) root.SetActive(false);
        dialogue = null;
    }

    public void Talk()
    {
        Debug.Log($"[DockUI] Talk() clicked. Dialogue={(dialogue != null ? dialogue.name : "NULL")}");

        if (dialogue == null) 
        {
            Debug.LogWarning("[DockUI] Talk clicked but no DialogueAsset set. Did DockZone.defaultDialogue get assigned?");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[DockUI] DialogueManager.Instance is NULL. Is DialogueManager in the scene and Awake setting Instance?");
            return;
        }
        GameStateManager.Instance.SetState(GameState.Dialogue);
        DialogueManager.Instance?.StartDialogue(dialogue);
        
        Hide(); 
    }
}
