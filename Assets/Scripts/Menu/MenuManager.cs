using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject levelPanel; // Панель вибору рівня

    void Start()
    {
        levelPanel.SetActive(false); // Спочатку приховуємо панель
    }

    public void PlayGame()
    {
        levelPanel.SetActive(true); // Показуємо панель вибору рівнів
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); // Завантажуємо обраний рівень
    }

    public void BackToMenu()
    {
        levelPanel.SetActive(false); // Ховаємо панель рівнів
    }
}
