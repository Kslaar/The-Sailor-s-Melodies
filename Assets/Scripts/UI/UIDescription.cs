using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIDescription : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup cg;

    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(18f, -18f);
    [SerializeField] private Vector2 padding = new Vector2(12f, 12f);

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        HideImmediate();
    }

    void Update()
    {
        if (cg == null || cg.alpha <= 0.001f) return;
        FollowMouse();
    }

    public void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (text != null) text.text = message;

        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (root != null) root.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();
        FollowMouse();
        ClampToScreen();
    }

    public void Hide()
    {
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (cg != null) cg.alpha = 0f;
        if (root != null) root.gameObject.SetActive(false);
    }

    private void FollowMouse()
    {
        if (root == null || canvas == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screenPos = mouse.position.ReadValue();

        RectTransform canvasRect = (RectTransform)canvas.transform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out var localPoint);

        // Offset rechts-unten
        localPoint += offset;

        root.anchoredPosition = localPoint;

        ClampToScreen();
    }

    private void ClampToScreen()
    {
        if (root == null || canvas == null) return;

        var canvasRect = (RectTransform)canvas.transform;

        Vector3[] corners = new Vector3[4];
        root.GetWorldCorners(corners);

        for (int i = 0; i < 4; i++)
            corners[i] = canvasRect.InverseTransformPoint(corners[i]);

        float minX = corners[0].x;
        float maxX = corners[2].x;
        float minY = corners[0].y;
        float maxY = corners[2].y;

        var pos = root.anchoredPosition;

        float canvasMinX = canvasRect.rect.xMin + padding.x;
        float canvasMaxX = canvasRect.rect.xMax - padding.x;
        float canvasMinY = canvasRect.rect.yMin + padding.y;
        float canvasMaxY = canvasRect.rect.yMax - padding.y;

        if (minX < canvasMinX) pos.x += (canvasMinX - minX);
        if (maxX > canvasMaxX) pos.x -= (maxX - canvasMaxX);
        if (minY < canvasMinY) pos.y += (canvasMinY - minY);
        if (maxY > canvasMaxY) pos.y -= (maxY - canvasMaxY);

        root.anchoredPosition = pos;
    }
}