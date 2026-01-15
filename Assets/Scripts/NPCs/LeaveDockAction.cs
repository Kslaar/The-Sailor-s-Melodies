using UnityEngine;

[CreateAssetMenu(menuName ="Game/DialogueActions/Leave Dock")]
public class LeaveDockAction : DialogueAction
{
    public override void Execute()
    {
        var docking = FindFirstObjectByType<BoatDockingController>();
        if (docking != null) docking.UndockNow();
    }
}
