using UnityEngine;

[CreateAssetMenu(menuName ="Game/DialogueActions/Start Quest")]
public class StartQuestAction : DialogueAction
{
    public string questID;

    public override void Execute()
    {
        QuestManager.Instance?.StartQuest(questID);
    }
}
