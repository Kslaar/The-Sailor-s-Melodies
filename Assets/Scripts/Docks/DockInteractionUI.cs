using UnityEngine;
using UnityEngine.UI;

public class DockInteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    //[ SerializeField] private  Button npcTalkButton;
    // private DialogueAsset dialogue;
    private DockZone dock;

    private void Awake()
    {
        if (root != null) root.SetActive(false); 
    }
    public void Show(DockZone dockZone)
    {
        dock = dockZone;

        Debug.Log($"[DockUI] Show() called. Dock={(dock != null ? dock.name : "NULL")}");

        if (root != null) root.SetActive(true);

        if (dock == null || dock.npcs == null)
            return;

        // Buttons neu binden
        foreach (var npc in dock.npcs)
        {
            if (npc == null || npc.talkButton == null) continue;

            npc.talkButton.onClick.RemoveAllListeners();

            var localNPC = npc;
            npc.talkButton.onClick.AddListener(() =>
            {
                Talk(localNPC);
            });
        }
    }

    public void Hide()
    {
        Debug.Log("[DockUI] Hide()");
        if (root != null) root.SetActive(false);

        // Optional: listeners entfernen (sauber)
        if (dock != null && dock.npcs != null)
        {
            foreach (var npc in dock.npcs)
            {
                if (npc == null || npc.talkButton == null) continue;
                npc.talkButton.onClick.RemoveAllListeners();
            }
        }

        dock = null;
    }

    public void Talk(DockZone.NPCSlot npc)
    {
        var dialogue = npc.ResolveDialogue();

        Debug.Log($"[DockUI] Talk clicked. NPC={(npc.displayName)} Dialogue={(dialogue != null ? dialogue.name : "NULL")}");

        if (dialogue == null)
        {
            Debug.LogWarning("[DockUI] NPC has no Dialogue assigned/resolved.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[DockUI] DialogueManager.Instance is NULL.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogue);
        Hide();   
    }
}
