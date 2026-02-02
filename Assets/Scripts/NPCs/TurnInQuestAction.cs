using UnityEngine;

[CreateAssetMenu(menuName = "Game/DialogueActions/Turn In Quest")]
public class TurnInQuestAction : DialogueAction
{
    public string questID;

    public override void Execute()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[TurnInQuestAction] QuestManager.Instance is NULL.");
            return;
        }

        Debug.Log($"[TurnInQuestAction] TurnInQuest({questID})");
        QuestManager.Instance.TurnInQuest(questID);
    }
}
