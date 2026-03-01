using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quests/Rewards/Unlock Engine")]
public class UnlockEngineReward : QuestReward
{
    public override void Apply()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.UnlockEngineAbility();
    }
}
