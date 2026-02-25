using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour
{
    public bool playHoverSound = true;
    public bool playClickSound = true;
    public bool playConfirmSound = true;
    public bool playBackSound = true;


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

        if (playConfirmSound)
            UIAudioManager.Instance.PlayConfirm();
        if (playBackSound)
            UIAudioManager.Instance.PlayBack();

    }

}

