using System;
using UnityEngine;

public class FuelPickup : ItemPickup
{
    [Header("Fuel Settings")]
    [SerializeField] private bool refillFull = true; 
    [SerializeField] private float addSeconds = 10f;

    [Header("Tracking")]
    [SerializeField] private string itemID = "fuel_canister";

    protected override bool TryApply(Collider other)
    {
        var fuel = other.GetComponentInParent<BoatFuel>();
        if (fuel == null)
            return false;

        if (refillFull) fuel.Refill();
        else fuel.AddFuel(addSeconds);

        if (!string.IsNullOrEmpty(itemID))
            ItemPickupEvents.RaiseItemCollected(itemID);

        var spawn = GetComponentInParent<FuelSpawnPoint>();
        if (spawn != null)
            spawn.NotifyPickedUp();
        else 
            gameObject.SetActive(false);

        return true;
    }
}
