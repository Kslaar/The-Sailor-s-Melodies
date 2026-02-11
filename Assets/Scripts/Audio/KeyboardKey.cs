using UnityEngine;

public class KeyboardKey : MonoBehaviour
{
  public AK.Wwise.Event playEvent;

  public void PlayNote()
  {
    if (playEvent != null)
    playEvent.Post (gameObject);
    
  }
}
