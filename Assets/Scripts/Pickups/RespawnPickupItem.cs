using System.Collections;
using UnityEngine;

public class RespawnPickupItem : MonoBehaviour
{
    public string itemPickupID;
    public float respawnSeconds = 5f;
    public bool disableInsteadOfDestroy;

    private Collider _col;
    private Renderer[] _rend;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _rend = GetComponentsInChildren<Renderer>(true);

        /*
        if (string.IsNullOrWhiteSpace(itemPickupID))
            itemPickupID = gameObject.name;
        */
    }

    public void Consume()
    {
        if (respawnSeconds <= 0f)
        {
            if (disableInsteadOfDestroy) gameObject.SetActive(false);
            else Destroy(gameObject);
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        SetVisible(false);
        yield return new WaitForSecondsRealtime(respawnSeconds);
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (_col != null) _col.enabled = visible;
        if (_rend != null)
        {
            foreach (var r in _rend)
            {
                if (r != null) r.enabled = visible;       
            }
        }
    }

    void OnDestroy()
    {
        Debug.LogWarning($"[FuelPickup] DESTROYED: Fuelcanister");
    }
}
