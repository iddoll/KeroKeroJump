using UnityEngine;

public class Fly : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Frog")) // Переконайтеся, що у жаби тег "Frog"
        {
            other.GetComponent<FrogJump>().CollectFly();
            Destroy(gameObject);
        }
    }
}
