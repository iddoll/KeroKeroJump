using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class GrowOnHover : MonoBehaviour
{
    private Vector3 originalScale;
    [SerializeField] private Vector3 targetScale = Vector3.one * 1.1f;

    Vector3 pointerPosition;
    CircleCollider2D myCollider;
    bool isGrown;

    private void Awake()
    {
        myCollider = GetComponent<CircleCollider2D>();
    }
    private void Start()
    {
        originalScale = transform.localScale;
    }
    private void LateUpdate()
    {
        pointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pointerPosition.z = 0;

        if (myCollider.OverlapPoint(pointerPosition))
        {
            if (!isGrown)
            {
                isGrown = true;
                transform.localScale = targetScale;
            }
        }
        else if (isGrown)
        {
            isGrown = false;
            transform.localScale = originalScale;
        }

    }
}
