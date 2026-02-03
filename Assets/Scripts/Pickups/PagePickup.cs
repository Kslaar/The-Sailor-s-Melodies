using UnityEngine;

public class PagePickup : ItemPickup
{
    [Header("Page")]
    [SerializeField] private string pageItemID; // erste Seite, zweit usw.

    protected override bool TryApply(Collider other)
    {
        if (string.IsNullOrWhiteSpace(pageItemID))
        {
            Debug.LogWarning("[PagePickup] pageItemID ist leer!");
            return false;
        }

        ItemPickupEvents.RaiseItemCollected(pageItemID);
        Debug.Log($"[PagePickup] Collected: {pageItemID}");
        return true;
    }
}
