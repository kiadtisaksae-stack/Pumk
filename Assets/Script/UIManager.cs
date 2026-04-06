using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{

    [Header("Level Buttons")]
    public List<Button> levelButtons = new List<Button>();

    public List<GameObject> hideOnStartObj;
    


    void Start()
    {
        SetupLevelButtons();
        SetUpStart();
    }

    // Setup ปุ่มให้โหลด Scene อัตโนมัติ
    public void SetUpStart()
    {
        foreach (GameObject gameObject in hideOnStartObj)
        {
            gameObject.SetActive(false);

        }
    }
    public void SetupLevelButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            Button btn = levelButtons[i];
            string sceneName = btn.name; 

            // ปลดล็อกด่านแรก (index 0) ให้เล่นได้เสมอ
            // ด่านอื่นๆ เช็คจาก PlayerPrefs
            if (i == 0 || PlayerPrefs.GetInt("UnlockedLevel_" + i, 0) == 1)
            {
                btn.interactable = true;
            }
            else
            {
                btn.interactable = false;
            }

            btn.onClick.AddListener(() =>
            {
                LoadScene(sceneName);
            });
        }
    }
    public void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene not found" + sceneName);
            return;
        }
    }

    public void UpdateGoldText(int goldAmount)
    {
        
    }
}