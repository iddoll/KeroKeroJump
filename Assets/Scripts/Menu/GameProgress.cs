using UnityEngine;

public static class GameProgress
{
    const string Key = "KeroReachedLevel";

    public static int Reached => Mathf.Max(1, PlayerPrefs.GetInt(Key, 1));

    public static void Complete(int level)
    {
        if (level != Reached)
            return;

        PlayerPrefs.SetInt(Key, level + 1);
        PlayerPrefs.Save();
    }
}
