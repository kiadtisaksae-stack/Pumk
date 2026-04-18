using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[DisallowMultipleComponent]
[RequireComponent(typeof(VideoPlayer))]
public class StoryVideoController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private VideoPlayer mainVideoPlayer;
    [SerializeField] private List<VideoPlayer> allVideoPlayers = new List<VideoPlayer>();

    [Header("UI")]
    [SerializeField] private Canvas videoCanvas;
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private Button skipButton;
    [SerializeField] private CanvasGroup fadeGroup;

    [Header("Behavior")]
    [SerializeField] private bool autoPlayOnStart = false;
    [SerializeField] private bool hideUIWhenStopped = true;
    [SerializeField] private float fadeOutDuration = 0.45f;

    private RenderTexture runtimeRenderTexture;
    private bool isFinishing;
    private bool isPlaying;

    public bool AutoPlayOnStart
    {
        get => autoPlayOnStart;
        set => autoPlayOnStart = value;
    }

    private void Reset()
    {
        mainVideoPlayer = GetComponent<VideoPlayer>();
    }

    private void Awake()
    {
        if (mainVideoPlayer == null)
        {
            mainVideoPlayer = GetComponent<VideoPlayer>();
        }

        if (allVideoPlayers == null || allVideoPlayers.Count == 0)
        {
            CollectAllVideoPlayers();
        }

        if (fadeGroup == null && videoCanvas != null)
        {
            fadeGroup = videoCanvas.GetComponent<CanvasGroup>();
        }

        BindSkipButton();
        PrepareRenderTarget();
    }

    private void OnEnable()
    {
        if (mainVideoPlayer != null)
        {
            mainVideoPlayer.loopPointReached += OnMainVideoEnded;
        }
    }

    private void Start()
    {
        if (autoPlayOnStart)
        {
            PlayStoryVideo();
        }
        else if (hideUIWhenStopped)
        {
            SetVideoUIVisible(false);
        }
    }

    private void OnDisable()
    {
        if (mainVideoPlayer != null)
        {
            mainVideoPlayer.loopPointReached -= OnMainVideoEnded;
        }
    }

    private void OnDestroy()
    {
        if (runtimeRenderTexture != null)
        {
            runtimeRenderTexture.Release();
            Destroy(runtimeRenderTexture);
            runtimeRenderTexture = null;
        }
    }

    public void PlayStoryVideo()
    {
        if (mainVideoPlayer == null) return;

        isFinishing = false;
        isPlaying = true;
        PrepareRenderTarget();

        SetVideoUIVisible(true);
        SetFadeAlpha(1f);

        mainVideoPlayer.frame = 0;
        mainVideoPlayer.time = 0;
        mainVideoPlayer.Play();

        for (int i = 0; i < allVideoPlayers.Count; i++)
        {
            VideoPlayer vp = allVideoPlayers[i];
            if (vp == null || vp == mainVideoPlayer) continue;
            vp.Play();
        }
    }

    public void SkipVideo()
    {
        if (!isPlaying || isFinishing) return;
        BeginFinishFlow();
    }

    private void OnMainVideoEnded(VideoPlayer _)
    {
        BeginFinishFlow();
    }

    private void BeginFinishFlow()
    {
        if (isFinishing) return;
        isFinishing = true;
        StartCoroutine(FadeOutAndStopRoutine());
    }

    private IEnumerator FadeOutAndStopRoutine()
    {
        if (fadeGroup != null && fadeOutDuration > 0f)
        {
            float elapsed = 0f;
            float startAlpha = fadeGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);
                fadeGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
        }

        StopAllVideoPlayers();
        isPlaying = false;

        if (hideUIWhenStopped)
        {
            SetVideoUIVisible(false);
            SetFadeAlpha(1f);
        }
    }

    private void StopAllVideoPlayers()
    {
        for (int i = 0; i < allVideoPlayers.Count; i++)
        {
            VideoPlayer vp = allVideoPlayers[i];
            if (vp == null) continue;
            vp.Stop();
        }
    }

    private void CollectAllVideoPlayers()
    {
        allVideoPlayers = new List<VideoPlayer>();
        VideoPlayer[] found = GetComponentsInChildren<VideoPlayer>(true);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] == null) continue;
            allVideoPlayers.Add(found[i]);
        }
    }

    private void BindSkipButton()
    {
        if (skipButton == null) return;
        skipButton.onClick.RemoveListener(SkipVideo);
        skipButton.onClick.AddListener(SkipVideo);
    }

    private void PrepareRenderTarget()
    {
        if (mainVideoPlayer == null || videoRawImage == null) return;

        int width = Mathf.Max(1, Screen.width);
        int height = Mathf.Max(1, Screen.height);

        if (runtimeRenderTexture != null && (runtimeRenderTexture.width != width || runtimeRenderTexture.height != height))
        {
            runtimeRenderTexture.Release();
            Destroy(runtimeRenderTexture);
            runtimeRenderTexture = null;
        }

        if (runtimeRenderTexture == null)
        {
            runtimeRenderTexture = new RenderTexture(width, height, 0)
            {
                name = $"RT_StoryVideo_{gameObject.name}",
                useMipMap = false,
                autoGenerateMips = false
            };
        }

        mainVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        mainVideoPlayer.targetTexture = runtimeRenderTexture;
        videoRawImage.texture = runtimeRenderTexture;
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeGroup != null)
        {
            fadeGroup.alpha = alpha;
        }
    }

    private void SetVideoUIVisible(bool isVisible)
    {
        if (videoCanvas != null)
        {
            videoCanvas.gameObject.SetActive(isVisible);
        }
        else if (videoRawImage != null)
        {
            videoRawImage.gameObject.SetActive(isVisible);
        }
    }

    public void EditorAssignReferences(
        VideoPlayer player,
        Canvas canvas,
        RawImage rawImage,
        Button skip,
        CanvasGroup canvasGroup)
    {
        mainVideoPlayer = player;
        videoCanvas = canvas;
        videoRawImage = rawImage;
        skipButton = skip;
        fadeGroup = canvasGroup;

        CollectAllVideoPlayers();
        BindSkipButton();
    }
}
