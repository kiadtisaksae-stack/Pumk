using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum RewardRank
{
    Bronze,
    Silver,
    Gold
}

// 2. สร้าง Class สำหรับจับคู่ Rank กับ จำนวนเงิน
[System.Serializable]
public class RankData
{
    public RewardRank rank;
    public int rewardAmount;
}
public class LevelManager : MonoBehaviour
{
    [Header("Rank Reward Settings")]
    // 3. สร้าง List เพื่อให้ไปแก้ค่าใน Inspector ได้
    public List<RankData> rankRewards = new List<RankData>();
    public int lv;
    public int guestQuitCount;
    public List<ItemSO> inLevelService = new List<ItemSO>();
    [Header("Rank Thresholds")]
    public float RankGoldThreshold = 1.0f; // 100% หรือมากกว่า
    public float RankSilverThreshold = 0.7f; // 70% หรือมากกว่า
    public float RankBronzeThreshold = 0.0f; // ต่ำกว่า 70% จะได้ Bronze

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
    public float gameTimeMinutes = 5f;
    [HideInInspector]public float gameTime = 300f;
    public bool infiniteGameTime;
    private float maxGameTime;

    [Header("Level Unlock Settings")]
    [Tooltip("Index ของปุ่มเลเวลถัดไปใน UIManager (เช่น ด่าน 2 คือ index 1)")]
    public int unlockNextLevelIndex;
    

    private RewardRank rank;

    private int gold;
    private bool levelEnded = false;
    private bool isWin = false;
    private LevelUI uiLevelManager;
    private void OnValidate()
    {
        gameTime = gameTimeMinutes * 60f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameTime = gameTimeMinutes * 60f;
        maxGameTime = gameTime;
        uiLevelManager = FindAnyObjectByType<LevelUI>();
        uiLevelManager.UpdateLevel(lv);
        uiLevelManager.UpdateProgressBar(moneyGetInLevel,requireMoneyToNext);
        levelEnded = false;
        uiLevelManager.UpdateMoney(currentMoney);//ทดสอบเฉยๆ

    }

    // Update is called once per frame
    void Update()
    {
        uiLevelManager.DisplayTime(infiniteGameTime, gameTime, maxGameTime);
        if (infiniteGameTime) return;

        gameTime -= Time.deltaTime;

        if (levelEnded == true)
        {
            return;
        }
        else if (gameTime <= 0)
        {
            gameTime = 0;
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
        float ratio = (float)moneyGetInLevel / requireMoneyToNext;

        if (ratio >= 1.0f)
        {
            rank = RewardRank.Gold;
        }
        else if (ratio >= 0.8f)
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
        if (levelEnded) return;
        levelEnded = true;
        CulculateRank();
        
        // หาจำนวนดาวรางวัลจาก List ที่ตั้งไว้ใน Inspector (ใน Inspector ควรแก้ rewardAmount เป็นจำนวนดาว 1, 2, 3)
        int starsEarned = 0;
        foreach (var rw in rankRewards)
        {
            if (rw.rank == rank)
            {
                starsEarned = rw.rewardAmount;
                break;
            }
        }

        // เช็คว่าชนะหรือแพ้ตาม rank (Gold >= 100%, Silver >= 80%)
        if (rank == RewardRank.Gold || rank == RewardRank.Silver)
        {
            isWin = true;
            PlayerPrefs.SetInt("UnlockedLevel_" + unlockNextLevelIndex, 1);
            PlayerPrefs.Save();

            // มอบดาวสะสมเมื่อเล่นผ่าน (อิงค่า rewardAmount จาก Inspector)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddStar(starsEarned);
            }
        }
        else
        {
            isWin = false;
        }
        uiLevelManager.ShowEndLevelScreen(isWin, guestServed , maxSteakCount , guestNotServed ,(moneyGetInLevel + bonusNet) , moneyGetInLevel, bonusNet);

        //GameManager.Instance.AddGold(currentMoney);
    }
}
