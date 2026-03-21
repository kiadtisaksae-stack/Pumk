using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LevelUI : MonoBehaviour
{
    public TextMeshProUGUI lvText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI comboText;

    public Slider priceSlider;

    [Header("Force End Pop Up")]
    public Transform forceback;
    public Button forceBackHome;
    public Button yes;
    public Button no;

    [Header("End Level Pop Up")]
    public Transform endPopUp;
    public Transform winHeader;
    public Transform failHeader;
    public Button continueButton;

    [Header("End Level Pop Up Text")]
    public TextMeshProUGUI winLevelText;
    public TextMeshProUGUI failLevelText;

    public TextMeshProUGUI guestServedText;
    public TextMeshProUGUI bestComboText;
    public TextMeshProUGUI notServedText;
    public TextMeshProUGUI totalScore;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI expertGoalText;
    public Button goToHome;

    [Header("NotifyUI")]
    public TextMeshProUGUI notifyText;
    public float notifyLife = 2.5f;
    private Coroutine notifyCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCombo(0);
        notifyText.text = "";
        forceBackHome.onClick.AddListener(() =>
        {
            forceback.gameObject.SetActive(true);
            Time.timeScale = 0f;
        });
        yes.onClick.AddListener(() =>
        {
            ClickGoToHome();
        });
        no.onClick.AddListener(() =>
        {
            forceback.gameObject.SetActive(false);
            Time.timeScale = 1f;
        });
        goToHome.onClick.AddListener(ClickGoToHome);
        forceback.gameObject.SetActive(false);
        endPopUp.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateLevel(int amount)
    {
        // อัปเดตข้อความเงินใน UI
        lvText.text = "LV." + amount.ToString();
        winLevelText.text = "Level " + amount.ToString();
        failLevelText.text = "Level " + amount.ToString();
    }

    public void UpdateMoney(int amount)
    {
        // อัปเดตข้อความเงินใน UI
        moneyText.text = "Money: $" + amount.ToString();
    }
    public void UpdateCombo(int points)
    {
        // อัปเดตข้อความคะแนนใน UI
        comboText.text = "Combo: " + points.ToString();
    }

    public void UpdateProgressBar(int currentPrice , int targetPrice)
    {
        priceSlider.maxValue = targetPrice;
        priceSlider.value = currentPrice;   
    }


    public void ShowEndLevelScreen(bool isWin, int guestServed, int combo, int notServed,  int score , int goal , int exGoal)
    {
        // สลับหัวข้อตามสถานะ
        winHeader.gameObject.SetActive(isWin);
        continueButton.gameObject.SetActive(isWin);
        failHeader.gameObject.SetActive(!isWin);


        // ใส่ข้อมูลลงใน Text
        guestServedText.text = "Guest Served : " + guestServed.ToString();
        bestComboText.text = "Best Combo : " + combo;
        notServedText.text = "Customers not served : " + notServed;
        totalScore.text = "Total Score : " + score.ToString();
        goalText.text = "Goal : " + goal.ToString();
        expertGoalText.text = "Expert goal : " + exGoal.ToString();


        endPopUp.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }


    public void DisplayTime(bool isUnLimit ,float timeToDisplay)
    {
        if (isUnLimit)
        {
            timeText.text = "-----";
            return;
        }

        // ป้องกันเลขติดลบตอนแสดงผล
        if (timeToDisplay < 0) timeToDisplay = 0;

        // คำนวณ นาที และ วินาที
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // แสดงผลในรูปแบบ 00:00
        timeText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public void ClicktoRetry()
    {
        Time.timeScale = 1f;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
    }
    public void ClicktoNextLevel()
    {
        Time.timeScale = 1f;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void ClickGoToHome()
    {
        // โหลดฉากโฮม
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameHub");
        
    }
    public void Notify(string message)
    {
 
        if (notifyCoroutine != null)
        {
            StopCoroutine(notifyCoroutine);
        }

        notifyText.text = message;
        notifyCoroutine = StartCoroutine(ClearNotify());
    }

    IEnumerator ClearNotify()
    {
        yield return new WaitForSecondsRealtime(notifyLife);

        notifyText.text = "";
        notifyCoroutine = null;
    }
}


