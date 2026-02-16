using TMPro;
using UnityEngine;

public enum RewardRank
{
    Bronze = 100,
    Silver = 200,
    Gold = 500
}
public class LevelManager : MonoBehaviour
{
    public int money = 1000;
    public float gameTime = 300f;
    public int currentPoints = 0;
    private RewardRank rank;

    private int gold;
    private bool levelEnded = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPoints = 0;
        levelEnded = false;
    }

    // Update is called once per frame
    void Update()
    {
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
        money += amount;
        LevelUI uiManager = FindAnyObjectByType<LevelUI>();
        uiManager.UpdateMoney(money);
    }
    public void AddServicePoint(int points)
    {
        currentPoints += points;
        LevelUI uiManager = FindAnyObjectByType<LevelUI>();
        uiManager.UpdatePoints(currentPoints);
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
        CulculateRank();
        gold = (int)rank;
        LevelUI levelUI = FindAnyObjectByType<LevelUI>();
        levelUI.ShowEndLevelScreen(rank,gold);

        GameManager.Instance.AddGold(gold);
    }
}
