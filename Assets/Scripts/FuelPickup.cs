using System;
using UnityEngine;

public class FuelPickup : ItemPickup
{
    [Header("Fuel Settings")]
    [SerializeField] private bool refillFull = true; 
    [SerializeField] private float addSeconds = 10f;

    protected override bool TryApply(GameObject collector)
    {
        var fuel = collector.GetComponentInParent<BoatFuel>();

        if (fuel == null) return false;

        if (refillFull) fuel.Refill();
        else fuel.AddFuel(addSeconds);

        Debug.Log("[Pickup] Fuel collected -> " + (refillFull ? "RefillFull" : $"Add {addSeconds}s"));
        return true;
    }
}
