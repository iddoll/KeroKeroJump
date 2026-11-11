using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Level; // твої enum'и (LilyPadType, LilyPadDirection)

[DefaultExecutionOrder(-100)] // щоб генератор відпрацював раніше ніж FrogJump.Start
public class LevelGenerator : MonoBehaviour
{
    [Header("Directional Prefabs (intact)")]
    public GameObject upLilyPadPrefab;
    public GameObject downLilyPadPrefab;
    public GameObject leftLilyPadPrefab;
    public GameObject rightLilyPadPrefab;

    [Header("Directional Crash Prefabs (optional - pre-broken visuals)")]
    public GameObject upCrashPrefab;
    public GameObject downCrashPrefab;
    public GameObject leftCrashPrefab;
    public GameObject rightCrashPrefab;

    [Header("Special Prefabs")]
    public GameObject breakableLilyPadPrefab; // optional generic breakable
    public GameObject winPadPrefab;
    public GameObject startPadPrefab;

    [Header("Frog")]
    public GameObject frogPrefab;

    [Header("Mode")]
    public bool useGridMode = true; // true = grid via LevelConfig.Generate; false = count mode
    [Tooltip("Grid mode settings")]
    public int levelToGenerate = 1;
    public int rows = 6;
    public int cols = 6;
    public float cellSize = 1f;

    [Tooltip("Count mode settings")]
    public int lilyPadCount = 20;
    public float fieldWidth = 10f;
    public float spacingY = 1.2f;

    [Header("Common")]
    public Transform spawnParent;
    public Sprite swampSprite;
    public Sprite crackedLilySprite;
    public Sprite brokenLilySprite;
    public GameObject hudPrefab;
    public GameObject flyPrefab;

    GameObject hudInstance;
    float playHalfW;
    float playHalfH;

    void Awake()
    {
        if (spawnParent == null) spawnParent = transform;
        GenerateAndBuildLevel();
    }

    [ContextMenu("Generate Level (editor)")]
    public void GenerateAndBuildLevel()
    {
        WinMenuPanel.Hide();
        LilyPad.HideMissedNotice();
        ClearCurrentLevel();

        if (useGridMode)
        {
            if (LevelLaunch.HasSelection)
                levelToGenerate = LevelLaunch.Level;

            FrameCamera();
            FitGridToCamera();
            ResolvePadSprites();

            var levelData = LevelConfig.Generate(levelToGenerate, rows, cols);
            if (levelData == null) return;

            Vector3 origin = transform.position + new Vector3(-((levelData.cols - 1) * cellSize) / 2f, -((levelData.rows - 1) * cellSize) / 2f, 0f);
            GameObject startPadInstance = null;

            for (int y = 0; y < levelData.rows; y++)
            {
                for (int x = 0; x < levelData.cols; x++)
                {
                    if (levelData.gridMask[y, x] == 1)
                    {
                        Vector3 pos = origin + new Vector3(x * cellSize, y * cellSize, 0f);

                        LilyPadType type = levelData.lilyPadTypes[y, x];
                        LilyPadDirection dir = levelData.lilyPadDirections[y, x];

                        GameObject prefab = GetPrefab(type, dir, false);
                        if (prefab == null) prefab = upLilyPadPrefab;

                        var spawned = Instantiate(prefab, pos, Quaternion.identity, spawnParent);
                        spawned.name = $"Lily_{x}_{y}";

                        var lp = spawned.GetComponent<LilyPad>();
                        if (lp != null)
                        {
                            lp.SetProperties(type, dir);
                            lp.Prepare(crackedLilySprite, brokenLilySprite);
                        }

                        if (type == LilyPadType.StartPad) startPadInstance = spawned;
                    }
                }
            }

            SpawnFrogOn(startPadInstance);
            SpawnSwamp(levelData);
            SpawnFlies();
            EnsureHud();
            WakeFliesManager();
            AlignHud();
            FliesManager.PushText();
        }
        else // count mode
        {
            GameObject startPadInstance = null;
            for (int i = 0; i < lilyPadCount; i++)
            {
                float xPos = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                float yPos = i * spacingY;
                Vector3 pos = transform.position + new Vector3(xPos, yPos, 0f);

                // Останній - WinPad
                LilyPadType type = (i == lilyPadCount - 1) ? LilyPadType.WinPad : LilyPadType.Normal;

                // Випадковий напрямок (Up/Down/Left/Right)
                LilyPadDirection dir = (LilyPadDirection)Random.Range(3, 7); // 3..6 у твоєму enum

                GameObject prefab = GetPrefab(type, dir, false);
                if (prefab == null) prefab = upLilyPadPrefab;

                var spawned = Instantiate(prefab, pos, Quaternion.identity, spawnParent);
                spawned.name = $"Lily_{i}";

                var lp = spawned.GetComponent<LilyPad>();
                if (lp != null) lp.SetProperties(type, dir);

                if (type == LilyPadType.StartPad) startPadInstance = spawned;
            }

            SpawnFrogOn(startPadInstance);
        }
    }

