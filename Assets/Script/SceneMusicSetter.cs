using System.Collections;
using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
public class SceneMusicSetter : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool onlyIfDifferentClip = true;
    [SerializeField] private bool waitForSoundManagerIfMissing = true;
    [SerializeField] private float waitTimeoutSeconds = 2f;

    private void Start()
    {
        if (sceneMusic == null) return;

        if (waitForSoundManagerIfMissing && SoundManager.Instance == null)
        {
            StartCoroutine(WaitAndApplyMusic());
            return;
        }

        ApplyMusic();
    }

    private IEnumerator WaitAndApplyMusic()
    {
        float elapsed = 0f;

        while (SoundManager.Instance == null && elapsed < waitTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyMusic();
    }

    private void ApplyMusic()
    {
        SoundManager manager = SoundManager.Instance;
        if (manager == null) return;

        if (onlyIfDifferentClip)
        {
            FieldInfo field = typeof(SoundManager).GetField("musicSource", BindingFlags.NonPublic | BindingFlags.Instance);
            AudioSource source = field != null ? field.GetValue(manager) as AudioSource : null;
            if (source != null && source.clip == sceneMusic && source.isPlaying) return;
        }

        manager.PlayMusic(sceneMusic, loop);
    }
}
