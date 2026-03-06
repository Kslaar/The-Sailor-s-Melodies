using UnityEngine;

[CreateAssetMenu(menuName = "Game/DialogueActions/Give Item")]
public class GiveItemAction : DialogueAction
{
    public string itemID;
    public int amount = 1;

    public override void Execute()
    {
        if (string.IsNullOrWhiteSpace(itemID)) return;

        for (int i = 0; i < Mathf.Max(1, amount); i++)
            ItemPickupEvents.RaiseItemCollected(itemID);

        Debug.Log($"[GiveItemAction] Gave item '{itemID}' x{amount}");
    }
}