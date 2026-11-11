using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelDebugPicker : MonoBehaviour
{
    const int LevelCount = 100;

    LevelGenerator generator;
    Dropdown dropdown;
    bool suppress;

    public static void Ensure(LevelGenerator owner)
    {
        if (!Application.isEditor && !Debug.isDebugBuild)
            return;

        LevelDebugPicker picker = FindAnyObjectByType<LevelDebugPicker>();
        if (picker == null)
        {
            var root = new GameObject("LevelDebugPicker");
            picker = root.AddComponent<LevelDebugPicker>();
            picker.Build();
        }

        picker.generator = owner;
        picker.Sync();
    }

    void Build()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            events.AddComponent<StandaloneInputModule>();
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        dropdown = CreateDropdown(canvasObject.transform, font);
        var options = new List<Dropdown.OptionData>(LevelCount);
        for (int level = 1; level <= LevelCount; level++)
            options.Add(new Dropdown.OptionData(Label(level)));

        dropdown.options = options;
        dropdown.onValueChanged.AddListener(OnPicked);
        dropdown.RefreshShownValue();
    }

    void Sync()
    {
        if (dropdown == null || generator == null)
            return;

        int index = Mathf.Clamp(generator.levelToGenerate, 1, LevelCount) - 1;
        suppress = true;
        dropdown.SetValueWithoutNotify(index);
        dropdown.captionText.text = Label(index + 1);
        suppress = false;
    }

    void OnPicked(int index)
    {
        if (suppress || generator == null)
            return;

        int level = index + 1;
        if (generator.levelToGenerate == level)
            return;

        generator.levelToGenerate = level;
        generator.GenerateAndBuildLevel();
    }

    static string Label(int level)
    {
        return "Level " + level + "    seed " + Game.Level.LevelSeeds.For(level);
    }

    static Dropdown CreateDropdown(Transform parent, Font font)
    {
        var root = new GameObject("LevelDropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
        root.transform.SetParent(parent, false);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -16f);
        rootRect.sizeDelta = new Vector2(460f, 40f);
        var background = root.GetComponent<Image>();
        background.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

        Text caption = CreateText(root.transform, "Label", font, TextAnchor.MiddleLeft, 22);
        Stretch(caption.rectTransform, 12f, 4f, 28f, 4f);

        Text arrow = CreateText(root.transform, "Arrow", font, TextAnchor.MiddleCenter, 22);
        var arrowRect = arrow.rectTransform;
        arrowRect.anchorMin = new Vector2(1f, 0f);
        arrowRect.anchorMax = new Vector2(1f, 1f);
        arrowRect.pivot = new Vector2(1f, 0.5f);
        arrowRect.sizeDelta = new Vector2(28f, 0f);
        arrowRect.anchoredPosition = new Vector2(-6f, 0f);
        arrow.text = "v";

        RectTransform template = CreateTemplate(root.transform, font);
        template.gameObject.SetActive(false);

        var dropdown = root.GetComponent<Dropdown>();
        dropdown.targetGraphic = background;
        dropdown.template = template;
        dropdown.captionText = caption;
        dropdown.itemText = template.Find("Viewport/Content/Item/Item Label").GetComponent<Text>();
        return dropdown;
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
        templateRect.sizeDelta = new Vector2(0f, 280f);
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
        contentRect.sizeDelta = new Vector2(0f, 36f);

        var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(content.transform, false);
        var itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.sizeDelta = new Vector2(0f, 36f);

        var itemBackground = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBackground.transform.SetParent(item.transform, false);
        Stretch(itemBackground.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        itemBackground.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 1f);

        Text check = CreateText(item.transform, "Item Checkmark", font, TextAnchor.MiddleCenter, 18);
        var checkRect = check.rectTransform;
        checkRect.anchorMin = new Vector2(0f, 0f);
        checkRect.anchorMax = new Vector2(0f, 1f);
        checkRect.pivot = new Vector2(0f, 0.5f);
        checkRect.sizeDelta = new Vector2(8f, 0f);
        check.text = "";

        Text itemLabel = CreateText(item.transform, "Item Label", font, TextAnchor.MiddleLeft, 20);
        Stretch(itemLabel.rectTransform, 12f, 2f, 8f, 2f);

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
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
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
