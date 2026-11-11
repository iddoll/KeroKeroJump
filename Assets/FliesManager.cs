using UnityEngine;

public class FliesManager : MonoBehaviour
{
    public static int count;
    public static int eaten;

    AudioClip clipSpawnFly;

    private void Awake()
    {
        count = FindObjectsByType<Fly>(FindObjectsSortMode.None).Length;
    }
    private void Start()
    {
        PushText();
    }

    public void UpdateFlyCount()
    {
        RegisterEaten();
    }

    public static void RegisterEaten()
    {
        eaten++;
        PushText();
    }

    public static void PushText()
    {
        FlyCountText[] counters = FindObjectsByType<FlyCountText>(FindObjectsSortMode.None);
        for (int i = 0; i < counters.Length; i++)
            counters[i].UpdateText();
    }

    public static bool AllCollected => count <= 0 || eaten >= count;

}
