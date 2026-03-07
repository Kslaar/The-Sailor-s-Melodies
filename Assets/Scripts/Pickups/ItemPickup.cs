using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ItemPickup : MonoBehaviour
{
    [Header("Pickup Rule")]
    [Tooltip("Nur Objekte mit diesem Tag können aufgesammelt werden!")]
    [SerializeField] private string requiredTag = "Player";

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
    
        bool ok = TryApply(other);
        Debug.Log($"[ItemPickup] {name} TryApply={ok}");
    
        if (!ok) return;
    
        var resp = GetComponent<RespawnPickupItem>();
        Debug.Log($"[ItemPickup] {name} has RespawnPickupItem? {(resp != null)} destroyOnPickup={destroyOnPickup}");
    
        if (onPickupEffect != null)
            Instantiate(onPickupEffect, transform.position, Quaternion.identity);
    
        if (resp != null)
        {
            Debug.Log($"[ItemPickup] {name} RETURN due to respawner");
            return;
        }
    
        if (destroyOnPickup)
        {
            Debug.LogWarning($"[ItemPickup] {name} DESTROY by ItemPickup");
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
