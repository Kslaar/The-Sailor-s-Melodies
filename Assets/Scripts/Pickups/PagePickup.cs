using UnityEngine;

public class PagePickup : ItemPickup
{
    [Header("Page")]
    [SerializeField] private string pageItemID;

    protected override bool TryApply(Collider other)
    {
        if (string.IsNullOrWhiteSpace(pageItemID))
        {
            return false;
        }

        ItemPickupEvents.RaiseItemCollected(pageItemID);
        Debug.Log($"Collected: {pageItemID}");
        return true;
    }
}
