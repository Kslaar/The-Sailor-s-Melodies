using UnityEngine;

public class NPCDialogueSelector : MonoBehaviour
{
    [Header("Quest Blocker")]
    public string quest2UnlockNextQuestID; // Muss erste Quest sein

    [Header("Quest 1")]
    [Tooltip("QuestID auf die der NPC reagiert")]
    public string questID_1;

    [Header("Dialogues 1")]
    public DialogueAsset notStarted_1;
    public DialogueAsset inProgress_1;
    public DialogueAsset readyToTurnIn_1;
    public DialogueAsset completed_1;
    public DialogueAsset fallbackRepeat_1;

    [Header("Second Quest")]
    public string questID_2;

    [Header("Dialogues 2")]
    public DialogueAsset notStarted_2;
    public DialogueAsset inProgress_2;
    public DialogueAsset readyToTurnIn_2;
    public DialogueAsset completed_2;
    public DialogueAsset fallbackRepeat_2;

    public DialogueAsset GetDialogue()
    {
        var qm = QuestManager.Instance;

        bool stage2Unlocked = IsStage2Unlocked(qm);

        if (!stage2Unlocked)
            return ResolveForQuest(qm, questID_1, notStarted_1, inProgress_1, readyToTurnIn_1, completed_1, fallbackRepeat_1);
        
        return ResolveForQuest(qm, questID_2, notStarted_2, inProgress_2, readyToTurnIn_2, completed_2, fallbackRepeat_2);
    }

    private bool IsStage2Unlocked(QuestManager qm)
    {
        if (string.IsNullOrWhiteSpace(quest2UnlockNextQuestID))
            return false;

        if (qm == null) return false;
        return qm.IsCompleted(quest2UnlockNextQuestID); // || qm.IsReadyToTurnIn(quest2UnlockNextQuestID);
    }
    public DialogueAsset ResolveForQuest(QuestManager qm, string questID, DialogueAsset notStarted, DialogueAsset inProgress, DialogueAsset readyToTurnIn, DialogueAsset completed, DialogueAsset fallbackRepeat)
    {
        if (string.IsNullOrEmpty(questID))
            return fallbackRepeat != null ? fallbackRepeat : notStarted;

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
