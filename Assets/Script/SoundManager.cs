using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Background Song")]
    public AudioClip audioBG;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Global Click SFX (Raw Pointer)")]
    [SerializeField] private bool enableGlobalClickSfx = true;
    [SerializeField] private AudioClip clickSfx;
    [Range(0f, 1f)]
    [SerializeField] private float clickSfxVolume = 1f;
    [Tooltip("Start playing click clip from this time (seconds).")]
    [Min(0f)]
    [SerializeField] private float clickStartOffsetSeconds = 1f;
    [Tooltip("Minimum interval between click sounds (seconds). Lower = more responsive.")]
    [Min(0f)]
    [SerializeField] private float clickMinIntervalSeconds = 0f;
    [Tooltip("Optional dedicated source for click SFX. If null, auto-create one.")]
    [SerializeField] private AudioSource clickSfxSource;
    private float lastClickSfxTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureClickAudioSource();
    }

    private void Start()
    {
        LoadVolume();
        PlayMusic(audioBG);
    }

    private void Update()
    {
        if (!enableGlobalClickSfx || clickSfx == null) return;
        if (!WasPointerPressedThisFrame()) return;

        if (clickMinIntervalSeconds > 0f)
        {
            float now = Time.unscaledTime;
            if (now - lastClickSfxTime < clickMinIntervalSeconds) return;
            lastClickSfxTime = now;
        }
        PlayClickSfxImmediate();
    }

    public void SetMasterVolume(float value)
    {
        mainMixer.SetFloat("MasterVol", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MasterVol", value);
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVol", value);
    }

    private void LoadVolume()
    {
        float master = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        mainMixer.SetFloat("MasterVol", Mathf.Log10(master) * 20);
        mainMixer.SetFloat("MusicVol", Mathf.Log10(music) * 20);
        mainMixer.SetFloat("SFXVol", Mathf.Log10(sfx) * 20);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    private bool WasPointerPressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void PlayClickSfxImmediate()
    {
        AudioSource source = clickSfxSource != null ? clickSfxSource : sfxSource;
        if (source == null || clickSfx == null) return;

        float safeStart = Mathf.Clamp(clickStartOffsetSeconds, 0f, Mathf.Max(0f, clickSfx.length - 0.01f));
        source.Stop();
        source.clip = clickSfx;
        source.volume = Mathf.Clamp01(clickSfxVolume);
        source.time = safeStart;
        source.Play();
    }

    private void EnsureClickAudioSource()
    {
        if (clickSfxSource != null) return;

        GameObject go = new GameObject("ClickSfxSource");
        go.transform.SetParent(transform, false);
        clickSfxSource = go.AddComponent<AudioSource>();
        clickSfxSource.playOnAwake = false;
        clickSfxSource.loop = false;
        clickSfxSource.spatialBlend = 0f;

        if (sfxSource != null && sfxSource.outputAudioMixerGroup != null)
        {
            clickSfxSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }
    }
}
