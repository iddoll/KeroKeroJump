using UnityEngine;
using UnityEngine.SceneManagement; // Додаємо бібліотеку для управління сценами

public class LilyPad : MonoBehaviour
{
    public GameObject frog;
    public Transform checkPoint;
    public Sprite brokenLilyPadSprite;
    public Animator arrowAnimator;

    public GameObject youMissedFliesTxt;


    public enum LilyType
    {
        Delayed,
        DelayedAfterExit,
        WinPad // Додаємо тип WinPad
    }

    public enum ArrowType
    {
        Horizontal,
        Vertical,
        AllDirections
    }

    public LilyType lilyType;
    public ArrowType arrowType;

    private SpriteRenderer spriteRenderer;
    private Collider2D lilyCollider;
    private Rigidbody2D frogRigidbody;
    public bool isBroken = false;
    private bool frogOnCheckPoint = false;
    private bool frogWasOnCheckPoint = false;
    private float timer = 0f;
    public float breakTime = 3f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lilyCollider = GetComponent<Collider2D>();
        if (frog != null)
        {
            frogRigidbody = frog.GetComponent<Rigidbody2D>();
        }

        if (arrowAnimator != null)
        {
            string triggerName = GetTriggerName();
            if (!string.IsNullOrEmpty(triggerName))
            {
                arrowAnimator.SetTrigger(triggerName);
            }
        }
    }

    void Update()
    {
        if (isBroken)
            return;

        frogOnCheckPoint = FrogOnCheckPoint();

        switch (lilyType)
        {
            case LilyType.Delayed:
                if (frogOnCheckPoint && !isBroken)
                {
                    timer += Time.deltaTime;
                    if (timer >= breakTime)
                    {
                        ChangeSprite();
                    }
                }
                else if (!frogOnCheckPoint && !isBroken)
                {
                    timer = 0f;
                }
                break;

            case LilyType.DelayedAfterExit:
                if (frogWasOnCheckPoint && !isBroken)
                {
                    timer += Time.deltaTime;
                    if (timer >= breakTime)
                    {
                        ChangeSprite();
                    }
                }
                else if (frogOnCheckPoint)
                {
                    frogWasOnCheckPoint = true;
                }
                break;

            case LilyType.WinPad:
                if (frogOnCheckPoint)
                {
                    if (FindObjectsByType<Fly>(FindObjectsSortMode.None).Length == 0)
                        LoadNextLevel();
                    else
                    {
                        youMissedFliesTxt.gameObject.SetActive(true);
                    }
                }

                break;
        }
    }

    void ChangeSprite()
    {
        isBroken = true;
        spriteRenderer.sprite = brokenLilyPadSprite;

        if (lilyCollider != null)
            lilyCollider.enabled = false;

        if (FrogOnCheckPoint())
        {
            frog.GetComponent<FrogJump>().GameOver();
        }

        this.enabled = false;
    }

    bool FrogOnCheckPoint()
    {
        if (frog != null && checkPoint != null)
        {
            float distance = Vector2.Distance(frog.transform.position, checkPoint.position);
            return distance < 0.5f;
        }
        return false;
    }

    void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = "";

        switch (currentScene)
        {
            case "TrainingLevelScene":
                nextScene = "TrainingLevelScene2";
                break;
            case "TrainingLevelScene2":
                nextScene = "TrainingLevelScene3";
                break;
            case "TrainingLevelScene3":
                nextScene = "Level_1";
                break;
            case "Level_1":
                nextScene = "Level_2";
                break;
            case "Level_2":
                nextScene = "Level_3";
                break;
            case "Level_3":
                nextScene = "WinMenu";
                break;
        }

        if (!string.IsNullOrEmpty(nextScene))
        {
            Debug.Log($"Frog reached WinPad! Loading {nextScene}...");
            SceneManager.LoadScene(nextScene);
        }
    }

    public bool IsDirectionAllowed(Vector2 direction)
    {
        direction = direction.normalized;

        Vector2[] allowedDirections;
        switch (arrowType)
        {
            case ArrowType.Horizontal:
                allowedDirections = new Vector2[] { Vector2.left, Vector2.right };
                break;
            case ArrowType.Vertical:
                allowedDirections = new Vector2[] { Vector2.up, Vector2.down };
                break;
            case ArrowType.AllDirections:
                return true;
            default:
                return false;
        }

        foreach (var allowedDirection in allowedDirections)
        {
            if (Vector2.Dot(direction, allowedDirection) > 0.9f)
                return true;
        }

        return false;
    }

    private string GetTriggerName()
    {
        switch (arrowType)
        {
            case ArrowType.Horizontal:
                return "isHorizontal";
            case ArrowType.Vertical:
                return "isVertical";
            default:
                return ""; // Якщо AllDirections, тригер не потрібен
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
