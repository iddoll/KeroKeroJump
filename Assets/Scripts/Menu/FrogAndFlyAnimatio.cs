using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FrogAndFlyAnimation : MonoBehaviour
{
    public Transform frog; // ��'��� ����� (UI Image)
    public Transform fly; // ��'��� ����

    public Transform[] frogPoints; // ����� ���� �����
    public Transform[] flyPoints;  // ����� ���� ����

    public int[] frogLilyPadIndexes; // ������� �����, �� ����� �� ����������
    public int[] flyLilyPadIndexes;  // ������� �����, �� ���� �� ����������

    public float moveDuration = 1f; // ��� ���������� �� �������
    public float waitTime = 0.5f; // ��� ���������� �� ���������

    private int frogIndex = 0;
    private int flyIndex = 0;

    public Image frogImage; // ��������� �� UI Image �����
    public Sprite[] jumpSprites; // ������� ��� ������� �������
    public float spriteChangeSpeed = 0.1f; // ��� ���� ������� �� ��� �������

    private int currentSpriteIndex = 0; // ������ ��������� ������� ��� �����

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

            // ��������� ����� �� ���� � �������� �������� �����
            RotateTowards(frog, frogPoints[nextFrogIndex].position);
            RotateTowards(fly, flyPoints[nextFlyIndex].position, true); // <== ��� ������ "true"


            // ��� ����� �� ���� ���������
            yield return StartCoroutine(MoveObjects(frog, frogPoints[nextFrogIndex].position,
                                                    fly, flyPoints[nextFlyIndex].position,
                                                    moveDuration));

            // ������� ������ ����� ���� ������� �������
            ChangeFrogSprite();

            // ����������, �� ����� �� ���������� �� �������� �� ������� ������
            if (IsLilyPadPoint(nextFrogIndex, frogLilyPadIndexes))
            {
                ChangeFrogSprite();
            }

            // ����������, �� ���� �� ���������� �� ��������
            bool frogOnLilyPad = System.Array.Exists(frogLilyPadIndexes, x => x == nextFrogIndex);
            bool flyOnLilyPad = System.Array.Exists(flyLilyPadIndexes, x => x == nextFlyIndex);

            if (frogOnLilyPad || flyOnLilyPad)
            {
                yield return new WaitForSeconds(waitTime);
            }

            // ��������� ������� ����
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

    // ���� ������� �����
    void ChangeFrogSprite()
    {
        if (frogImage != null && jumpSprites.Length > 0)
        {
            // �������� ������ �������
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
