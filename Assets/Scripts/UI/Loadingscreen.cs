using System.Collections;
using UnityEngine;

public class Loadingscreen : MonoBehaviour
{
    public static Loadingscreen Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public Coroutine FadeToBlack() => StartCoroutine(Fade(1f, blockRaycasts: true));
    public Coroutine FadeFromBlack() => StartCoroutine(Fade(0f, blockRaycasts: false));

    private IEnumerator Fade(float target, bool blockRaycasts)
    {
        canvasGroup.blocksRaycasts = blockRaycasts;
        canvasGroup.interactable = blockRaycasts;

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = blockRaycasts;
        canvasGroup.interactable = blockRaycasts;
    }
}
