using UnityEngine;

public class NPCDialogueSelector : MonoBehaviour
{
    [Header("Quest")]
    [Tooltip("QuestID auf die dieser NPC reagiert.")]
    public string questID;

    [Header("Dialogues")]
    public DialogueAsset notStarted;
    public DialogueAsset inProgress;
    public DialogueAsset readyToTurnIn;
    public DialogueAsset completed;
    public DialogueAsset fallbackRepeat;

    public DialogueAsset GetDialogue()
    {
        if (string.IsNullOrEmpty(questID))
            return fallbackRepeat != null ? fallbackRepeat : notStarted;

        var qm = QuestManager.Instance;
        if (qm == null)
            return notStarted != null ? notStarted :fallbackRepeat;

        if (!qm.HasStarted(questID))
            return notStarted != null ? notStarted : fallbackRepeat;

        if (qm.IsCompleted(questID))
            return completed != null ? completed : fallbackRepeat;

        if (qm.IsReadyToTurnIn(questID))
            return readyToTurnIn != null ? readyToTurnIn : inProgress;

        return inProgress != null ? inProgress : notStarted;
    }
}
