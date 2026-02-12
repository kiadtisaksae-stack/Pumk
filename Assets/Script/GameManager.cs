using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int Gold { get; set; }
    public string loadcurrentlevel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddGold(int amount)
    {
        Gold += amount;
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        uIManager.UpdateGoldText(Gold);

    }
    public void OnSelectLevel(string levelName)
    {
        
    }
    public void OnClickPlay()
    {
        SceneManager.LoadScene("");
    }
}

