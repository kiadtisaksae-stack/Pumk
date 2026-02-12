using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    private Button start;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        start = GetComponent<Button>();
        start.onClick.AddListener(ClisckStart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ClisckStart()
    {
        SceneManager.LoadScene("GameHub");
    }
}
