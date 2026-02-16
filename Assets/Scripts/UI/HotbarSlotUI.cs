using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    public Image iconImage;
    public GameObject selectedFrame;

    [Header("Empty Slot")]
    public Sprite emptySprite;
    public float emptyAlpha = 0.25f;

    public void SetIcon(Sprite icon)
    {
        iconImage.sprite = icon != null ? icon : emptySprite;
        var c = iconImage.color;
        c.a = 1f;
        iconImage.color = c;
    }

    public void SetEmpty()
    {
        iconImage.sprite = emptySprite;
        var c = iconImage.color;
        c.a = emptyAlpha;
        iconImage.color = c;
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
            selectedFrame.SetActive(selected);
    }
}
