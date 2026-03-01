using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quests/Rewards/Unlock WindGust")]
public class UnlockWindGustReward : QuestReward
{
    public override void Apply()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.UnlockWindGustAbility();
    }
}