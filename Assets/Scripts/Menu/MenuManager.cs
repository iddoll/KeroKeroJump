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
        levelPanel.SetActive(true); // �������� ������ ������ ����
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); // ����������� ������� �����
    }

    public void BackToMenu()
    {
        levelPanel.SetActive(false); // ������ ������ ����
    }
}
