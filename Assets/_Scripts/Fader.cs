using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class Fader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1.0f;
    public float fadeOutDuration = 0.6f;

    [SerializeField] private bool isFading = false;
    [SerializeField] bool autoFadeIn;
    [SerializeField] bool startZero = false;
    [SerializeField] float delay = 0.5f;

    [Space]
    [SerializeField] UnityEvent onShowAction;
    [SerializeField] UnityEvent onEnableAction;
    [SerializeField] float onShowActionDelay = 3;
    [SerializeField] bool releaseVideoPlayers = false;
    public void Awake()
    {
        // Ensure the Canvas Group is not null
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("Canvas Group not found. Please assign a Canvas Group to this script or attach it to a GameObject with a Canvas Group component.");
                enabled = false; // Disable the script
                return;
            }
            if (startZero)
            {
                canvasGroup.alpha = 0;
            }
        }

    }
    private void OnEnable()
    {
        if (startZero)
        {
            canvasGroup.alpha = 0;
        }
        try { onEnableAction?.Invoke(); } catch { }
        if (autoFadeIn)
        {
            StartCoroutine(waitor(delegate
            {
                FadeIn();
            }));
        }
        if (releaseVideoPlayers)
            ReleaseAllVideoPlayersInChildren();
    }
    private void OnDisable()
    {
        isFading = false;
    }

    IEnumerator waitor(System.Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback(); // Execute the delegate
    }
    public void FadeIn()
    {
        //if (!isFading)
        //{
        StartCoroutine(FadeCanvasGroup(null, fadeDuration, 0f, 1f));
        //}
    }
    public bool IsFading() { return isFading; }

    public void FadeOut()
    {
        if (!isFading)
        {
            if (this.gameObject.activeSelf)
            {
                StartCoroutine(FadeCanvasGroup(null, fadeOutDuration, 1f, 0f));
            }
        }
    }
    public void FadeOutTurnOff()
    {
        if (!isFading)
        {
            FadeOut(delegate { this.gameObject.SetActive(false); });
        }
    }
    public void FadeOut(System.Action callback)
    {
        //if (!isFading)
        // {
        try
        {
            if (this.gameObject.activeSelf)
            {
                StartCoroutine(FadeCanvasGroup(callback, fadeOutDuration, 1f, 0f));
            }
        }
        catch { }
        //}
    }

    private IEnumerator FadeCanvasGroup(System.Action callback, float fadeDuration, float startAlpha, float targetAlpha)
    {
        isFading = true;

        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        try
        {

            //where it will trigger the action once it shows the page with a delay
            IEnumerator waitor()
            {
                yield return new WaitForSeconds(onShowActionDelay);
                if (this.gameObject.activeSelf && !isFading)
                {
                    onShowAction?.Invoke();
                }
            }
            StartCoroutine(waitor());
            callback();
        }
        catch { }
        canvasGroup.alpha = targetAlpha;
        isFading = false;
    }
    VideoPlayer[] videoPlayers;
    private void ReleaseAllVideoPlayersInChildren()
    {
        if (videoPlayers == null)
            videoPlayers = GetComponentsInChildren<VideoPlayer>();

        foreach (VideoPlayer videoPlayer in videoPlayers)
        {
            if (videoPlayer.targetTexture != null)
            {
                // Release the RenderTexture
                videoPlayer.targetTexture.Release();
            }
        }
    }
}
