using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const int BaseInventorySlots = 3;
    public const int MaxInventorySlots = 6;

    private const string InventoryUnlockedSlotsKey = "GameManager.InventoryUnlockedSlots";

    public int Star;

    [SerializeField] private int inventoryUnlockedSlots = BaseInventorySlots;
    [SerializeField] private int defaultInventoryUpgradeCost = 1;
    [SerializeField] private int[] inventoryUpgradeCosts = new[] { 1, 2, 3 };

    public int InventoryUnlockedSlots => inventoryUnlockedSlots;
    public bool CanUpgradeInventory => inventoryUnlockedSlots < MaxInventorySlots;

    public event Action OnStarChanged;
    public event Action OnInventoryUpgradeChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPersistentData();
    }

    private void LoadPersistentData()
    {
        int loadedSlots = PlayerPrefs.GetInt(InventoryUnlockedSlotsKey, BaseInventorySlots);
        inventoryUnlockedSlots = Mathf.Clamp(loadedSlots, BaseInventorySlots, MaxInventorySlots);
    }

    private void SaveInventoryUpgrade()
    {
        PlayerPrefs.SetInt(InventoryUnlockedSlotsKey, inventoryUnlockedSlots);
        PlayerPrefs.Save();
    }

    public void AddStar(int amount)
    {
        if (amount <= 0) return;

        Star += amount;
        OnStarChanged?.Invoke();
    }

    public void RemoveStar(int amount)
    {
        if (amount <= 0) return;

        Star = Mathf.Max(0, Star - amount);
        OnStarChanged?.Invoke();
    }

    public int GetInventoryUpgradeCost()
    {
        if (!CanUpgradeInventory) return 0;

        int upgradeTier = inventoryUnlockedSlots - BaseInventorySlots;
        if (inventoryUpgradeCosts != null && upgradeTier >= 0 && upgradeTier < inventoryUpgradeCosts.Length)
        {
            return Mathf.Max(0, inventoryUpgradeCosts[upgradeTier]);
        }

        return Mathf.Max(0, defaultInventoryUpgradeCost);
    }

    public bool TryUpgradeInventoryWithStar()
    {
        if (!CanUpgradeInventory) return false;

        int upgradeCost = GetInventoryUpgradeCost();
        if (Star < upgradeCost) return false;

        RemoveStar(upgradeCost);
        inventoryUnlockedSlots = Mathf.Clamp(inventoryUnlockedSlots + 1, BaseInventorySlots, MaxInventorySlots);
        SaveInventoryUpgrade();
        OnInventoryUpgradeChanged?.Invoke();
        return true;
    }
}

