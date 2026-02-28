using UnityEngine;

public class AnimalSoundTrigger : MonoBehaviour
{
    public AK.Wwise.Event animalEvent;
    public AK.Wwise.Event stopEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            animalEvent.Post(gameObject);

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
            stopEvent.Post(gameObject);


    }
}
