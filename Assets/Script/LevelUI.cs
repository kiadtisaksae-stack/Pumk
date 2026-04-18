using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    public TextMeshProUGUI lvText;
    public TextMeshProUGUI timeText;
    public RectTransform clockHand;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI _starText;

    public Slider priceSlider;

    [Header("settingLevel")]
    public Transform settingLevelPanel;
    public Button settingButton;
    public Button continueLevelButton;
    public Button backToMenuButton;

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

    private void Start()
    {
        UpdateCombo(0);
        if (notifyText != null) notifyText.text = "";

        if (settingButton != null) settingButton.onClick.AddListener(OpenSettingLevel);
        if (continueLevelButton != null) continueLevelButton.onClick.AddListener(CloseSettingLevel);
        if (backToMenuButton != null) backToMenuButton.onClick.AddListener(BackToMainMenuFromSettingLevel);
        if (goToHome != null) goToHome.onClick.AddListener(ClickGoToHome);

        if (settingLevelPanel != null) settingLevelPanel.gameObject.SetActive(false);
        if (endPopUp != null) endPopUp.gameObject.SetActive(false);

        UpdateStarUI();
    }

    public void UpdateStarUI()
    {
        if (_starText != null && GameManager.Instance != null)
        {
            _starText.text = "  " + GameManager.Instance.Star;
        }
    }

    public void UpdateLevel(int amount)
    {
        if (lvText != null) lvText.text = "LV." + amount;
        if (winLevelText != null) winLevelText.text = "Level " + amount;
        if (failLevelText != null) failLevelText.text = "Level " + amount;
    }

    public void UpdateMoney(int amount)
    {
        if (moneyText != null) moneyText.text = "Money: $" + amount;
    }

    public void UpdateCombo(int points)
    {
        if (comboText != null) comboText.text = "Combo: " + points;
    }

    public void UpdateProgressBar(int currentPrice, int targetPrice)
    {
        if (priceSlider == null) return;

        priceSlider.maxValue = targetPrice;
        priceSlider.value = currentPrice;
    }

    public void ShowEndLevelScreen(bool isWin, int guestServed, int combo, int notServed, int score, int goal, int comboBonusScore)
    {
        if (winHeader != null) winHeader.gameObject.SetActive(isWin);
        if (continueButton != null) continueButton.gameObject.SetActive(isWin);
        if (failHeader != null) failHeader.gameObject.SetActive(!isWin);

        if (guestServedText != null) guestServedText.text = "Guest Served : " + guestServed;
        if (bestComboText != null) bestComboText.text = "Best Combo : " + combo + " = +" + comboBonusScore + " score";
        if (notServedText != null) notServedText.text = "Customers not served : " + notServed;
        if (totalScore != null) totalScore.text = "Total Score : " + score;
        if (goalText != null) goalText.text = "Goal : " + goal;
        if (expertGoalText != null) expertGoalText.text = "Combo Bonus : +" + comboBonusScore;

        if (endPopUp != null) endPopUp.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void DisplayTime(bool isUnLimit, float timeToDisplay, float maxTime)
    {
        if (isUnLimit)
        {
            if (timeText != null) timeText.text = "-----";
            return;
        }

        if (timeToDisplay < 0) timeToDisplay = 0;

        if (clockHand != null && maxTime > 0)
        {
            float timeRatio = 1f - (timeToDisplay / maxTime);
            float rotationZ = -360f * timeRatio;
            clockHand.localRotation = Quaternion.Euler(0, 0, rotationZ);
        }

        if (timeText != null)
        {
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timeText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        }
    }

    public void ClicktoRetry()
    {
        Time.timeScale = 1f;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void ClicktoNextLevel()
    {
        Time.timeScale = 1f;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void ClickGoToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettingLevel()
    {
        if (settingLevelPanel != null) settingLevelPanel.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettingLevel()
    {
        if (settingLevelPanel != null) settingLevelPanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void BackToMainMenuFromSettingLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Notify(string message)
    {
        if (notifyText == null) return;

        if (notifyCoroutine != null)
        {
            StopCoroutine(notifyCoroutine);
        }

        notifyText.text = message;
        notifyCoroutine = StartCoroutine(ClearNotify());
    }

    private IEnumerator ClearNotify()
    {
        yield return new WaitForSecondsRealtime(notifyLife);

        if (notifyText != null) notifyText.text = "";
        notifyCoroutine = null;
    }
}
