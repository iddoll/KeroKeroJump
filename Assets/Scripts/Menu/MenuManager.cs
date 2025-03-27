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
        SceneManager.LoadScene("TrainingLevelScene"); // Запускаємо тренувальний рівень
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public void ShowLevels()
    {
        levelPanel.SetActive(true); // Відкриваємо панель рівнів
    }

    public void HideLevels()
    {
        levelPanel.SetActive(false); // Ховаємо панель рівнів
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); // Завантажуємо обраний рівень
    }
}
