using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public static class StoryVideoAutoSetupTool
{
    private const string ToolsMenuAll = "Tools/Story Video/Auto Setup For ALL Build Scenes";
    private const string ToolsMenuCurrent = "Tools/Story Video/Auto Setup For Current Scene";

    [MenuItem(ToolsMenuAll)]
    private static void AutoSetupAllBuildScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Story Video Auto Setup", "No scenes in Build Settings.", "OK");
            return;
        }

        int processed = 0;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (!scenes[i].enabled) continue;
            if (!File.Exists(scenes[i].path)) continue;

            Scene scene = EditorSceneManager.OpenScene(scenes[i].path, OpenSceneMode.Single);
            SetupScene(scene);
            EditorSceneManager.SaveScene(scene);
            processed++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Story Video Auto Setup", $"Done. Processed {processed} scene(s).", "OK");
    }

    [MenuItem(ToolsMenuCurrent)]
    private static void AutoSetupCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SetupScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Story Video Auto Setup", $"Done in scene: {scene.name}", "OK");
    }

    private static void SetupScene(Scene scene)
    {
        EnsureEventSystemInScene();

        VideoPlayer[] players = Object.FindObjectsByType<VideoPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (players == null || players.Length == 0) return;

        StoryVideoController primaryController = null;

        for (int i = 0; i < players.Length; i++)
        {
            VideoPlayer player = players[i];
            if (player == null) continue;

            StoryVideoController controller = player.GetComponent<StoryVideoController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<StoryVideoController>(player.gameObject);
            }

            Canvas canvas;
            RawImage rawImage;
            Button skipButton;
            CanvasGroup group;

            EnsureVideoUI(player.transform, out canvas, out rawImage, out skipButton, out group);

            bool isTitleScene = scene.name.ToLower().Contains("title");
            controller.AutoPlayOnStart = isTitleScene;
            controller.EditorAssignReferences(player, canvas, rawImage, skipButton, group);
            EditorUtility.SetDirty(controller);

            if (primaryController == null)
            {
                primaryController = controller;
            }
        }

        bool isMainMenuScene = scene.name.ToLower().Contains("mainmenu");
        if (isMainMenuScene && primaryController != null)
        {
            EnsureMainMenuPlayStoryButton(primaryController);
        }
    }

    private static void EnsureVideoUI(Transform videoRoot, out Canvas videoCanvas, out RawImage videoRawImage, out Button skipButton, out CanvasGroup fadeGroup)
    {
        GameObject canvasGO = FindOrCreateChild(videoRoot, "StoryVideoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        videoCanvas = canvasGO.GetComponent<Canvas>();
        videoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        videoCanvas.sortingOrder = 500;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        fadeGroup = canvasGO.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 1f;

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        StretchToParent(canvasRect);

        GameObject rawGO = FindOrCreateChild(canvasGO.transform, "VideoRawImage", typeof(RectTransform), typeof(RawImage));
        videoRawImage = rawGO.GetComponent<RawImage>();
        videoRawImage.color = Color.white;
        videoRawImage.raycastTarget = false;
        StretchToParent(rawGO.GetComponent<RectTransform>());

        GameObject skipGO = FindOrCreateChild(canvasGO.transform, "SkipButton", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform skipRect = skipGO.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(1f, 0f);
        skipRect.anchorMax = new Vector2(1f, 0f);
        skipRect.pivot = new Vector2(1f, 0f);
        skipRect.sizeDelta = new Vector2(170f, 60f);
        skipRect.anchoredPosition = new Vector2(-28f, 22f);

        Image skipImage = skipGO.GetComponent<Image>();
        skipImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        skipButton = skipGO.GetComponent<Button>();

        GameObject labelGO = FindOrCreateChild(skipGO.transform, "Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        StretchToParent(labelRect);

        TextMeshProUGUI label = labelGO.GetComponent<TextMeshProUGUI>();
        label.text = "Skip";
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.enableWordWrapping = false;
        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }
    }

    private static void EnsureMainMenuPlayStoryButton(StoryVideoController targetController)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject buttonGO = FindOrCreateChild(canvas.transform, "PlayStoryVideoButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(StoryVideoPlayButtonBinder));
        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(320f, 74f);
        rect.anchoredPosition = new Vector2(0f, 100f);

        Image image = buttonGO.GetComponent<Image>();
        image.color = new Color(0.16f, 0.36f, 0.22f, 0.95f);

        Button button = buttonGO.GetComponent<Button>();

        GameObject labelGO = FindOrCreateChild(buttonGO.transform, "Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        StretchToParent(labelRect);

        TextMeshProUGUI label = labelGO.GetComponent<TextMeshProUGUI>();
        label.text = "Play Story Video";
        label.fontSize = 34f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.enableWordWrapping = false;
        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }

        StoryVideoPlayButtonBinder binder = buttonGO.GetComponent<StoryVideoPlayButtonBinder>();
        binder.EditorAssignReferences(button, targetController);
        EditorUtility.SetDirty(binder);
    }

    private static void EnsureEventSystemInScene()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }

    private static GameObject FindOrCreateChild(Transform parent, string name, params System.Type[] components)
    {
        Transform found = parent.Find(name);
        if (found != null)
        {
            EnsureComponents(found.gameObject, components);
            return found.gameObject;
        }

        GameObject go = new GameObject(name, components);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void EnsureComponents(GameObject go, params System.Type[] components)
    {
        for (int i = 0; i < components.Length; i++)
        {
            if (go.GetComponent(components[i]) == null)
            {
                Undo.AddComponent(go, components[i]);
            }
        }
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
