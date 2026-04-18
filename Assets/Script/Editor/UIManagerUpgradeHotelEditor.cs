using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[CustomEditor(typeof(UIManager))]
public class UIManagerUpgradeHotelEditor : Editor
{
    private const string RootName = "UpgradeHotelRoot";
    private const string UpgradeButtonName = "UpgradeHotelButton";
    private const string PanelName = "UpgradeHotelPanel";
    private const string StarTextName = "StarText";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10f);
        if (GUILayout.Button("Auto Setup UpgradeHotel UI (Manual Position)", GUILayout.Height(28f)))
        {
            SetupUpgradeHotelUI((UIManager)target);
        }
    }

    [MenuItem("Tools/Upgrade Hotel/Auto Setup From Selected UIManager")]
    private static void AutoSetupFromSelectedUIManager()
    {
        UIManager manager = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<UIManager>()
            : null;

        if (manager == null)
        {
            EditorUtility.DisplayDialog(
                "UpgradeHotel Auto Setup",
                "Please select a GameObject with UIManager component first.",
                "OK");
            return;
        }

        SetupUpgradeHotelUI(manager);
        Selection.activeObject = manager.gameObject;
    }

    [MenuItem("Tools/Upgrade Hotel/Auto Setup From Selected UIManager", true)]
    private static bool ValidateAutoSetupFromSelectedUIManager()
    {
        return Selection.activeGameObject != null &&
               Selection.activeGameObject.GetComponent<UIManager>() != null;
    }

    [MenuItem("GameObject/UI/Upgrade Hotel/Auto Setup On Selected UIManager", false, 10)]
    private static void GameObjectMenuAutoSetup()
    {
        AutoSetupFromSelectedUIManager();
    }

    private static void SetupUpgradeHotelUI(UIManager manager)
    {
        if (manager == null) return;

        Canvas canvas = EnsureCanvas(manager);
        EnsureEventSystem();

        RectTransform root = FindOrCreateRectTransform(canvas.transform, RootName);
        StretchToParent(root);

        Button upgradeHotelButton = FindOrCreateButton(root, UpgradeButtonName, new Vector2(220f, 56f), new Vector2(-150f, -70f), "Upgrade Hotel");
        SetAnchorPreset(upgradeHotelButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));

        GameObject panelObj = FindOrCreateGameObject(root, PanelName);
        RectTransform panelRect = EnsureRectTransform(panelObj);
        SetAnchorPreset(panelRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        panelRect.sizeDelta = new Vector2(520f, 470f);
        panelRect.anchoredPosition = new Vector2(-290f, -300f);

        Image panelImage = panelObj.GetComponent<Image>();
        if (panelImage == null) panelImage = Undo.AddComponent<Image>(panelObj);
        panelImage.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);

        TMP_Text starText = FindOrCreateTMPText(panelRect, StarTextName, new Vector2(460f, 42f), new Vector2(0f, -36f), "Star: 0", 30f, TextAlignmentOptions.Center);
        SetAnchorPreset(starText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

        Button inventoryButton = FindOrCreateButton(panelRect, "UpgradeInventoryButton", new Vector2(440f, 66f), new Vector2(0f, -110f), "Inventory");
        Button playerSpeedButton = FindOrCreateButton(panelRect, "UpgradePlayerSpeedButton", new Vector2(440f, 66f), new Vector2(0f, -190f), "PlayerSpeed");
        Button elevatorSpeedButton = FindOrCreateButton(panelRect, "UpgradeElevatorSpeedButton", new Vector2(440f, 66f), new Vector2(0f, -270f), "ElevatorSpeed");
        Button roomButton = FindOrCreateButton(panelRect, "UpgradeRoomButton", new Vector2(440f, 66f), new Vector2(0f, -350f), "Room 1");

        Image inventoryFill = EnsureButtonVisuals(inventoryButton);
        Image playerFill = EnsureButtonVisuals(playerSpeedButton);
        Image elevatorFill = EnsureButtonVisuals(elevatorSpeedButton);
        Image roomFill = EnsureButtonVisuals(roomButton);

        AssignUIManagerReferences(
            manager,
            upgradeHotelButton,
            panelObj,
            inventoryButton,
            playerSpeedButton,
            elevatorSpeedButton,
            roomButton,
            starText,
            inventoryFill,
            playerFill,
            elevatorFill,
            roomFill
        );

        Undo.RecordObject(manager, "Assign UpgradeHotel UI References");
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

        Debug.Log("Auto setup UpgradeHotel UI complete (manual positions, no grid layout).");
    }

    private static void AssignUIManagerReferences(
        UIManager manager,
        Button upgradeHotelButton,
        GameObject panelObj,
        Button inventoryButton,
        Button playerSpeedButton,
        Button elevatorSpeedButton,
        Button roomButton,
        TMP_Text starText,
        Image inventoryFill,
        Image playerFill,
        Image elevatorFill,
        Image roomFill)
    {
        manager.upgradeHotel = upgradeHotelButton;
        manager.upgradeHotelPanel = panelObj;

        manager.upgradeInventoryButton = inventoryButton;
        manager.upgradePlayerSpeedButton = playerSpeedButton;
        manager.upgradeElevatorSpeedButton = elevatorSpeedButton;
        manager.upgradeRoomButton = roomButton;

        manager.starText = starText;
        manager.inventoryButtonText = GetButtonLabel(inventoryButton);
        manager.playerSpeedButtonText = GetButtonLabel(playerSpeedButton);
        manager.elevatorSpeedButtonText = GetButtonLabel(elevatorSpeedButton);
        manager.roomButtonText = GetButtonLabel(roomButton);

        manager.inventoryProgressFill = inventoryFill;
        manager.playerSpeedProgressFill = playerFill;
        manager.elevatorSpeedProgressFill = elevatorFill;
        manager.roomProgressFill = roomFill;
    }

    private static TMP_Text GetButtonLabel(Button button)
    {
        if (button == null) return null;
        return button.GetComponentInChildren<TMP_Text>(true);
    }

    private static Canvas EnsureCanvas(UIManager manager)
    {
        Canvas canvas = manager.GetComponentInParent<Canvas>();
        if (canvas != null) return canvas;

        canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null) return canvas;

        GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");

        canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }

    private static RectTransform FindOrCreateRectTransform(Transform parent, string name)
    {
        GameObject go = FindOrCreateGameObject(parent, name);
        return EnsureRectTransform(go);
    }

    private static GameObject FindOrCreateGameObject(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        go.transform.SetParent(parent, false);
        return go;
    }

    private static RectTransform EnsureRectTransform(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect == null) rect = Undo.AddComponent<RectTransform>(go);
        return rect;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchorPreset(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
    }

    private static Button FindOrCreateButton(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, string label)
    {
        GameObject go = FindOrCreateGameObject(parent, name);
        RectTransform rect = EnsureRectTransform(go);
        SetAnchorPreset(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = go.GetComponent<Image>();
        if (image == null) image = Undo.AddComponent<Image>(go);
        image.color = new Color(0.20f, 0.28f, 0.38f, 0.95f);

        Button button = go.GetComponent<Button>();
        if (button == null) button = Undo.AddComponent<Button>(go);

        TMP_Text text = FindOrCreateTMPText(rect, "Label", size - new Vector2(20f, 12f), Vector2.zero, label, 28f, TextAlignmentOptions.Center);
        SetAnchorPreset(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        text.rectTransform.anchoredPosition = Vector2.zero;

        return button;
    }

    private static TMP_Text FindOrCreateTMPText(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, string content, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = FindOrCreateGameObject(parent, name);
        RectTransform rect = EnsureRectTransform(go);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TMP_Text text = go.GetComponent<TMP_Text>();
        if (text == null) text = Undo.AddComponent<TextMeshProUGUI>(go);

        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.color = Color.white;

        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        return text;
    }

    private static Image EnsureButtonVisuals(Button button)
    {
        if (button == null) return null;

        GameObject buttonObj = button.gameObject;
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        if (buttonRect == null) return null;

        Outline outline = buttonObj.GetComponent<Outline>();
        if (outline == null) outline = Undo.AddComponent<Outline>(buttonObj);
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform barBgRect = FindOrCreateRectTransform(buttonRect, "ProgressBG");
        SetAnchorPreset(barBgRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        barBgRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x - 40f, 10f);
        barBgRect.anchoredPosition = new Vector2(0f, 8f);

        Image barBg = barBgRect.GetComponent<Image>();
        if (barBg == null) barBg = Undo.AddComponent<Image>(barBgRect.gameObject);
        barBg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
        barBg.raycastTarget = false;

        RectTransform fillRect = FindOrCreateRectTransform(barBgRect, "ProgressFill");
        StretchToParent(fillRect);

        Image fillImage = fillRect.GetComponent<Image>();
        if (fillImage == null) fillImage = Undo.AddComponent<Image>(fillRect.gameObject);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 0f;
        fillImage.color = new Color(1f, 0.86f, 0.2f, 1f);
        fillImage.raycastTarget = false;

        return fillImage;
    }
}
