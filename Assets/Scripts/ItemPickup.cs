using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ItemPickup : MonoBehaviour
{
    [Header("Pickup Rule")]
    [Tooltip("Nur Objekte mit diesem Tag können aufgesammelt werden!")]
    [SerializeField] private string requiredTag = "Player";

    [Header("FX")]
    [SerializeField] private GameObject onPickupEffect;
    [SerializeField] private bool destroyOnPickup = true;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) 
            return;
        
        GameObject collector = other.attachedRigidbody != null // Falls COllider child ist
            ? other.attachedRigidbody.gameObject
            : other.transform.root.gameObject;

        if (TryApply(collector))
        {
            if (onPickupEffect != null)
                Instantiate(onPickupEffect, transform.position, Quaternion.identity);

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }

    protected abstract bool TryApply(GameObject collector);
}
