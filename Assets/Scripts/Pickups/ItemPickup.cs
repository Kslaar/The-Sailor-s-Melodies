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

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !HasTagInParents(other.transform, requiredTag)) 
            return;

        if (TryApply(other))
        {
            if (onPickupEffect != null)
                Instantiate(onPickupEffect, transform.position, Quaternion.identity);

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }

    private static bool HasTagInParents(Transform t, string tag)
    {
        while (t != null)
        {
            if (t.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }

    protected abstract bool TryApply(Collider other);
}
