using DG.Tweening;
using UnityEngine;

public class animateElements : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float slideAmount = 50f; // how much it slides vertically
    public Ease easeType = Ease.OutCubic;

    private Vector2 originalPos;
    private Tween currentTween;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;

        // Start hidden
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPos - new Vector2(0, slideAmount);
    }

    public void OnEnable()
    {
        gameObject.SetActive(true);

        // Reset before animation
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPos - new Vector2(0, slideAmount);

        // Kill old tweens
        currentTween?.Kill();

        // Play fade in + slide up
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, duration));
        seq.Join(rectTransform.DOAnchorPos(originalPos, duration).SetEase(easeType));
        currentTween = seq;
    }

    public void Hide()
    {
        // Kill old tweens
        currentTween?.Kill();

        // Play fade out + slide down
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, duration));
        seq.Join(rectTransform.DOAnchorPos(originalPos - new Vector2(0, slideAmount), duration).SetEase(easeType));
        seq.OnComplete(() => gameObject.SetActive(false));
        currentTween = seq;
    }
}
