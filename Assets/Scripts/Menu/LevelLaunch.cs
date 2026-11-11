using UnityEngine;

public static class LevelLaunch
{
    public static bool HasSelection { get; private set; }
    public static int Level { get; private set; } = 1;

    public static void Select(int level)
    {
        HasSelection = true;
        Level = Mathf.Max(1, level);
    }
}
