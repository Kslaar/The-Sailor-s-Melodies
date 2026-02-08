using UnityEngine;

public class PickupSound : MonoBehaviour
{
    public AK.Wwise.Event pickupEvent;   // Wwise-Event im Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupEvent.Post(gameObject);   // Sound abspielen

            Destroy(gameObject);
        }
    }
}

