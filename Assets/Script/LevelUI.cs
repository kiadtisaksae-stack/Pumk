using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class LevelUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI pointsText;
    [Header("Force End Pop Up")]
    public Transform forceback;
    public Button forceBackHome;
    public Button yes;
    public Button no;
    [Header("End Level Pop Up")]
    public Transform endPopUp;
    public TextMeshProUGUI endRankText;
    public TextMeshProUGUI rewardGold;
    public Button goToHome;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        UpdateMoney(0);
        UpdatePoints(0);
        
        forceBackHome.onClick.AddListener(() =>
        {
            forceback.gameObject.SetActive(true);
        });
        yes.onClick.AddListener(() =>
        {
            ClickGoToHome();
        });
        no.onClick.AddListener(() =>
        {
            forceback.gameObject.SetActive(false);
        });
        goToHome.onClick.AddListener(ClickGoToHome);
        forceback.gameObject.SetActive(false);
        endPopUp.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateMoney(int amount)
    {
        // อัปเดตข้อความเงินใน UI
        moneyText.text = "Money: $" + amount.ToString();
    }
    public void UpdatePoints(int points)
    {
        // อัปเดตข้อความคะแนนใน UI
        pointsText.text = "Points: " + points.ToString();
    }

    public void ShowEndLevelScreen(RewardRank rank, int gold)
    {
        endRankText.text = "Rank: " + rank.ToString();
        rewardGold.text = "Reward Gold: " + gold.ToString();
        endPopUp.gameObject.SetActive(true);

    }
    public void ClickGoToHome()
    {
        // โหลดฉากโฮม
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameHub");
    }
}


