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
        foreach (Button btn in levelButtons)
        {
            string sceneName = btn.name; 

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
            Debug.Log("Loading" + sceneName);
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