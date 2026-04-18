using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Level Buttons")]
    public List<Button> levelButtons = new List<Button>();

    public List<GameObject> hideOnStartObj;

    [Header("Upgrade Hotel")]
    public Button upgradeHotel;
    public GameObject upgradeHotelPanel;

    public Button upgradeInventoryButton;
    public Button upgradePlayerSpeedButton;
    public Button upgradeElevatorSpeedButton;
    public Button upgradeRoomButton;

    [Header("Upgrade Labels")]
    public TMP_Text starText;
    public TMP_Text inventoryButtonText;
    public TMP_Text playerSpeedButtonText;
    public TMP_Text elevatorSpeedButtonText;
    public TMP_Text roomButtonText;

    [Header("Upgrade Progress Fill")]
    public Image inventoryProgressFill;
    public Image playerSpeedProgressFill;
    public Image elevatorSpeedProgressFill;
    public Image roomProgressFill;
    [Header("TitleScene")]
    public Button storyBtn;
    public string titleSceneName = "Title";
    [Header("System")]
    public Button quitGameButton;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStarChanged += RefreshUpgradeUI;
            GameManager.Instance.OnUpgradeDataChanged += RefreshUpgradeUI;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStarChanged -= RefreshUpgradeUI;
            GameManager.Instance.OnUpgradeDataChanged -= RefreshUpgradeUI;
        }
    }

    private void Start()
    {
        SetupLevelButtons();
        SetUpStart();
        SetupUpgradeButtons();
        RefreshUpgradeUI();
        if(storyBtn != null)
        {
            storyBtn.onClick.RemoveAllListeners();
            storyBtn.onClick.AddListener(LoadStoryScene);
        }
        if (quitGameButton != null)
        {
            quitGameButton.onClick.RemoveAllListeners();
            quitGameButton.onClick.AddListener(QuitGame);
        }
    }

    // Setup à¸›à¸¸à¹ˆà¸¡à¹ƒà¸«à¹‰à¹‚à¸«à¸¥à¸” Scene à¸­à¸±à¸•à¹‚à¸™à¸¡à¸±à¸•à¸´
    public void SetUpStart()
    {
        foreach (GameObject gameObject in hideOnStartObj)
        {
            if (gameObject == null) continue;
            gameObject.SetActive(false);
        }

        if (upgradeHotelPanel != null)
        {
            upgradeHotelPanel.SetActive(false);
        }
    }

    public void SetupLevelButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            Button btn = levelButtons[i];
            if (btn == null) continue;

            string sceneName = btn.name;

            if (i == 0 || PlayerPrefs.GetInt("UnlockedLevel_" + i, 0) == 1)
            {
                btn.interactable = true;
            }
            else
            {
                btn.interactable = false;
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                LoadScene(sceneName);
            });
        }
    }

    private void SetupUpgradeButtons()
    {
        if (upgradeHotel != null)
        {
            upgradeHotel.onClick.RemoveAllListeners();
            upgradeHotel.onClick.AddListener(ToggleUpgradeHotelPanel);
        }

        if (upgradeInventoryButton != null)
        {
            upgradeInventoryButton.onClick.RemoveAllListeners();
            upgradeInventoryButton.onClick.AddListener(UpgradeInventory);
        }

        if (upgradePlayerSpeedButton != null)
        {
            upgradePlayerSpeedButton.onClick.RemoveAllListeners();
            upgradePlayerSpeedButton.onClick.AddListener(UpgradePlayerSpeed);
        }

        if (upgradeElevatorSpeedButton != null)
        {
            upgradeElevatorSpeedButton.onClick.RemoveAllListeners();
            upgradeElevatorSpeedButton.onClick.AddListener(UpgradeElevatorSpeed);
        }

        if (upgradeRoomButton != null)
        {
            upgradeRoomButton.onClick.RemoveAllListeners();
            upgradeRoomButton.onClick.AddListener(UpgradeNextRoom);
        }
        
    }
    public void LoadStoryScene()
    {
        SceneManager.LoadScene(titleSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void ToggleUpgradeHotelPanel()
    {
        if (upgradeHotelPanel == null) return;

        upgradeHotelPanel.SetActive(!upgradeHotelPanel.activeSelf);
        RefreshUpgradeUI();
    }

    private void UpgradeInventory()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.TryUpgradeInventoryWithStar();
        RefreshUpgradeUI();
    }

    private void UpgradePlayerSpeed()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.TryUpgradePlayerSpeedWithStar();
        RefreshUpgradeUI();
    }

    private void UpgradeElevatorSpeed()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.TryUpgradeElevatorSpeedWithStar();
        RefreshUpgradeUI();
    }

    private void UpgradeNextRoom()
    {
        if (GameManager.Instance == null) return;

        int roomIndex = GameManager.Instance.GetCurrentUpgradeableRoomIndex();
        if (roomIndex < 0) return;

        GameManager.Instance.TryUpgradeRoomWithStar(roomIndex);
        RefreshUpgradeUI();
    }

    private void RefreshUpgradeUI()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        if (starText != null)
        {
            starText.text = gameManager.Star.ToString();
        }

        RefreshInventoryUpgradeButton(gameManager);
        RefreshPlayerSpeedUpgradeButton(gameManager);
        RefreshElevatorUpgradeButton(gameManager);
        RefreshRoomUpgradeButton(gameManager);
    }

    private void RefreshInventoryUpgradeButton(GameManager gameManager)
    {
        if (upgradeInventoryButton != null)
        {
            bool canUpgrade = gameManager.CanUpgradeInventory;
            int cost = gameManager.GetInventoryUpgradeCost();
            upgradeInventoryButton.interactable = canUpgrade && gameManager.Star >= cost;
        }

        if (inventoryButtonText != null)
        {
            if (!gameManager.CanUpgradeInventory)
            {
                inventoryButtonText.text = "Inventory MAX";
            }
            else
            {
                int cost = gameManager.GetInventoryUpgradeCost();
                inventoryButtonText.text = $"Inventory ({gameManager.InventoryUnlockedSlots}/6) - {cost} Star";
            }
        }

        SetProgressFill(inventoryProgressFill, gameManager.GetInventoryProgress01());
    }

    private void RefreshPlayerSpeedUpgradeButton(GameManager gameManager)
    {
        if (upgradePlayerSpeedButton != null)
        {
            bool canUpgrade = gameManager.CanUpgradePlayerSpeed;
            int cost = gameManager.GetPlayerSpeedUpgradeCost();
            upgradePlayerSpeedButton.interactable = canUpgrade && gameManager.Star >= cost;
        }

        if (playerSpeedButtonText != null)
        {
            if (!gameManager.CanUpgradePlayerSpeed)
            {
                playerSpeedButtonText.text = "PlayerSpeed MAX";
            }
            else
            {
                int cost = gameManager.GetPlayerSpeedUpgradeCost();
                playerSpeedButtonText.text = $"PlayerSpeed Lv.{gameManager.PlayerSpeedUpgradeLevel} - {cost} Star";
            }
        }

        SetProgressFill(playerSpeedProgressFill, gameManager.GetPlayerSpeedProgress01());
    }

    private void RefreshElevatorUpgradeButton(GameManager gameManager)
    {
        if (upgradeElevatorSpeedButton != null)
        {
            bool canUpgrade = gameManager.CanUpgradeElevatorSpeed;
            int cost = gameManager.GetElevatorSpeedUpgradeCost();
            upgradeElevatorSpeedButton.interactable = canUpgrade && gameManager.Star >= cost;
        }

        if (elevatorSpeedButtonText != null)
        {
            if (!gameManager.CanUpgradeElevatorSpeed)
            {
                elevatorSpeedButtonText.text = "ElevatorSpeed MAX";
            }
            else
            {
                int cost = gameManager.GetElevatorSpeedUpgradeCost();
                elevatorSpeedButtonText.text = $"ElevatorSpeed Lv.{gameManager.ElevatorSpeedUpgradeLevel} - {cost} Star";
            }
        }

        SetProgressFill(elevatorSpeedProgressFill, gameManager.GetElevatorSpeedProgress01());
    }

    private void RefreshRoomUpgradeButton(GameManager gameManager)
    {
        int roomIndex = gameManager.GetCurrentUpgradeableRoomIndex();

        if (upgradeRoomButton != null)
        {
            if (roomIndex < 0)
            {
                upgradeRoomButton.interactable = false;
            }
            else
            {
                int cost = gameManager.GetRoomUpgradeCost(roomIndex);
                upgradeRoomButton.interactable = gameManager.Star >= cost;
            }
        }

        if (roomButtonText != null)
        {
            if (roomIndex < 0)
            {
                roomButtonText.text = "Room MAX";
                SetProgressFill(roomProgressFill, 1f);
                return;
            }

            int roomTier = gameManager.GetRoomUpgradeLevel(roomIndex);
            int cost = gameManager.GetRoomUpgradeCost(roomIndex);
            roomButtonText.text = $"Room {roomIndex + 1} Lv.{roomTier}/{gameManager.MaxRoomUpgradeTier} - {cost} Star";
        }

        SetProgressFill(roomProgressFill, gameManager.GetRoomProgress01());
    }

    private void SetProgressFill(Image fillImage, float progress01)
    {
        if (fillImage == null) return;

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = Mathf.Clamp01(progress01);
        fillImage.color = new Color(1f, 0.86f, 0.20f, 1f);
    }

    public void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene not found " + sceneName);
        }
    }

    public void UpdateGoldText(int goldAmount)
    {
    }
}

