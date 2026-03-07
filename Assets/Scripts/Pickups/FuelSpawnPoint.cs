using System.Collections;
using UnityEngine;

public class FuelSpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject fuelPrefab;
    [SerializeField] private float respawnSeconds = 5f;

    private GameObject current;

    private void Start()
    {
        SpawnNow();
    }

    public void NotifyPickedUp()
    {
        // falls noch da... weg damit
        if (current != null)
        {
            Destroy(current);
            current = null;
        }

        if (respawnSeconds > 0f)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSecondsRealtime(respawnSeconds);
        SpawnNow();
    }

    private void SpawnNow()
    {
        if (fuelPrefab == null) return;
        current = Instantiate(fuelPrefab, transform.position, transform.rotation, transform);
    }
}