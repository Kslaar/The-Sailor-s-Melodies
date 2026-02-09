using UnityEngine;

public class MusicStateController : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("MusicManager gestartet");

        //Musik starten
        AkUnitySoundEngine.PostEvent("Play_Music_World", gameObject);

        //Standardzustand setzen
        SetExplorationMusic();

    }

    public void SetExplorationMusic()
    {
        AkUnitySoundEngine.SetSwitch("MusicState", "Exploration", gameObject);
    }
    public void SetIslandMusic()
    {
        AkUnitySoundEngine.SetSwitch("MusicState", "Island_01", gameObject);
    }


}
