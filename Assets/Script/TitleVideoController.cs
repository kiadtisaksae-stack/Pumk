using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[DisallowMultipleComponent]
[RequireComponent(typeof(VideoPlayer))]
public class TitleVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer mainVideoPlayer;
    [SerializeField] private List<VideoPlayer> videoPlayers = new List<VideoPlayer>();
    [SerializeField] private bool autoFindChildVideoPlayers = true;

    [Header("Skip")]
    [SerializeField] private bool allowSkipByClick = false;
    [SerializeField] private bool allowSkipByTouch = false;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Hide Targets")]
    [SerializeField] private bool autoCollectChildCanvasAndRawImage = true;
    [SerializeField] private List<GameObject> hideAfterFinished = new List<GameObject>();

    private bool isFinishing;

    private void Awake()
    {
        if (mainVideoPlayer == null)
        {
            mainVideoPlayer = GetComponent<VideoPlayer>();
        }

        if (autoFindChildVideoPlayers)
        {
            videoPlayers.Clear();
            VideoPlayer[] foundPlayers = GetComponentsInChildren<VideoPlayer>(true);
            for (int i = 0; i < foundPlayers.Length; i++)
            {
                if (foundPlayers[i] == null) continue;
                videoPlayers.Add(foundPlayers[i]);
            }
        }
        else if (mainVideoPlayer != null && !videoPlayers.Contains(mainVideoPlayer))
        {
            videoPlayers.Add(mainVideoPlayer);
        }

        if (autoCollectChildCanvasAndRawImage)
        {
            CollectHideTargetsFromChildren();
        }

        if (fadeCanvasGroup == null)
        {
            fadeCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }
    }

    private void OnEnable()
    {
        if (mainVideoPlayer != null)
        {
            mainVideoPlayer.loopPointReached += OnMainVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (mainVideoPlayer != null)
        {
            mainVideoPlayer.loopPointReached -= OnMainVideoFinished;
        }
    }

    private void Update()
    {
        // Deprecated flow: keep script stable but disable click/touch skip by default.
        if (isFinishing) return;
    }

    private void OnMainVideoFinished(VideoPlayer _)
    {
        BeginFinishFlow();
    }

    public void SkipToEndImmediately()
    {
        if (isFinishing) return;

        if (mainVideoPlayer != null)
        {
            if (mainVideoPlayer.frameCount > 0)
            {
                mainVideoPlayer.frame = (long)mainVideoPlayer.frameCount - 1;
            }
            else if (mainVideoPlayer.length > 0.05d)
            {
                mainVideoPlayer.time = mainVideoPlayer.length - 0.05d;
            }
        }

        BeginFinishFlow();
    }

    private void BeginFinishFlow()
    {
        if (isFinishing) return;
        isFinishing = true;
        StartCoroutine(FadeOutAndDisableRoutine());
    }

    private IEnumerator FadeOutAndDisableRoutine()
    {
        if (fadeCanvasGroup != null && fadeDuration > 0f)
        {
            float elapsed = 0f;
            float startAlpha = fadeCanvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            fadeCanvasGroup.alpha = 0f;
        }

        StopAllVideos();
        HideAllTargets();
    }

    private void StopAllVideos()
    {
        for (int i = 0; i < videoPlayers.Count; i++)
        {
            VideoPlayer vp = videoPlayers[i];
            if (vp == null) continue;
            vp.Stop();
        }
    }

    private void HideAllTargets()
    {
        for (int i = 0; i < hideAfterFinished.Count; i++)
        {
            GameObject target = hideAfterFinished[i];
            if (target == null) continue;
            target.SetActive(false);
        }
    }

    private void CollectHideTargetsFromChildren()
    {
        hideAfterFinished.Clear();

        Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] == null) continue;
            AddHideTarget(canvases[i].gameObject);
        }

        RawImage[] rawImages = GetComponentsInChildren<RawImage>(true);
        for (int i = 0; i < rawImages.Length; i++)
        {
            if (rawImages[i] == null) continue;
            AddHideTarget(rawImages[i].gameObject);
        }
    }

    private void AddHideTarget(GameObject go)
    {
        if (go == null) return;
        if (hideAfterFinished.Contains(go)) return;
        hideAfterFinished.Add(go);
    }
}
