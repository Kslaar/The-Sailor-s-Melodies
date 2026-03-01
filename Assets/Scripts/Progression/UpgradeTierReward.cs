using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quests/Rewards/Unlock Tier")]
public class UpgradeTierReward : QuestReward
{
    public UpgradeType type;
    [Min(1)] public int amount = 1;

    public override void Apply()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.AddUpgradeTier(type, amount);
    }
}
