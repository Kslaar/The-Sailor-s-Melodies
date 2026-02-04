using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
{
    public bool playHoverSound = true;
    public bool playClickSound = true;
    public bool playStartSound = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound)
            UIAudioManager.Instance.PlayHover();
    }

    // Wird in Button OnClick() eingetragen
    public void OnClick()
    {
        if (playClickSound)
            UIAudioManager.Instance.PlayClick();


    }
    public void OnStart()
    {
        if (playStartSound)
            UIAudioManager.Instance.PlayStart();


    }

       

}


