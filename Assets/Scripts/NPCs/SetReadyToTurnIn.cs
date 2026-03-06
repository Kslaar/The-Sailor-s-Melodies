using UnityEngine;

[CreateAssetMenu(menuName = "Game/DialogueActions/Set Quest ReadyToTurnIn")]
public class SetReadyToTurnIn : DialogueAction
{
    public string questID;

    public override void Execute()
    {
        if (QuestManager.Instance == null) return;
        if (string.IsNullOrWhiteSpace(questID)) return;

        QuestManager.Instance.ForceSetState(questID, QuestState.ReadyToTurnIn);
    }
}