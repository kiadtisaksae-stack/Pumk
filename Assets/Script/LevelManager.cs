using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum RewardRank
{
    Bronze = 100,
    Silver = 200,
    Gold = 500
}
public class LevelManager : MonoBehaviour
{
    public int lv;


    [Header("Currency")]
    public int currentMoney = 0;

    public int requireMoneyToNext;
    public int moneyGetInLevel;

    [Header("Guest count in Level")]
    public int guestServed;
    public int guestNotServed;


    [Header("Bonus Combo")] //bonus สำหรับสีห้องตร
    public int bonusNet;
    public int streakCount = 0;
    public int maxSteakCount;

    [Header("Game Time Settings")]
    public float gameTime = 300f;
    public bool infiniteGameTime;

    public int currentPoints = 0;
    private RewardRank rank;

    private int gold;
    private bool levelEnded = false;
    private bool isWin = false;
    private LevelUI uiLevelManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        uiLevelManager = FindAnyObjectByType<LevelUI>();
        uiLevelManager.UpdateLevel(lv);
        uiLevelManager.UpdateProgressBar(moneyGetInLevel,requireMoneyToNext);
        currentPoints = 0;
        levelEnded = false;
        uiLevelManager.UpdateMoney(currentMoney);//ทดสอบเฉยๆ

    }

    // Update is called once per frame
    void Update()
    {
        uiLevelManager.DisplayTime(infiniteGameTime, gameTime);
        if (infiniteGameTime) return;

        gameTime -= Time.deltaTime;

        if (levelEnded == true)
        {
            return;
        }
        else if (gameTime <= 0)
        {
            EndLevel();
            levelEnded = true;
        }
    }
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        moneyGetInLevel += amount;
        uiLevelManager.UpdateMoney(currentMoney);
        uiLevelManager.UpdateProgressBar(moneyGetInLevel, requireMoneyToNext);
    }
    public void PriceItem(int amount)
    {
        currentMoney -= amount;
        moneyGetInLevel -= amount;
        uiLevelManager.UpdateMoney(currentMoney);
        uiLevelManager.UpdateProgressBar(moneyGetInLevel, requireMoneyToNext);
    }
    public void AddCombo(int streak)
    {
        streakCount += streak;

        if (streakCount > maxSteakCount)
        {
            maxSteakCount = streakCount;
        }

        int bonus = 50 * streakCount;
        bonusNet += bonus;

        currentMoney += bonus;
        uiLevelManager.UpdateMoney(currentMoney);
        uiLevelManager.UpdateCombo(streakCount);
    }

    public void ResetCombo()
    {
        streakCount = 0;
        uiLevelManager.UpdateCombo(streakCount);
    }

    public void CulculateRank()
    {
        if(currentPoints >= 3)
        {
            rank = RewardRank.Gold;
        }
        else if(currentPoints >= 2)
        {
            rank = RewardRank.Silver;
        }
        else
        {
            rank = RewardRank.Bronze;
        }
    }

    public void EndLevel()
    {
        levelEnded = true;
        CulculateRank();
        gold = (int)rank;
        // เช็คว่าชนะหรือแพ้ตามเงื่อนไข
        if (moneyGetInLevel == requireMoneyToNext)
        {
            isWin = true;
        }
        uiLevelManager.ShowEndLevelScreen(isWin, guestServed , maxSteakCount , guestNotServed ,(moneyGetInLevel + bonusNet) , moneyGetInLevel, bonusNet);

        //GameManager.Instance.AddGold(currentMoney);
    }
}
