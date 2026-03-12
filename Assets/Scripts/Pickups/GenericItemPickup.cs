using UnityEngine;

public class GenericItemPickup : ItemPickup
{
    [Header("Item")]
    [SerializeField] private string itemID;

    protected override bool TryApply(Collider other)
    {
        if (string.IsNullOrWhiteSpace(itemID))
        {
            return false;
        }

        ItemPickupEvents.RaiseItemCollected(itemID);
        Debug.Log($"Collected: {itemID}");
        return true;
    }
}