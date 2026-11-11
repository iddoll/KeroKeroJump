#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Game.Level;

public static class LevelExporter
{
    [MenuItem("KeroKero/Export Current Scene LilyPads to LevelData")]
    static void ExportCurrentScene()
    {
        var lilies = UnityEngine.Object.FindObjectsOfType<LilyPad>();
        if (lilies == null || lilies.Length == 0)
        {
            EditorUtility.DisplayDialog("Export", "No LilyPad objects found in the scene.", "OK");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        LevelDataSO data = ScriptableObject.CreateInstance<LevelDataSO>();
        data.lilies = new List<LevelDataSO.LilyEntry>();

        foreach (var lp in lilies)
        {
            var entry = new LevelDataSO.LilyEntry();
            entry.position = lp.transform.position;
            entry.lilyType = lp.lilyPadType;
            entry.lilyDirection = lp.lilyPadDirection;
            entry.breakTime = lp.breakTime;
            data.lilies.Add(entry);
        }

        System.IO.Directory.CreateDirectory("Assets/LevelsData");
        string path = $"Assets/LevelsData/{sceneName}_LevelData.asset";
        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Export", $"Saved LevelData to {path} ({data.lilies.Count} items).", "OK");
        Selection.activeObject = data;
    }

    [MenuItem("KeroKero/Delete All LilyPads In Scene")]
    static void DeleteAllLilyPads()
    {
        var lilies = UnityEngine.Object.FindObjectsOfType<LilyPad>();
        if (lilies == null || lilies.Length == 0)
        {
            EditorUtility.DisplayDialog("Delete LilyPads", "No LilyPads found.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Delete LilyPads", $"Delete {lilies.Length} LilyPads from the scene? Make backup first.", "Delete", "Cancel"))
            return;

        foreach (var lp in lilies)
        {
            GameObject.DestroyImmediate(lp.gameObject);
        }

        EditorUtility.DisplayDialog("Delete LilyPads", "Deleted.", "OK");
    }
}
#endif
