using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinMenuPanel : MonoBehaviour
{
    static WinMenuPanel instance;

    GameObject root;
    LevelGenerator generator;
    bool wired;

    public static void Show(LevelGenerator levelGenerator)
    {
        if (instance == null)
        {
            var host = new GameObject("WinMenuPanel");
            instance = host.AddComponent<WinMenuPanel>();
            GameObject existing = FindWinPanel();
            if (existing != null)
                instance.Use(existing);
            else
                instance.Build();
        }

        instance.generator = levelGenerator;
        instance.root.SetActive(true);
        Time.timeScale = 0f;
    }

    public static void Hide()
    {
        if (instance != null)
            instance.root.SetActive(false);
        else
        {
            GameObject existing = FindWinPanel();
            if (existing != null)
                existing.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void Use(GameObject panel)
    {
        root = panel;
        WireButtons();
    }

    static GameObject FindWinPanel()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Transform[] all = canvases[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < all.Length; j++)
            {
                if (all[j].name == "WinPanel")
                    return all[j].gameObject;
            }
        }

        return null;
    }

    void WireButtons()
    {
        if (wired || root == null)
            return;

        wired = true;
        Wire("RestartButton", Restart);
        Wire("MenuButton", OpenMenu);
        Wire("NextButton", NextLevel);
    }

    void Wire(string objectName, UnityEngine.Events.UnityAction action)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.name == objectName)
                buttons[i].onClick.AddListener(action);
        }
    }

    void Build()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Transform parent = null;
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
            parent = canvas.transform;

        root = new GameObject("WinPanel", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        if (parent == null)
            root.transform.SetParent(transform, false);

        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(280f, 280f);

        var background = root.AddComponent<Image>();
        background.color = new Color(0.05f, 0.08f, 0.05f, 0.94f);

        var title = CreateText(root.transform, font, "Lily!", 32, new Vector2(0f, 90f));
        title.color = new Color(0.93f, 0.84f, 0f, 1f);

        CreateButton(root.transform, font, "Заново", new Vector2(0f, 20f), Restart);
        CreateButton(root.transform, font, "Меню", new Vector2(0f, -50f), OpenMenu);
        CreateButton(root.transform, font, "Наступний рівень", new Vector2(0f, -120f), NextLevel);
    }

    void Restart()
    {
        Hide();
        if (generator != null)
            generator.GenerateAndBuildLevel();
    }

    void OpenMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    void NextLevel()
    {
        Hide();
        if (generator == null)
            return;

        generator.levelToGenerate++;
        LevelLaunch.Select(generator.levelToGenerate);
        generator.GenerateAndBuildLevel();
    }

    static Text CreateText(Transform parent, Font font, string value, int size, Vector2 position)
    {
        var go = new GameObject(value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(250f, 40f);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = value;
        return text;
    }

    static void CreateButton(Transform parent, Font font, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(230f, 48f);
        var image = go.GetComponent<Image>();
        image.color = new Color(0.15f, 0.35f, 0.18f, 1f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        var text = CreateText(go.transform, font, label, 20, Vector2.zero);
        text.rectTransform.sizeDelta = new Vector2(220f, 40f);
    }
}
