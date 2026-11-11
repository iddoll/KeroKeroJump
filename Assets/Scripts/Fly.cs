using UnityEngine;

public class Fly : MonoBehaviour
{
    bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Frog"))
            Collect();
    }

    public void Collect()
    {
        if (collected)
            return;

        collected = true;
        FliesManager.RegisterEaten();
        Destroy(gameObject);
    }
}
