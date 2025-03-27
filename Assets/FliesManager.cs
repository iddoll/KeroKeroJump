using UnityEngine;

public class FliesManager : MonoBehaviour
{
    public static int count;

    FlyCountText textDisplay;
    AudioClip clipSpawnFly;

    private void Awake()
    {
        textDisplay = FindAnyObjectByType<FlyCountText>();
        count = FindObjectsByType<Fly>(FindObjectsSortMode.None).Length;
    }
    private void Start()
    {
        UpdateFlyCount();
    }

    public void UpdateFlyCount()
    {
        textDisplay.UpdateText();
    }

}
