using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const int BaseInventorySlots = 3;
    public const int MaxInventorySlots = 6;
    public const int TotalUpgradeableRooms = 7;

    private const string InventoryUnlockedSlotsKey = "GameManager.InventoryUnlockedSlots";
    private const string StarKey = "GameManager.Star";
    private const string PlayerSpeedUpgradeLevelKey = "GameManager.PlayerSpeedUpgradeLevel";
    private const string ElevatorSpeedUpgradeLevelKey = "GameManager.ElevatorSpeedUpgradeLevel";
    private const string RoomUpgradeLevelKeyPrefix = "GameManager.RoomUpgradeLevel_";

    public int Star;

    [Header("Inventory Upgrade")]
    [SerializeField] private int inventoryUnlockedSlots = BaseInventorySlots;
    [SerializeField] private int defaultInventoryUpgradeCost = 1;
    [SerializeField] private int[] inventoryUpgradeCosts = new[] { 1, 2, 3 };

    [Header("Player Speed Upgrade")]
    [SerializeField] private int playerSpeedUpgradeLevel;
    [SerializeField] private int defaultPlayerSpeedUpgradeCost = 2;
    [SerializeField] private int[] playerSpeedUpgradeCosts = new[] { 2, 3, 4 };
    [SerializeField] private float[] playerSpeedMultipliersByLevel = new[] { 1f, 1.15f, 1.3f, 1.45f };

    [Header("Elevator Speed Upgrade")]
    [SerializeField] private int elevatorSpeedUpgradeLevel;
    [Tooltip("Max elevator upgrade level (Lv0 is base). Example: 3 means Lv0 -> Lv3.")]
    [Min(0)]
    [SerializeField] private int elevatorMaxUpgradeLevel = 3;
    [SerializeField] private int defaultElevatorSpeedUpgradeCost = 2;
    [SerializeField] private int[] elevatorSpeedUpgradeCosts = new[] { 2, 3, 4 };
    [SerializeField] private float[] elevatorSpeedMultipliersByLevel = new[] { 1f, 1.2f, 1.35f, 1.5f };

    [Header("Room Upgrade")]
    [SerializeField] private int maxRoomUpgradeTier = 2;
    [SerializeField] private int defaultRoomUpgradeCost = 2;
    [SerializeField] private int[] roomUpgradeCostsByTier = new[] { 2, 3 };
    [SerializeField] private int[] roomUpgradeTiers = new int[TotalUpgradeableRooms];

    public int InventoryUnlockedSlots => inventoryUnlockedSlots;
    public int PlayerSpeedUpgradeLevel => playerSpeedUpgradeLevel;
    public int ElevatorSpeedUpgradeLevel => elevatorSpeedUpgradeLevel;
    public int MaxRoomUpgradeTier => Mathf.Max(1, maxRoomUpgradeTier);

    public bool CanUpgradeInventory => inventoryUnlockedSlots < MaxInventorySlots;
    public bool CanUpgradePlayerSpeed => playerSpeedUpgradeLevel < GetMaxPlayerSpeedUpgradeLevel();
    public bool CanUpgradeElevatorSpeed => elevatorSpeedUpgradeLevel < GetMaxElevatorSpeedUpgradeLevel();

    public event Action OnStarChanged;
    public event Action OnUpgradeDataChanged;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyUpgradesToCurrentScene();
    }

    private void LoadPersistentData()
    {
        // Star: if no save exists yet, keep Inspector value as first-run start value and save it once.
        if (PlayerPrefs.HasKey(StarKey))
        {
            Star = Mathf.Max(0, PlayerPrefs.GetInt(StarKey, 0));
        }
        else
        {
            Star = Mathf.Max(0, Star);
            PlayerPrefs.SetInt(StarKey, Star);
            PlayerPrefs.Save();
        }

        int loadedSlots = PlayerPrefs.GetInt(InventoryUnlockedSlotsKey, BaseInventorySlots);
        inventoryUnlockedSlots = Mathf.Clamp(loadedSlots, BaseInventorySlots, MaxInventorySlots);

        playerSpeedUpgradeLevel = Mathf.Clamp(PlayerPrefs.GetInt(PlayerSpeedUpgradeLevelKey, 0), 0, GetMaxPlayerSpeedUpgradeLevel());
        elevatorSpeedUpgradeLevel = Mathf.Clamp(PlayerPrefs.GetInt(ElevatorSpeedUpgradeLevelKey, 0), 0, GetMaxElevatorSpeedUpgradeLevel());

        EnsureRoomUpgradeArray();
        for (int i = 0; i < roomUpgradeTiers.Length; i++)
        {
            int loadedTier = PlayerPrefs.GetInt(RoomUpgradeLevelKeyPrefix + i, 0);
            roomUpgradeTiers[i] = Mathf.Clamp(loadedTier, 0, Mathf.Max(0, maxRoomUpgradeTier));
        }
    }

    private void SaveUpgradeData()
    {
        PlayerPrefs.SetInt(InventoryUnlockedSlotsKey, inventoryUnlockedSlots);
        PlayerPrefs.SetInt(StarKey, Mathf.Max(0, Star));
        PlayerPrefs.SetInt(PlayerSpeedUpgradeLevelKey, playerSpeedUpgradeLevel);
        PlayerPrefs.SetInt(ElevatorSpeedUpgradeLevelKey, elevatorSpeedUpgradeLevel);

        EnsureRoomUpgradeArray();
        for (int i = 0; i < roomUpgradeTiers.Length; i++)
        {
            PlayerPrefs.SetInt(RoomUpgradeLevelKeyPrefix + i, roomUpgradeTiers[i]);
        }

        PlayerPrefs.Save();
    }

    private void EnsureRoomUpgradeArray()
    {
        if (roomUpgradeTiers == null || roomUpgradeTiers.Length != TotalUpgradeableRooms)
        {
            int[] newArray = new int[TotalUpgradeableRooms];
            if (roomUpgradeTiers != null)
            {
                int copyLength = Mathf.Min(roomUpgradeTiers.Length, newArray.Length);
                Array.Copy(roomUpgradeTiers, newArray, copyLength);
            }

            roomUpgradeTiers = newArray;
        }
    }

    public void AddStar(int amount)
    {
        if (amount <= 0) return;

        Star += amount;
        PlayerPrefs.SetInt(StarKey, Mathf.Max(0, Star));
        PlayerPrefs.Save();
        OnStarChanged?.Invoke();
    }

    public void RemoveStar(int amount)
    {
        if (amount <= 0) return;

        Star = Mathf.Max(0, Star - amount);
        PlayerPrefs.SetInt(StarKey, Star);
        PlayerPrefs.Save();
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
        SaveUpgradeData();
        ApplyUpgradesToCurrentScene();
        OnUpgradeDataChanged?.Invoke();
        return true;
    }

    public int GetPlayerSpeedUpgradeCost()
    {
        if (!CanUpgradePlayerSpeed) return 0;

        return GetUpgradeCost(playerSpeedUpgradeCosts, playerSpeedUpgradeLevel, defaultPlayerSpeedUpgradeCost);
    }

    public bool TryUpgradePlayerSpeedWithStar()
    {
        if (!CanUpgradePlayerSpeed) return false;

        int cost = GetPlayerSpeedUpgradeCost();
        if (Star < cost) return false;

        RemoveStar(cost);
        playerSpeedUpgradeLevel = Mathf.Clamp(playerSpeedUpgradeLevel + 1, 0, GetMaxPlayerSpeedUpgradeLevel());
        SaveUpgradeData();
        ApplyUpgradesToCurrentScene();
        OnUpgradeDataChanged?.Invoke();
        return true;
    }

    public int GetElevatorSpeedUpgradeCost()
    {
        if (!CanUpgradeElevatorSpeed) return 0;

        return GetUpgradeCost(elevatorSpeedUpgradeCosts, elevatorSpeedUpgradeLevel, defaultElevatorSpeedUpgradeCost);
    }

    public bool TryUpgradeElevatorSpeedWithStar()
    {
        if (!CanUpgradeElevatorSpeed) return false;

        int cost = GetElevatorSpeedUpgradeCost();
        if (Star < cost) return false;

        RemoveStar(cost);
        elevatorSpeedUpgradeLevel = Mathf.Clamp(elevatorSpeedUpgradeLevel + 1, 0, GetMaxElevatorSpeedUpgradeLevel());
        SaveUpgradeData();
        ApplyUpgradesToCurrentScene();
        OnUpgradeDataChanged?.Invoke();
        return true;
    }

    public int GetRoomUpgradeLevel(int roomIndex)
    {
        EnsureRoomUpgradeArray();
        if (roomIndex < 0 || roomIndex >= roomUpgradeTiers.Length) return 0;
        return Mathf.Clamp(roomUpgradeTiers[roomIndex], 0, Mathf.Max(0, maxRoomUpgradeTier));
    }

    public int GetCurrentUpgradeableRoomIndex()
    {
        EnsureRoomUpgradeArray();

        for (int i = 0; i < roomUpgradeTiers.Length; i++)
        {
            if (roomUpgradeTiers[i] >= maxRoomUpgradeTier) continue;
            if (i > 0 && roomUpgradeTiers[i - 1] < maxRoomUpgradeTier) return -1;
            return i;
        }

        return -1;
    }

    public bool AreAllRoomsFullyUpgraded()
    {
        EnsureRoomUpgradeArray();

        for (int i = 0; i < roomUpgradeTiers.Length; i++)
        {
            if (roomUpgradeTiers[i] < maxRoomUpgradeTier) return false;
        }

        return true;
    }

    public int GetRoomUpgradeCost(int roomIndex)
    {
        int level = GetRoomUpgradeLevel(roomIndex);
        if (level >= maxRoomUpgradeTier) return 0;

        return GetUpgradeCost(roomUpgradeCostsByTier, level, defaultRoomUpgradeCost);
    }

    public bool CanUpgradeRoom(int roomIndex)
    {
        EnsureRoomUpgradeArray();
        if (roomIndex < 0 || roomIndex >= roomUpgradeTiers.Length) return false;

        if (roomUpgradeTiers[roomIndex] >= maxRoomUpgradeTier) return false;
        if (roomIndex > 0 && roomUpgradeTiers[roomIndex - 1] < maxRoomUpgradeTier) return false;

        return true;
    }

    public bool TryUpgradeRoomWithStar(int roomIndex)
    {
        if (!CanUpgradeRoom(roomIndex)) return false;

        int cost = GetRoomUpgradeCost(roomIndex);
        if (Star < cost) return false;

        RemoveStar(cost);
        roomUpgradeTiers[roomIndex] = Mathf.Clamp(roomUpgradeTiers[roomIndex] + 1, 0, maxRoomUpgradeTier);
        SaveUpgradeData();
        ApplyUpgradesToCurrentScene();
        OnUpgradeDataChanged?.Invoke();
        return true;
    }

    public float GetPlayerSpeedMultiplier()
    {
        return GetUpgradeValue(playerSpeedMultipliersByLevel, playerSpeedUpgradeLevel, 1f);
    }

    public float GetElevatorSpeedMultiplier()
    {
        return GetUpgradeValue(elevatorSpeedMultipliersByLevel, elevatorSpeedUpgradeLevel, 1f);
    }

    public int GetPlayerSpeedMaxLevel()
    {
        return GetMaxPlayerSpeedUpgradeLevel();
    }

    public int GetElevatorSpeedMaxLevel()
    {
        return GetMaxElevatorSpeedUpgradeLevel();
    }

    public float GetInventoryProgress01()
    {
        float denom = Mathf.Max(1, MaxInventorySlots - BaseInventorySlots);
        return Mathf.Clamp01((inventoryUnlockedSlots - BaseInventorySlots) / denom);
    }

    public float GetPlayerSpeedProgress01()
    {
        int max = Mathf.Max(1, GetMaxPlayerSpeedUpgradeLevel());
        return Mathf.Clamp01((float)playerSpeedUpgradeLevel / max);
    }

    public float GetElevatorSpeedProgress01()
    {
        int max = Mathf.Max(1, GetMaxElevatorSpeedUpgradeLevel());
        return Mathf.Clamp01((float)elevatorSpeedUpgradeLevel / max);
    }

    public float GetRoomProgress01()
    {
        int roomIndex = GetCurrentUpgradeableRoomIndex();
        if (roomIndex < 0) return 1f;

        int level = GetRoomUpgradeLevel(roomIndex);
        int max = Mathf.Max(1, maxRoomUpgradeTier);
        return Mathf.Clamp01((float)level / max);
    }

    public void ApplyUpgradesToCurrentScene()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float playerSpeedMultiplier = GetPlayerSpeedMultiplier();

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;
            players[i].ApplyGlobalUpgradesFromGameManager(this, playerSpeedMultiplier);
        }

        float elevatorSpeedMultiplier = GetElevatorSpeedMultiplier();
        ElevatorManager elevatorManager = FindAnyObjectByType<ElevatorManager>();
        if (elevatorManager != null)
        {
            elevatorManager.ApplyElevatorSpeedUpgrade(elevatorSpeedMultiplier);
        }
        else
        {
            ElevatorController[] elevators = FindObjectsByType<ElevatorController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < elevators.Length; i++)
            {
                if (elevators[i] == null) continue;
                elevators[i].ApplySpeedMultiplier(elevatorSpeedMultiplier);
            }
        }

        RoomManager roomManager = FindAnyObjectByType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.ApplyPersistentRoomUpgrades(this);
        }
        else
        {
            Room[] rooms = FindObjectsByType<Room>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < rooms.Length; i++)
            {
                Room room = rooms[i];
                if (room == null || room.RoomData == null) continue;

                int roomId = room.RoomData.RoomID;
                int roomTier = GetRoomUpgradeLevel(roomId);
                room.ApplyPersistentRoomLevel(roomTier);
            }
        }
    }

    private int GetMaxPlayerSpeedUpgradeLevel()
    {
        if (playerSpeedMultipliersByLevel == null || playerSpeedMultipliersByLevel.Length == 0) return 0;
        return playerSpeedMultipliersByLevel.Length - 1;
    }

    private int GetMaxElevatorSpeedUpgradeLevel()
    {
        int configuredMax = Mathf.Max(0, elevatorMaxUpgradeLevel);
        if (elevatorSpeedMultipliersByLevel == null || elevatorSpeedMultipliersByLevel.Length == 0)
        {
            return configuredMax;
        }

        // Clamp by available multiplier data to avoid "upgrade level increases but speed does not change".
        int multiplierBasedMax = elevatorSpeedMultipliersByLevel.Length - 1;
        return Mathf.Min(configuredMax, Mathf.Max(0, multiplierBasedMax));
    }

    private int GetUpgradeCost(int[] costs, int currentLevel, int defaultCost)
    {
        if (costs != null && currentLevel >= 0 && currentLevel < costs.Length)
        {
            return Mathf.Max(0, costs[currentLevel]);
        }

        return Mathf.Max(0, defaultCost);
    }

    private float GetUpgradeValue(float[] valuesByLevel, int level, float fallback)
    {
        if (valuesByLevel != null && valuesByLevel.Length > 0)
        {
            int safeIndex = Mathf.Clamp(level, 0, valuesByLevel.Length - 1);
            return Mathf.Max(0.01f, valuesByLevel[safeIndex]);
        }

        return Mathf.Max(0.01f, fallback);
    }

}
