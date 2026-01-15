using UnityEngine;

[CreateAssetMenu(menuName ="Game/Quests/Rewards/Enable Component")]
public class EnableComponentReward : QuestReward
{
    public string componentTypeName; 
    public override void Apply()
    {
        var boat = FindFirstObjectByType<BoatControl>();
        if (boat == null) return;

        var comp = boat.GetComponent(componentTypeName) as Behaviour;
        if (comp != null) comp.enabled = true;
    }
}
