using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FuelPickup : MonoBehaviour
{
    [SerializeField] private bool refillFull = true; 
    [SerializeField] private float addSeconds = 10f;

    [Header("FX")]
    [SerializeField] private GameObject onPickupEffect;
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        BoatFuel fuel = other.GetComponentInParent<BoatFuel>();

        if (fuel == null) return;

        if (refillFull) fuel.Refill();
        else fuel.AddFuel(addSeconds);

        if (onPickupEffect != null)
            Instantiate(onPickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
