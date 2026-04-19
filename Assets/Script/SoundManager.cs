using UnityEngine;
using UnityEngine.Audio;

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
    private void Awake()
    {
        // ตรวจสอบว่ามี Instance อยู่แล้วหรือไม่ (ป้องกันการเกิด Duplicate)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ถ้ามีอยู่แล้วให้ลบทิ้ง
            return;
        }

        Instance = this;
        // ทำให้ Object นี้อยู่ข้าม Scene (ไม่ถูกลบเมื่อโหลดฉากใหม่)
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadVolume();
        PlayMusic(audioBG);
    }


    public void SetMasterVolume(float value)
    {
        // ใช้สูตร Log10 เพื่อให้เสียงเบา-ดังดูเป็นธรรมชาติ
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
        // ถ้าไม่เคยเซฟ ให้ค่าเริ่มต้นเป็น 0.75f
        float master = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        mainMixer.SetFloat("MasterVol", Mathf.Log10(master) * 20);
        mainMixer.SetFloat("MusicVol", Mathf.Log10(music) * 20);
        mainMixer.SetFloat("SFXVol", Mathf.Log10(sfx) * 20);
    }

    // ฟังก์ชันสำหรับเล่นเพลง (Music)
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // ฟังก์ชันสำหรับเล่นเสียง Effect (SFX)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }
}
