using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GeneratedLevelMenu : MonoBehaviour
{
    const int LevelCount = 100;

    Dropdown dropdown;

    void Start()
    {
        var existing = transform.Find("GeneratedLevelDropdown");
        if (existing != null)
        {
            dropdown = existing.GetComponent<Dropdown>();
            SelectReached();
            return;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Sprite buttonSprite = null;
        var startButton = GameObject.Find("StartButton");
        if (startButton != null)
        {
            var startImage = startButton.GetComponent<Image>();
            if (startImage != null)
                buttonSprite = startImage.sprite;
        }

        dropdown = CreateDropdown(transform, font);
        var options = new List<Dropdown.OptionData>(LevelCount);
        for (int level = 1; level <= LevelCount; level++)
            options.Add(new Dropdown.OptionData(Label(level)));
        dropdown.options = options;
        SelectReached();

        CreatePlayButton(transform, font, buttonSprite);
    }

    void SelectReached()
    {
        if (dropdown == null || dropdown.options.Count == 0)
            return;

        int index = Mathf.Clamp(GameProgress.Reached - 1, 0, dropdown.options.Count - 1);
        dropdown.value = index;
        dropdown.RefreshShownValue();
    }

    void PlaySelected()
    {
        int level = dropdown != null ? dropdown.value + 1 : 1;
        LevelLaunch.Select(level);
        SceneManager.LoadScene("GameScene");
    }

    static string Label(int level)
    {
        return "Level " + level + "    seed " + Game.Level.LevelSeeds.For(level);
    }

    void CreatePlayButton(Transform parent, Font font, Sprite sprite)
    {
        var root = new GameObject("GeneratedPlayButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -70f);
        rect.sizeDelta = new Vector2(160f, 60f);

        var image = root.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = true;

        var button = root.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(PlaySelected);

        Text caption = CreateText(root.transform, "Label", font, TextAnchor.MiddleCenter, 22);
        Stretch(caption.rectTransform, 8f, 6f, 8f, 6f);
        caption.text = "Play";
    }

    Dropdown CreateDropdown(Transform parent, Font font)
    {
        var root = new GameObject("GeneratedLevelDropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
        root.transform.SetParent(parent, false);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, 140f);
        rootRect.sizeDelta = new Vector2(420f, 40f);
        var background = root.GetComponent<Image>();
        background.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

        Text caption = CreateText(root.transform, "Label", font, TextAnchor.MiddleLeft, 18);
        Stretch(caption.rectTransform, 12f, 4f, 28f, 4f);

        Text arrow = CreateText(root.transform, "Arrow", font, TextAnchor.MiddleCenter, 20);
        var arrowRect = arrow.rectTransform;
        arrowRect.anchorMin = new Vector2(1f, 0f);
        arrowRect.anchorMax = new Vector2(1f, 1f);
        arrowRect.pivot = new Vector2(1f, 0.5f);
        arrowRect.sizeDelta = new Vector2(28f, 0f);
        arrowRect.anchoredPosition = new Vector2(-6f, 0f);
        arrow.text = "v";

        RectTransform template = CreateTemplate(root.transform, font);
        template.gameObject.SetActive(false);

        var list = root.GetComponent<Dropdown>();
        list.targetGraphic = background;
        list.template = template;
        list.captionText = caption;
        list.itemText = template.Find("Viewport/Content/Item/Item Label").GetComponent<Text>();
        return list;
    }

    static RectTransform CreateTemplate(Transform parent, Font font)
    {
        var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        template.transform.SetParent(parent, false);
        var templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.anchoredPosition = Vector2.zero;
        templateRect.sizeDelta = new Vector2(0f, 240f);
        template.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.98f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(template.transform, false);
        Stretch(viewport.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 32f);

        var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(content.transform, false);
        var itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.sizeDelta = new Vector2(0f, 32f);

        var itemBackground = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBackground.transform.SetParent(item.transform, false);
        Stretch(itemBackground.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        itemBackground.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 1f);

        Text check = CreateText(item.transform, "Item Checkmark", font, TextAnchor.MiddleCenter, 16);
        var checkRect = check.rectTransform;
        checkRect.anchorMin = new Vector2(0f, 0f);
        checkRect.anchorMax = new Vector2(0f, 1f);
        checkRect.pivot = new Vector2(0f, 0.5f);
        checkRect.sizeDelta = new Vector2(8f, 0f);
        check.text = "";

        Text itemLabel = CreateText(item.transform, "Item Label", font, TextAnchor.MiddleLeft, 16);
        Stretch(itemLabel.rectTransform, 10f, 2f, 8f, 2f);

        var toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = itemBackground.GetComponent<Image>();
        toggle.graphic = check;
        toggle.isOn = true;

        var scroll = template.GetComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        return templateRect;
    }

    static Text CreateText(Transform parent, string name, Font font, TextAnchor anchor, int size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
