using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DockZone : MonoBehaviour
{
    public Transform snapPoint;
    public Camera dockCamera;

    [Header("Dock Interactions")]
    public DockInteractionUI dockUI;

    [Header("NPCs on this island")]
    public List<NPCSlot> npcs = new();

    [Serializable]
    public class NPCSlot
    {
        public string displayName;
        public Button talkButton;
        public NPCDialogueSelector selector;
        public DialogueAsset fallbackDialogue;

        public DialogueAsset ResolveDialogue()
        {
            if (selector != null) return selector.GetDialogue();
            return fallbackDialogue;
        }
    }
}
