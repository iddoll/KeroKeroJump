using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        string lastScene = PlayerPrefs.GetString("LastLevel", "MainMenuScene"); // запасний вар≥ант Ч головне меню
        UnityEngine.SceneManagement.SceneManager.LoadScene(lastScene);
    }

}
