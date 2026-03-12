using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIDescription : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup cg;

    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(18f, -18f);
    [SerializeField] private Vector2 padding = new Vector2(12f, 12f);

    private Canvas canvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRect = canvas.transform as RectTransform;

        HideImmediate();
    }

    private void Update()
    {
        if (cg == null || cg.alpha <= 0.001f) return;
        FollowMouse();
    }

    public void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (text != null)
        {
            text.text = message;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (root != null)
            root.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();
        FollowMouse();
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
        if (root == null || canvasRect == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screenPos = mouse.position.ReadValue();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out var localPoint))
            return;

        Canvas.ForceUpdateCanvases();

        Vector2 tooltipSize = root.rect.size;
        Rect canvasBounds = canvasRect.rect;

        bool placeLeft = false;
        bool placeAbove = false;

        float rightEdge = localPoint.x + offset.x + tooltipSize.x;
        float bottomEdge = localPoint.y + offset.y - tooltipSize.y;

        if (rightEdge > canvasBounds.xMax - padding.x)
            placeLeft = true;

        if (bottomEdge < canvasBounds.yMin + padding.y)
            placeAbove = true;

        Vector2 pivot = new Vector2(
            placeLeft ? 1f : 0f,
            placeAbove ? 0f : 1f
        );

        root.pivot = pivot;
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);

        Vector2 finalOffset = new Vector2(
            placeLeft ? -offset.x : offset.x,
            placeAbove ? Mathf.Abs(offset.y) : -Mathf.Abs(offset.y)
        );

        root.anchoredPosition = localPoint + finalOffset;

        ClampToScreen();
    }

    private void ClampToScreen()
    {
        if (root == null || canvasRect == null) return;

        Vector3[] corners = new Vector3[4];
        root.GetWorldCorners(corners);

        for (int i = 0; i < 4; i++)
            corners[i] = canvasRect.InverseTransformPoint(corners[i]);

        float minX = corners[0].x;
        float maxX = corners[2].x;
        float minY = corners[0].y;
        float maxY = corners[2].y;

        Vector2 pos = root.anchoredPosition;

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