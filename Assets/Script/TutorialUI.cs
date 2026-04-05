using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public List<GameObject> pages;
    private int currentIndex = 0;
    public Button backButton;
    public TextMeshProUGUI lvText;

    private LevelManager levelManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdatePages();
        levelManager = FindAnyObjectByType<LevelManager>();
        UpdateLeveltext(levelManager.lv);
        Time.timeScale = 0f;

    }

    public void UpdateLeveltext(int amount)
    {
        lvText.text = "Level " + amount.ToString();
    }

    public void NextPage()
    {
        currentIndex++;

        // เช็คว่าถ้า Index เกินจำนวนหน้าที่มี (หน้า 4 คือ Index 3)
        if (currentIndex >= pages.Count)
        {
            CloseUI();
        }
        else
        {
            if (currentIndex >= 1)
            {
                backButton.gameObject.SetActive(true);
            }
            UpdatePages();
        }
    }
    public void BackPage()
    {
        currentIndex--;

        // เช็คว่าถ้า Index เกินจำนวนหน้าที่มี (หน้า 4 คือ Index 3)
        UpdatePages();
        if (currentIndex <= 0)
        {
            backButton.gameObject.SetActive(false);
        }
    }

    void UpdatePages()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            // ถ้า i ตรงกับ Index ปัจจุบันให้เปิด (true) นอกนั้นปิด (false)
            pages[i].SetActive(i == currentIndex);
        }
    }

    void CloseUI()
    {
        // คำสั่งปิดตัวเอง (ปิด Object ที่ Script นี้เกาะอยู่ หรือปิดทั้ง Canvas)
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        // หรือถ้าอยากทำลาย Object ทิ้งไปเลยใช้:
        // Destroy(gameObject);
    }
}