    private void SpawnFrogOn(GameObject startPadInstance)
    {
        if (frogPrefab == null) return;

        Transform lilyCentre = null;
        if (startPadInstance != null) lilyCentre = startPadInstance.transform.Find("LilyCentre");

        if (lilyCentre != null)
        {
            // видаляємо стару жабу якщо є
            var old = GameObject.FindWithTag("Frog");
            if (old != null) DestroyImmediate(old);

            Instantiate(frogPrefab, lilyCentre.position, Quaternion.identity);
        }
        else
        {
            // якщо немає стартової плитки - просто не спавнимо жабу (або спавн на center)
            Debug.LogWarning("StartPad or LilyCentre not found. Frog not spawned automatically.");
        }
    }

    private GameObject GetPrefab(LilyPadType type, LilyPadDirection dir, bool spawnBrokenVariant)
    {
        if (type == LilyPadType.StartPad) return startPadPrefab;
        if (type == LilyPadType.WinPad) return winPadPrefab;
        if (type == LilyPadType.Breakable || type == LilyPadType.Gate)
        {
            switch (dir)
            {
                case LilyPadDirection.Horizontal:
                    return leftCrashPrefab != null ? leftCrashPrefab : leftLilyPadPrefab;
                default:
                    return upCrashPrefab != null ? upCrashPrefab : upLilyPadPrefab;
            }
        }

        // Для звичайних кувшинок, вибираємо префаб на основі напрямку.
        // Оскільки у вас немає окремих префабів для Horizontal, Vertical, All,
        // ми будемо вибирати один з наявних, і дозволяти аніматору робити решту.
        switch (dir)
        {
            case LilyPadDirection.Vertical:
                return upLilyPadPrefab; // Наприклад, використовуємо префаб 'вгору'
            case LilyPadDirection.Horizontal:
                return leftLilyPadPrefab; // Наприклад, використовуємо префаб 'вліво'
            case LilyPadDirection.All:
                return upLilyPadPrefab; // Використовуємо один з наявних, аніматору потрібно буде керувати цим
            default:
                return upLilyPadPrefab; // Запасний варіант
        }
    }


    private void ClearCurrentLevel()
    {
        // видаляємо дітей LevelManager (кувшинки)
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            toDestroy.Add(child.gameObject);
        }
        for (int i = 0; i < toDestroy.Count; i++)
        {
            #if UNITY_EDITOR
            DestroyImmediate(toDestroy[i]);
            #else
            Destroy(toDestroy[i]);
            #endif
        }

