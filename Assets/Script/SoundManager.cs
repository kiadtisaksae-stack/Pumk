using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

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
        PlayMusic(audioBG);
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
