using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSliderSync : MonoBehaviour
{
    public enum VolumeType { Master, Music, SFX }
    [SerializeField] private VolumeType type;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        // 1. ดึงค่าที่เซฟไว้มาใส่ Slider
        LoadSliderValue();

        // 2. สั่งให้ Slider ฟังคำสั่งเมื่อมีการเลื่อน (Add Listener ผ่าน Code)
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void LoadSliderValue()
    {
        string key = type.ToString() + "Vol"; // สร้าง Key เช่น "MasterVol"
        slider.value = PlayerPrefs.GetFloat(key, 0.75f);
    }

    private void OnSliderValueChanged(float value)
    {
        // 3. เรียกใช้ Singleton โดยไม่ต้องลากวางใน Inspector
        if (SoundManager.Instance != null)
        {
            switch (type)
            {
                case VolumeType.Master:
                    SoundManager.Instance.SetMasterVolume(value);
                    break;
                case VolumeType.Music:
                    SoundManager.Instance.SetMusicVolume(value);
                    break;
                case VolumeType.SFX:
                    SoundManager.Instance.SetSFXVolume(value);
                    break;
            }
        }
    }
}