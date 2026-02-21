using UnityEngine;

public class NPCSoundManager : MonoBehaviour
{
    [Header("Wwise Events")]
    public AK.Wwise.Event greetEvent;
    public AK.Wwise.Event idleEvent;
    public AK.Wwise.Event happyEvent;
    public AK.Wwise.Event concernedEvent;

    [Header("Wwise Switches / States")]
    public AK.Wwise.Switch npcTypeSwitch;
    public AK.Wwise.State npcMoodState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set NPC type on Spawn
        npcTypeSwitch?.SetValue(gameObject);
        npcMoodState?.SetValue();

        
    }
public void PlayGreet() => greetEvent?.Post(gameObject);
public void PlayIdle() => idleEvent?.Post(gameObject);
public void PlayHappy() => happyEvent?.Post(gameObject);
public void PlayConcerned() => concernedEvent?.Post(gameObject);


   

}
