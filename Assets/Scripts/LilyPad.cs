using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Game.Level; // Додаємо, щоб використовувати ваші enum

public class LilyPad : MonoBehaviour
{
    public FrogJump frog; // Замінюємо посилання на GameObject на сам скрипт
    public Transform checkPoint;
    public Sprite brokenLilyPadSprite;
    public Animator arrowAnimator;

    public GameObject youMissedFliesTxt;

    // Використовуємо типи з LevelConfig
    public LilyPadType lilyPadType;
    public LilyPadDirection lilyPadDirection;

    public Sprite crackedLilyPadSprite;

    enum PadWear
    {
        Intact,
        Cracked,
        Broken
    }

    PadWear wear = PadWear.Intact;
    bool frogWasOnPad;
    bool gateTouched;
    bool winOpened;

    private SpriteRenderer spriteRenderer;
    private Collider2D lilyCollider;
    public bool isBroken = false;
    private float timer = 0f;
    public float breakTime = 2f;

    private void Awake()
    {
        // Присвоюємо посилання на жабу при створенні рівня
        frog = FindObjectOfType<FrogJump>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lilyCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Необхідно, щоб префаби мали відповідні компоненти,
        // які LevelGenerator може налаштувати після спавну
    }

    public void Prepare(Sprite cracked, Sprite sunk)
    {
        if (cracked != null)
            crackedLilyPadSprite = cracked;
        if (brokenLilyPadSprite == null && sunk != null)
            brokenLilyPadSprite = sunk;
        if (checkPoint == null)
            checkPoint = transform.Find("LilyCentre");
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        breakTime = 2f;
        if (lilyPadType == LilyPadType.Breakable || lilyPadType == LilyPadType.Gate)
            BecomeCracked();
    }

    void Update()
    {
        if (isBroken || wear == PadWear.Broken)
            return;

        ResolveFrog();
        bool frogHere = FrogOnCheckPoint();

        if (lilyPadType == LilyPadType.WinPad)
        {
            if (frogHere && FliesManager.AllCollected)
            {
                HideMissedNotice();
                OpenWinMenu();
            }
            else if (frogHere)
                ShowMissedNotice();
            else
                HideMissedNotice();
            return;
        }

        if (lilyPadType == LilyPadType.StartPad)
            return;

        if (lilyPadType == LilyPadType.Gate)
        {
            if (frogHere)
                gateTouched = true;
            if (gateTouched)
            {
                timer += Time.deltaTime;
                if (timer >= breakTime)
                    BreakFully(frogHere);
            }

            frogWasOnPad = frogHere && wear != PadWear.Broken;
            return;
        }

        if (frogHere)
        {
            timer += Time.deltaTime;
            if (timer >= breakTime)
            {
                timer = 0f;
                if (wear == PadWear.Intact)
                    BecomeCracked();
                else
                    BreakFully(sinkFrog: true);
            }
        }
        else if (frogWasOnPad)
        {
            if (wear == PadWear.Cracked)
                BreakFully(sinkFrog: false);
            else
                timer = 0f;
        }

        frogWasOnPad = frogHere && wear != PadWear.Broken;
    }

    void ResolveFrog()
    {
        if (frog != null && frog.gameObject.scene.isLoaded)
            return;

        frog = FindAnyObjectByType<FrogJump>();
        if (checkPoint == null)
            checkPoint = transform.Find("LilyCentre");
    }

    void BecomeCracked()
    {
        wear = PadWear.Cracked;
        timer = 0f;
        if (spriteRenderer != null && crackedLilyPadSprite != null)
            spriteRenderer.sprite = crackedLilyPadSprite;
    }

    void BreakFully(bool sinkFrog)
    {
        wear = PadWear.Broken;
        isBroken = true;
        timer = 0f;

        if (spriteRenderer != null && brokenLilyPadSprite != null)
            spriteRenderer.sprite = brokenLilyPadSprite;

        if (lilyCollider != null)
            lilyCollider.enabled = false;

        if (sinkFrog && FrogOnCheckPoint() && frog != null)
            frog.GameOver();
    }

    bool FrogOnCheckPoint()
    {
        if (frog == null)
            return false;

        Vector2 point = checkPoint != null ? (Vector2)checkPoint.position : (Vector2)transform.position;
        return Vector2.Distance(frog.transform.position, point) < 0.5f;
    }

    void OpenWinMenu()
    {
        if (winOpened)
            return;

        winOpened = true;
        var generator = FindAnyObjectByType<LevelGenerator>();
        if (generator != null)
            GameProgress.Complete(generator.levelToGenerate);
        WinMenuPanel.Show(generator);
    }

    public static void HideMissedNotice()
    {
        Transform panel = FindMissedPanel();
        if (panel != null)
            panel.gameObject.SetActive(false);
    }

    static string missedTemplate;

    static void ShowMissedNotice()
    {
        Transform panel = FindMissedPanel();
        if (panel == null)
            return;

        panel.gameObject.SetActive(true);
        var label = panel.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
            return;

        if (string.IsNullOrEmpty(missedTemplate))
        {
            missedTemplate = label.text;
            if (string.IsNullOrEmpty(missedTemplate) || !missedTemplate.Contains("{n}"))
                missedTemplate = "Залишилось мух: {n}";
        }

        int left = Mathf.Max(0, FliesManager.count - FliesManager.eaten);
        label.text = missedTemplate.Replace("{n}", left.ToString());
    }

    static Transform FindMissedPanel()
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Transform[] all = canvases[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < all.Length; j++)
            {
                if (all[j].name == "youMissedFlies")
                    return all[j];
            }
        }
        return null;
    }

    // Метод для встановлення властивостей після спавну
    public void SetProperties(LilyPadType type, LilyPadDirection direction)
    {
        this.lilyPadType = type;
        this.lilyPadDirection = direction;

        if (arrowAnimator != null)
        {
            // Скидаємо всі тригери
            arrowAnimator.ResetTrigger("isVertical");
            arrowAnimator.ResetTrigger("isHorizontal");

            switch (lilyPadDirection)
            {
                case LilyPadDirection.Vertical:
                    arrowAnimator.SetTrigger("isVertical");
                    break;
                case LilyPadDirection.Horizontal:
                    arrowAnimator.SetTrigger("isHorizontal");
                    break;
                case LilyPadDirection.All:
                    // Нічого не вмикаємо → анімація "без стрілки"
                    break;
            }
        }
    }

    public bool IsDirectionAllowed(Vector2 direction)
    {
        direction = direction.normalized;

        switch (lilyPadDirection)
        {
            case LilyPadDirection.Vertical:
                return Vector2.Dot(direction, Vector2.up) > 0.9f ||
                       Vector2.Dot(direction, Vector2.down) > 0.9f;

            case LilyPadDirection.Horizontal:
                return Vector2.Dot(direction, Vector2.left) > 0.9f ||
                       Vector2.Dot(direction, Vector2.right) > 0.9f;

            case LilyPadDirection.All:
                return true; // всі сторони дозволені

            default:
                return false;
        }
    }
    
    void LateUpdate()
    {
        if (arrowAnimator != null)
        {
            arrowAnimator.transform.rotation = Quaternion.identity;
        }
    }
}