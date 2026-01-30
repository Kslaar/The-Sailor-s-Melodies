using System;
using UnityEngine;

public class FuelPickup : ItemPickup
{
    [Header("Fuel Settings")]
    [SerializeField] private bool refillFull = true; 
    [SerializeField] private float addSeconds = 10f;

    [Header("Quest")]
    [SerializeField] private string itemID = "fuel_canister";

    protected override bool TryApply(Collider other)
    {
        var fuel = other.GetComponentInParent<BoatFuel>();

        if (fuel == null)
        {
            Debug.LogWarning($"[FuelPickup] No BoatFuel found in parents of '{other.name}'. Check player hierarchy.");
            return false;
        }

        if (refillFull) fuel.Refill();
        else fuel.AddFuel(addSeconds);

        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning("[FuelPickup] itemID is null/empty, not raising quest event.");
        }
        else
        {
            ItemPickupEvents.RaiseItemCollected(itemID);
        }

        Debug.Log("[Pickup] Fuel collected -> " + (refillFull ? "RefillFull" : $"Add {addSeconds}s"));
        return true;
    }
}
