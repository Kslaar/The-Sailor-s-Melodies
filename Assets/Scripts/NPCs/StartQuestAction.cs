using UnityEngine;

[CreateAssetMenu(menuName="Game/DialogueActions/Start Quest")]
public class StartQuestAction : DialogueAction
{
    public string questID;

    public override void Execute()
    {
        if (string.IsNullOrWhiteSpace(questID))
        {
            Debug.LogWarning("[DialogueAction] StartQuestAction has empty questId.");
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[DialogueAction] QuestManager.Instance not found.");
            return;
        }

        QuestManager.Instance.StartQuest(questID);
        Debug.Log($"[DialogueAction] StartQuest executed: {questID}");
    }
}
