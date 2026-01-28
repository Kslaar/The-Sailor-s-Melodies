using UnityEngine;

public class SoundManager : MonoBehaviour
{
  public static SoundManager Instance;
    public AudioSource audioSource;

    void Awake()
    {
        Instance = this;
    }

    public void PlayPickupSound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    public AudioClip pickupSound;
    void OnTriggerEnter(Collider other)
    { 
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlayPickupSound(pickupSound);
            Destroy(gameObject);
        }
    }


}
