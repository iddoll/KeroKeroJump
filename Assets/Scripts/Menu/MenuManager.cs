using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject levelPanel1; // ������ ������ ����
    public GameObject levelPanel2; // ������ ������ ����

    void Start()
    {
        levelPanel1.SetActive(false); // �������� ��������� ������
        levelPanel2.SetActive(false);
    }

    public void PlayGame()
    {
        levelPanel1.SetActive(true); // �������� ������ ������ �����
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
        levelPanel1.SetActive(false); // ������ ������ �����
        levelPanel2.SetActive(false);
    }

    public void NextLevelPage()
    {
        levelPanel2.SetActive(true);
        levelPanel1.SetActive(false);
    }
}
