using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject levelPanel; // ������ ������ ����

    void Start()
    {
        levelPanel.SetActive(false); // �������� ��������� ������
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("TrainingLevelScene"); // ��������� ������������ �����
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public void ShowLevels()
    {
        levelPanel.SetActive(true); // ³�������� ������ ����
    }

    public void HideLevels()
    {
        levelPanel.SetActive(false); // ������ ������ ����
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); // ����������� ������� �����
    }
}
