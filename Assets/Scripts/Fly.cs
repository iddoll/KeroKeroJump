using UnityEngine;

public class Fly : MonoBehaviour
{
    FliesManager manager;

    private void Awake()
    {
        manager = FindAnyObjectByType<FliesManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Frog")) // Переконайтеся, що у жаби тег "Frog"
        {
            manager.UpdateFlyCount();
            //other.GetComponent<FrogJump>().CollectFly();
            Destroy(gameObject);
        }
    }
}
