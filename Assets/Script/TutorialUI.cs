using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Tutorial Pages")]
    [Tooltip("Tutorial pages in display order (first page is index 0).")]
    public List<GameObject> pages = new List<GameObject>();

    [Tooltip("Back button for previous page. Usually hidden on first page.")]
    public Button backButton;

    [Tooltip("Optional level text label (example: Level 1).")]
    public TextMeshProUGUI lvText;

    [Header("Tutorial Access")]
    [Tooltip("Root object to show or hide tutorial UI. If empty, this GameObject is used.")]
    public GameObject tutorialPanelRoot;

    [Tooltip("Button used during gameplay to reopen tutorial.")]
    public Button tutorialOpenButton;

    [Tooltip("When reopened, reset to first page before showing tutorial.")]
    public bool resetToFirstPageWhenOpen = true;

    [Header("Tutorial Start Behavior (This Scene)")]
    [Tooltip("Per-scene prefab setting: open tutorial automatically on Start.")]
    public bool openTutorialOnStart = true;

    private int currentIndex;
    private LevelManager levelManager;
    private CanvasGroup tutorialCanvasGroup;

    private void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            UpdateLeveltext(levelManager.lv);
        }

        if (tutorialPanelRoot == null)
        {
            tutorialPanelRoot = gameObject;
        }

        tutorialCanvasGroup = tutorialPanelRoot.GetComponent<CanvasGroup>();

        if (tutorialOpenButton != null)
        {
            tutorialOpenButton.onClick.RemoveListener(OpenTutorialFromButton);
            tutorialOpenButton.onClick.AddListener(OpenTutorialFromButton);
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (openTutorialOnStart)
        {
            OpenTutorial();
        }
        else
        {
            HideTutorial();
            Time.timeScale = 1f;
        }
    }

    public void UpdateLeveltext(int amount)
    {
        if (lvText != null)
        {
            lvText.text = "Level " + amount;
        }
    }

    public void OpenTutorialFromButton()
    {
        OpenTutorial();
    }

    public void OpenTutorial()
    {
        if (resetToFirstPageWhenOpen)
        {
            currentIndex = 0;
        }

        UpdatePages();
        SetTutorialVisible(true);

        if (backButton != null)
        {
            backButton.gameObject.SetActive(currentIndex > 0);
        }

        Time.timeScale = 0f;
    }

    public void CloseTutorial()
    {
        Time.timeScale = 1f;
        HideTutorial();
    }

    public void CloseUI()
    {
        CloseTutorial();
    }

    public void NextPage()
    {
        currentIndex++;

        if (currentIndex >= pages.Count)
        {
            CloseTutorial();
            return;
        }

        UpdatePages();

        if (backButton != null)
        {
            backButton.gameObject.SetActive(currentIndex >= 1);
        }
    }

    public void BackPage()
    {
        currentIndex = Mathf.Max(0, currentIndex - 1);
        UpdatePages();

        if (backButton != null)
        {
            backButton.gameObject.SetActive(currentIndex > 0);
        }
    }

    private void UpdatePages()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentIndex);
            }
        }
    }

    private void HideTutorial()
    {
        SetTutorialVisible(false);
    }

    private void SetTutorialVisible(bool isVisible)
    {
        if (tutorialPanelRoot == null)
        {
            return;
        }

        if (tutorialPanelRoot == gameObject)
        {
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = isVisible ? 1f : 0f;
                tutorialCanvasGroup.interactable = isVisible;
                tutorialCanvasGroup.blocksRaycasts = isVisible;
            }

            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(isVisible && i == currentIndex);
                }
            }

            return;
        }

        tutorialPanelRoot.SetActive(isVisible);
    }
}
