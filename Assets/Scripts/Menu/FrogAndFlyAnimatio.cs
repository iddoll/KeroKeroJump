using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FrogAndFlyAnimation : MonoBehaviour
{
    public Transform frog; // Об'єкт жабки (UI Image)
    public Transform fly; // Об'єкт мухи

    public Transform[] frogPoints; // Точки руху жабки
    public Transform[] flyPoints;  // Точки руху мухи

    public int[] frogLilyPadIndexes; // Індекси точок, де жабка має зупинятись
    public int[] flyLilyPadIndexes;  // Індекси точок, де муха має зупинятись

    public float moveDuration = 1f; // Час переміщення між точками
    public float waitTime = 0.5f; // Час очікування на кувшинках

    private int frogIndex = 0;
    private int flyIndex = 0;

    public Image frogImage; // Посилання на UI Image жабки
    public Sprite[] jumpSprites; // Спрайти для анімації стрибка
    public float spriteChangeSpeed = 0.1f; // Час зміни спрайту під час стрибка

    private int currentSpriteIndex = 0; // Індекс поточного спрайту для жабки

    void Start()
    {
        StartCoroutine(MoveCycle());
    }

    IEnumerator MoveCycle()
    {
        while (true)
        {
            int nextFrogIndex = (frogIndex + 1) % frogPoints.Length;
            int nextFlyIndex = (flyIndex + 1) % flyPoints.Length;

            // Повертаємо жабку та муху у напрямку наступної точки
            RotateTowards(frog, frogPoints[nextFrogIndex].position);
            RotateTowards(fly, flyPoints[nextFlyIndex].position, true); // <== тут додано "true"


            // Рух жабки та мухи одночасно
            yield return StartCoroutine(MoveObjects(frog, frogPoints[nextFrogIndex].position,
                                                    fly, flyPoints[nextFlyIndex].position,
                                                    moveDuration));

            // Змінюємо спрайт жабки після кожного стрибка
            ChangeFrogSprite();

            // Перевіряємо, чи жабка має зупинятись на кувшинці та змінюємо спрайт
            if (IsLilyPadPoint(nextFrogIndex, frogLilyPadIndexes))
            {
                ChangeFrogSprite();
            }

            // Перевіряємо, чи муха має зупинятись на кувшинці
            bool frogOnLilyPad = System.Array.Exists(frogLilyPadIndexes, x => x == nextFrogIndex);
            bool flyOnLilyPad = System.Array.Exists(flyLilyPadIndexes, x => x == nextFlyIndex);

            if (frogOnLilyPad || flyOnLilyPad)
            {
                yield return new WaitForSeconds(waitTime);
            }

            // Оновлюємо індекси руху
            frogIndex = nextFrogIndex;
            flyIndex = nextFlyIndex;
        }
    }

    IEnumerator MoveObjects(Transform obj1, Vector3 target1, Transform obj2, Vector3 target2, float duration)
    {
        Vector3 startPos1 = obj1.position;
        Vector3 startPos2 = obj2.position;
        float time = 0;

        while (time < duration)
        {
            obj1.position = Vector3.Lerp(startPos1, target1, time / duration);
            obj2.position = Vector3.Lerp(startPos2, target2, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj1.position = target1;
        obj2.position = target2;
    }

    // Зміна спрайту жабки
    void ChangeFrogSprite()
    {
        if (frogImage != null && jumpSprites.Length > 0)
        {
            // Збільшуємо індекс спрайту
            currentSpriteIndex = (currentSpriteIndex + 1) % jumpSprites.Length;
            frogImage.sprite = jumpSprites[currentSpriteIndex];
        }
    }

    void RotateTowards(Transform obj, Vector3 target, bool flip180 = false)
    {
        Vector3 direction = target - obj.position;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (flip180)
                angle += 180f;
            obj.rotation = Quaternion.Euler(0, 0, angle);
        }
    }


    bool IsLilyPadPoint(int index, int[] lilyPadIndexes)
    {
        foreach (int padIndex in lilyPadIndexes)
        {
            if (index == padIndex)
                return true;
        }
        return false;
    }
}
