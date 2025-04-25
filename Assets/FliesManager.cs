using UnityEngine;

public class FliesManager : MonoBehaviour
{
    public static int count;
    public static int eaten;

    FlyCountText textDisplay;
    AudioClip clipSpawnFly;

    private void Awake()
    {
        textDisplay = FindAnyObjectByType<FlyCountText>();
        count = FindObjectsByType<Fly>(FindObjectsSortMode.None).Length;
    }
    private void Start()
    {
        eaten = 0;
        textDisplay.UpdateText();
    }

    public void UpdateFlyCount()
    {
        eaten++;
        textDisplay.UpdateText();
    }

}