        // видалити стару жабу
        var frog = GameObject.FindWithTag("Frog");
        if (frog != null) DestroyImmediate(frog);
    }

    void FrameCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
    }

    void ResolvePadSprites()
    {
#if UNITY_EDITOR
        if (crackedLilySprite == null)
            crackedLilySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Cracked_Lily.png");
        if (brokenLilySprite == null)
            brokenLilySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Brocke_Lily.png");
#endif
    }

    void FitGridToCamera()
    {
        cellSize = 2f;
        Camera cam = Camera.main;
        float ortho = cam != null ? cam.orthographicSize : 7f;
        float aspect = cam != null && cam.aspect > 0.2f ? cam.aspect : 16f / 9f;
        float playH = ortho * 2f * 0.62f;
        float playW = ortho * 2f * aspect * 0.75f;
        rows = Mathf.Clamp(Mathf.FloorToInt(playH / cellSize) + 1, 4, 6);
        cols = Mathf.Clamp(Mathf.FloorToInt(playW / cellSize) + 1, 6, 9);
    }

    void SpawnSwamp(LevelConfig.LevelData data)
    {
        Sprite sprite = swampSprite;
#if UNITY_EDITOR
        if (sprite == null)
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Swamp.png");
#endif
        Camera cam = Camera.main;
        if (sprite == null || cam == null)
            return;

        var swamp = new GameObject("Swamp");
        swamp.transform.SetParent(spawnParent, false);
        var renderer = swamp.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -10;
        FitSwamp(cam, sprite, swamp.transform);

        if (swampRoutine != null)
            StopCoroutine(swampRoutine);
        swampRoutine = StartCoroutine(RefitSwampNextFrame(sprite));
    }

    Coroutine swampRoutine;

    IEnumerator RefitSwampNextFrame(Sprite sprite)
    {
        for (int i = 0; i < 8; i++)
        {
            yield return null;
            Camera cam = Camera.main;
            Transform swamp = spawnParent != null ? spawnParent.Find("Swamp") : null;
            if (cam == null || swamp == null || sprite == null)
                yield break;

            FitSwamp(cam, sprite, swamp);
            AlignHud();
        }
    }

    void FitSwamp(Camera cam, Sprite sprite, Transform swamp)
    {
        var renderer = swamp.GetComponent<SpriteRenderer>();
        float viewH = cam.orthographicSize * 2f;
        float viewW = viewH * Mathf.Max(cam.aspect, 0.2f);
        float ppu = Mathf.Max(1f, sprite.pixelsPerUnit);
        Rect rect = sprite.rect;
        float width = Mathf.Max(0.01f, rect.width / ppu);
        float height = Mathf.Max(0.01f, rect.height / ppu);
        swamp.localScale = new Vector3(viewW / width, viewH / height, 1f);

        if (renderer != null)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                Bounds bounds = renderer.bounds;
                if (bounds.size.x < 0.01f || bounds.size.y < 0.01f)
                    break;

                Vector3 scale = swamp.localScale;
                swamp.localScale = new Vector3(
                    scale.x * viewW / bounds.size.x,
                    scale.y * viewH / bounds.size.y,
                    1f);

                bounds = renderer.bounds;
                float dx = cam.transform.position.x - bounds.center.x;
                float dy = cam.transform.position.y + cam.orthographicSize - bounds.max.y;
                Vector3 placed = swamp.position;
                swamp.position = new Vector3(placed.x + dx, placed.y + dy, 2f);
            }
        }

        playHalfW = viewW * 0.5f;
        playHalfH = cam.orthographicSize;
    }

    void EnsureHud()
    {
        if (hudInstance != null)
            return;

        GameObject prefab = hudPrefab;
#if UNITY_EDITOR
        if (prefab == null)
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Canvas.prefab");
#endif
        if (prefab == null)
            return;

        hudInstance = Instantiate(prefab);
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var events = new GameObject("EventSystem");
            events.AddComponent<UnityEngine.EventSystems.EventSystem>();
            events.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Canvas.ForceUpdateCanvases();
    }

    void AlignHud()
    {
        if (hudInstance == null)
            return;

        Camera cam = Camera.main;
        var canvas = hudInstance.GetComponent<RectTransform>();
        if (cam == null || canvas == null || playHalfW < 0.1f)
            return;

        Vector3 camPos = cam.transform.position;
        PlaceHud(canvas, cam, camPos + new Vector3(-playHalfW, playHalfH, 0f), "counter", new Vector2(18f, -16f), new Vector2(0f, 1f));
        PlaceHud(canvas, cam, camPos + new Vector3(playHalfW, playHalfH, 0f), "pause", new Vector2(-18f, -16f), new Vector2(1f, 1f));

        FliesManager.PushText();
    }

    void WakeFliesManager()
    {
        var manager = FindAnyObjectByType<FliesManager>(FindObjectsInactive.Include);
        if (manager != null && !manager.gameObject.activeSelf)
            manager.gameObject.SetActive(true);
    }

    void PlaceHud(RectTransform canvas, Camera cam, Vector3 world, string which, Vector2 inset, Vector2 pivot)
    {
        Vector2 screen = cam.WorldToScreenPoint(world);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out Vector2 canvasPoint))
            return;

        RectTransform target = null;
        if (which == "counter")
        {
            var counter = hudInstance.GetComponentInChildren<FlyCountText>(true);
            if (counter != null)
                target = counter.transform.parent as RectTransform;
        }
        else
        {
            Transform pause = FindNamed(hudInstance.transform, "PauseButton");
            if (pause != null)
                target = pause as RectTransform;
        }

        if (target == null || target.parent == null)
            return;

        Vector3 local = target.parent.InverseTransformPoint(canvas.TransformPoint(canvasPoint));
        Vector3 parentScale = target.parent.localScale;
        float sx = Mathf.Abs(parentScale.x) > 0.01f ? parentScale.x : 1f;
        float sy = Mathf.Abs(parentScale.y) > 0.01f ? parentScale.y : 1f;
        target.anchorMin = new Vector2(0.5f, 0.5f);
        target.anchorMax = new Vector2(0.5f, 0.5f);
        target.pivot = pivot;
        target.anchoredPosition = new Vector2(local.x + inset.x / sx, local.y + inset.y / sy);
    }

    static Transform FindNamed(Transform root, string objectName)
    {
        if (root.name == objectName)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindNamed(root.GetChild(i), objectName);
            if (found != null)
                return found;
        }
        return null;
    }

    void SpawnFlies()
    {
        GameObject prefab = flyPrefab;
#if UNITY_EDITOR
        if (prefab == null)
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Myha.prefab");
#endif
        if (prefab == null)
            return;

        var pads = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in spawnParent)
        {
            var pad = child.GetComponent<LilyPad>();
            if (pad == null)
                continue;
            if (pad.lilyPadType == LilyPadType.StartPad || pad.lilyPadType == LilyPadType.WinPad || pad.lilyPadType == LilyPadType.Trap)
                continue;

            Transform centre = child.Find("LilyCentre");
            pads.Add(centre != null ? centre : child);
        }

        int want = Mathf.Min(3, pads.Count);
        int spawned = 0;
        for (int i = 0; i < want; i++)
        {
            int index = want == 1 ? 0 : Mathf.RoundToInt(i * (pads.Count - 1) / (float)(want - 1));
            Vector3 spot = pads[index].position + new Vector3(0f, 0.05f, -0.2f);
            var fly = Instantiate(prefab, spot, Quaternion.identity, spawnParent);
            fly.name = "Fly";
            var sprite = fly.GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.sortingOrder = 5;
            spawned++;
        }

        FliesManager.eaten = 0;
        FliesManager.count = spawned;
        var counter = FindAnyObjectByType<FlyCountText>();
        if (counter != null)
            counter.UpdateText();
    }
}
