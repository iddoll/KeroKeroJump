using UnityEngine;

[ExecuteAlways]
public class FrogGlow : MonoBehaviour
{
    public float startScale = 2.4f;
    public float restScale = 1.18f;
    public float shrinkSeconds = 1.6f;
    public float blinkSpeed = 1.5f;
    [Range(0f, 1f)] public float blinkAmount = 0.55f;
    public Material silhouetteMaterial;

    SpriteRenderer glow;
    SpriteRenderer frog;
    Color tint = Color.white;
    float shrinkLeft;

    void OnEnable()
    {
        glow = GetComponent<SpriteRenderer>();
        frog = transform.parent != null ? transform.parent.GetComponent<SpriteRenderer>() : null;
        if (glow != null)
        {
            glow.sortingOrder = 8;
            if (silhouetteMaterial != null)
                glow.sharedMaterial = silhouetteMaterial;
            tint = glow.color;
            tint.r = 1f;
            tint.g = 1f;
            tint.b = 1f;
            if (tint.a < 0.05f)
                tint.a = 1f;
        }

        shrinkLeft = Application.isPlaying ? Mathf.Max(0.05f, shrinkSeconds) : 0f;
        ApplyScale(Application.isPlaying ? startScale : restScale);
        CopyFrame();
    }

    void OnValidate()
    {
        startScale = Mathf.Max(restScale, startScale);
        restScale = Mathf.Max(1.02f, restScale);
        if (glow == null)
            glow = GetComponent<SpriteRenderer>();
        if (glow != null)
            tint = new Color(1f, 1f, 1f, Mathf.Max(0.05f, glow.color.a));
    }

    void LateUpdate()
    {
        CopyFrame();
        if (!Application.isPlaying)
        {
            ApplyScale(restScale);
            SetAlpha(1f);
            return;
        }

        if (shrinkLeft > 0f)
        {
            shrinkLeft -= Time.deltaTime;
            float duration = Mathf.Max(0.05f, shrinkSeconds);
            float t = 1f - Mathf.Clamp01(shrinkLeft / duration);
            t = t * t * (3f - 2f * t);
            ApplyScale(Mathf.Lerp(startScale, restScale, t));
            SetAlpha(Mathf.Lerp(0.95f, 1f, t));
            return;
        }

        ApplyScale(restScale);
        float blink = 1f - blinkAmount * (0.5f + 0.5f * Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f));
        SetAlpha(blink);
    }

    void CopyFrame()
    {
        if (frog == null && transform.parent != null)
            frog = transform.parent.GetComponent<SpriteRenderer>();
        if (glow == null || frog == null)
            return;

        if (frog.sprite != null)
            glow.sprite = frog.sprite;
        glow.flipX = frog.flipX;
        glow.flipY = frog.flipY;
    }

    void ApplyScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    void SetAlpha(float multiplier)
    {
        if (glow == null)
            return;

        Color color = tint;
        color.a = Mathf.Clamp01(tint.a * multiplier);
        glow.color = color;
    }
}
