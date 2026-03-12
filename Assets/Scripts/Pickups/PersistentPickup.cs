using System.Collections;
using UnityEngine;

public class PersistentPickup : MonoBehaviour
{
    public string itemID;

    [Tooltip("true = destroy! false = SetActive(false)")]
    public bool destroyIfCollected = true;
    void OnEnable()
    {
        ItemCollectionRegistry.OnRegistryLoaded -= CheckNow;
        ItemCollectionRegistry.OnRegistryLoaded += CheckNow;
        CheckNow();
    }
    private void OnDisable()
    {
        ItemCollectionRegistry.OnRegistryLoaded -= CheckNow;
    }

    private void CheckNow()
    {
        var reg = ItemCollectionRegistry.Instance;
        if (reg == null) return;
        if (string.IsNullOrWhiteSpace(itemID)) return;

        int c = reg.GetCount(itemID);

        if (c > 0)
        {
            if (destroyIfCollected) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }

}
