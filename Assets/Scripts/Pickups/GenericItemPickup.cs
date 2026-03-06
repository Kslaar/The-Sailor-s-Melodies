using UnityEngine;

public class GenericItemPickup : ItemPickup
{
    [Header("Item")]
    [SerializeField] private string itemID;

    protected override bool TryApply(Collider other)
    {
        if (string.IsNullOrWhiteSpace(itemID))
        {
            Debug.LogWarning($"[GenericItemPickup] itemID empty on {name}");
            return false;
        }

        ItemPickupEvents.RaiseItemCollected(itemID);
        Debug.Log($"[GenericItemPickup] Collected: {itemID}");
        return true;
    }
}