using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;

public class FrogJump : MonoBehaviour
{
    public float jumpSpeed = 5f;
    public Transform[] checkPoints;

    private bool isJumping = false;
    private bool isGameOver = false;

    private Collider2D frogCollider;
    private Animator animator;
    private Rigidbody2D frogRigidbody;
    private Transform frogCheck;

    private LilyPad currentLilyPad;

    public int flyCount = 0;
    public TMP_Text flyText; // Призначте цей об'єкт в Inspector

    [Header("Sound")]
    [SerializeField] private AudioClip failJumpSound;
    [SerializeField] private AudioClip successJumpSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip sinkSound;


    void Start()
    {
        frogCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        frogRigidbody = GetComponent<Rigidbody2D>();
        frogCheck = transform.Find("FrogCheck");

        Collider2D[] lilyPads = GameObject.FindGameObjectsWithTag("LilyPad")
                                         .Select(obj => obj.GetComponent<Collider2D>())
                                         .ToArray();

        foreach (Collider2D lilyPad in lilyPads)
        {
            Physics2D.IgnoreCollision(frogCollider, lilyPad, true);
        }
    }

    void Update()
    {
        if (isGameOver || isJumping)
            return;

        if (IsInWater())
        {
            GameOver();
            return;
        }

        UpdateCurrentLilyPad();

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D clicked = Physics2D.OverlapPoint(mousePos);

            // Якщо натиснули на муху, шукаємо кувшинку під нею
            if (clicked != null && clicked.CompareTag("Fly"))
            {
                clicked = FindLilyPadUnderFly(mousePos);
            }

            if (clicked != null && clicked.CompareTag("LilyPad"))
            {
                Transform lilyCentre = clicked.transform.Find("LilyCentre");
                if (lilyCentre == null)
                    return;

                LilyPad targetLilyPad = clicked.GetComponent<LilyPad>();
                if (targetLilyPad != null && targetLilyPad.isBroken)
                {
                    SoundManager.instance.PlayEffect(failJumpSound);
                    return;
                }

                Vector2 direction = ((Vector2)lilyCentre.position - (Vector2)transform.position).normalized;

                if (CanJumpTo(lilyCentre.position) && currentLilyPad != null && currentLilyPad.IsDirectionAllowed(direction))
                {
                    SoundManager.instance.PlayEffect(jumpSound);
                    FlipToDirection(lilyCentre.position);
                    StartCoroutine(JumpTo(lilyCentre.position));
                }
                else
                {
                    SoundManager.instance.PlayEffect(failJumpSound);
                    Debug.Log("Jump not allowed: current lily doesn't permit this direction.");
                }
            }
        }
    }

    // Допоміжний метод для пошуку кувшинки під мухою
    Collider2D FindLilyPadUnderFly(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(position);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("LilyPad"))
            {
                return hit; // Повертаємо першу знайдену кувшинку
            }
        }
        return null;
    }

    void UpdateCurrentLilyPad()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(frogCheck.position, 0.1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("LilyPad"))
            {
                currentLilyPad = hit.GetComponent<LilyPad>();
                return;
            }
        }

        currentLilyPad = null;
    }

    bool CanJumpTo(Vector2 targetPos)
    {
        foreach (Transform check in checkPoints)
        {
            if (Vector2.Distance(check.position, targetPos) < 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    void FlipToDirection(Vector2 targetPos)
    {
        Vector2 direction = targetPos - (Vector2)transform.position;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            transform.rotation = Quaternion.Euler(0, 0, direction.x > 0 ? 90 : -90);
        else
            transform.rotation = Quaternion.Euler(0, 0, direction.y > 0 ? 180 : 0);
    }

    IEnumerator JumpTo(Vector2 targetPos)
    {
        isJumping = true;
        animator.SetBool("isJumping", true);
        yield return new WaitForSeconds(0.2f);

        Vector2 start = transform.position;
        float t = 0f;
        SoundManager.instance.PlayEffect(successJumpSound);

        while (t < 1f)
        {
            transform.position = Vector2.Lerp(start, targetPos, t);
            t += Time.deltaTime * jumpSpeed;
            yield return null;
        }

        transform.position = targetPos;
        isJumping = false;
        animator.SetBool("isJumping", false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("LilyPad"))
        {
            LilyPad pad = collision.collider.GetComponent<LilyPad>();
            if (pad != null && pad.isBroken)
            {
                GameOver();
            }
        }

        if (collision.collider.CompareTag("Water") && !IsOnLilyPad())
        {
            GameOver();
        }
    }

    bool IsOnLilyPad()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(frogCheck.position, 0.1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("LilyPad"))
                return true;
        }
        return false;
    }

    public void GameOver()
    {
        SoundManager.instance.PlayEffect(sinkSound);
        isGameOver = true;
        animator.SetTrigger("SinkToWater");
        StopAllCoroutines();
        animator.SetBool("isJumping", false);

        if (frogRigidbody != null)
        {
            frogRigidbody.linearVelocity = Vector2.zero;
            frogRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        frogCollider.enabled = false;
        StartCoroutine(HandleGameOverSequence());

    }

    private IEnumerator HandleGameOverSequence()
    {
        PlayerPrefs.SetString("LastLevel", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        yield return new WaitForSeconds(1f); // Чекаємо завершення анімації
        UnityEngine.SceneManagement.SceneManager.LoadScene("AfterDeathScene");
    }


    bool IsInWater()
    {
        Collider2D hit = Physics2D.OverlapCircle(frogCheck.position, 0.1f, LayerMask.GetMask("Water"));
        return hit != null;
    }

    public void CollectFly()
    {
        //flyCount++;
        //UpdateFlyUI();
    }

    //private void UpdateFlyUI()
    //{
    //    if (flyText != null)
    //    {
    //        flyText.text = "Flies: " + flyCount;
    //    }
    //}
}
