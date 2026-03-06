using UnityEngine;
using UnityEngine.EventSystems;

public class QuestButtonDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UIDescription questtip;
    private string message;

    public void Init(UIDescription tooltipRef, string msg)
    {
        questtip = tooltipRef;
        message = msg;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (questtip != null && !string.IsNullOrEmpty(message))
            questtip.Show(message);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (questtip != null)
            questtip.Hide();
    }

    private void OnDisable()
    {
        questtip?.Hide();
    }
}