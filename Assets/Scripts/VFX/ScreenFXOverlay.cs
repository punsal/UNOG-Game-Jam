using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Central coordinator for the few allowed screen-scale overlays: damage edge
/// frame, section scanline, world degradation tint and the dark closure.
/// All images are non-blocking (raycastTarget off) and never stack opaquely.
/// Lives on the HUD canvas prefab.
/// </summary>
public class ScreenFXOverlay : MonoBehaviour, IResettable
{
    public static ScreenFXOverlay Instance { get; private set; }

    [SerializeField] private LastLightVFXSettings settings;

    [Header("Damage edge frame (pink, under 0.12s)")]
    [SerializeField] private Image[] damageEdges;
    [SerializeField] private float damageEdgeDuration = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float damageEdgeAlpha = 0.55f;

    [Header("Section scanline")]
    [SerializeField] private Image scanline;
    [SerializeField] private float scanlineDuration = 0.3f;
    [SerializeField] private int scanlineSteps = 12;

    [Header("World degradation tint (kept subtle: hazards must stay readable)")]
    [SerializeField] private Image degradationTint;
    [SerializeField] private float[] degradationAlphas = { 0f, 0.05f, 0.1f, 0.16f };

    [Header("Dark closure / final tint")]
    [SerializeField] private Image fullTint;

    private float damageTimer;
    private float scanlineTimer;
    private float closureTimer;
    private float closureDuration;
    private float closureHoldAlpha;
    private float canvasHeight;

    private void Awake()
    {
        var rect = transform as RectTransform;
        canvasHeight = rect != null ? rect.rect.height : 640f;
    }

    private void OnEnable()
    {
        Instance = this;
        AllOff();
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlayDamageEdge()
    {
        damageTimer = damageEdgeDuration;
    }

    public void PlayScanline(Color color)
    {
        if (scanline == null)
        {
            return;
        }
        color.a = 0.8f;
        scanline.color = color;
        scanlineTimer = scanlineDuration;
        scanline.enabled = true;
    }

    public void SetDegradationLevel(int level)
    {
        if (degradationTint == null || degradationAlphas.Length == 0)
        {
            return;
        }
        int index = Mathf.Clamp(level, 0, degradationAlphas.Length - 1);
        Color c = settings != null ? settings.Navy : LastLightVFXPalette.Navy;
        c.a = degradationAlphas[index];
        degradationTint.color = c;
        degradationTint.enabled = c.a > 0f;
    }

    /// <summary>Steps the full tint up to holdAlpha over duration and keeps it there until reset.</summary>
    public void PlayDarkClosure(float duration, float holdAlpha)
    {
        closureDuration = Mathf.Max(0.05f, duration);
        closureTimer = closureDuration;
        closureHoldAlpha = Mathf.Clamp01(holdAlpha);
    }

    /// <summary>Immediate persistent tint (e.g. bad-final darkness). Reset clears it.</summary>
    public void SetFullTint(Color color, float alpha)
    {
        if (fullTint == null)
        {
            return;
        }
        color.a = Mathf.Clamp01(alpha);
        fullTint.color = color;
        fullTint.enabled = color.a > 0f;
    }

    private void Update()
    {
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
            // Two-step decay: strong for the first half, half-alpha after.
            float alpha = damageTimer <= 0f ? 0f
                        : damageTimer > damageEdgeDuration * 0.5f ? damageEdgeAlpha
                        : damageEdgeAlpha * 0.5f;
            Color c = settings != null ? settings.Pink : LastLightVFXPalette.Pink;
            c.a = alpha;
            for (int i = 0; i < damageEdges.Length; i++)
            {
                if (damageEdges[i] != null)
                {
                    damageEdges[i].color = c;
                    damageEdges[i].enabled = alpha > 0f;
                }
            }
        }

        if (scanlineTimer > 0f && scanline != null)
        {
            scanlineTimer -= Time.deltaTime;
            if (scanlineTimer <= 0f)
            {
                scanline.enabled = false;
            }
            else
            {
                // Quantized top-to-bottom sweep.
                float progress = 1f - scanlineTimer / scanlineDuration;
                int step = Mathf.FloorToInt(progress * scanlineSteps);
                float y = -(canvasHeight / scanlineSteps) * step;
                var rt = (RectTransform)scanline.transform;
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            }
        }

        if (closureTimer > 0f && fullTint != null)
        {
            closureTimer -= Time.deltaTime;
            float progress = 1f - Mathf.Max(0f, closureTimer) / closureDuration;
            // Four hard steps up to the hold alpha.
            float alpha = Mathf.Floor(progress * 4f) / 4f * closureHoldAlpha;
            Color c = settings != null ? settings.Navy : LastLightVFXPalette.Navy;
            c.a = closureTimer <= 0f ? closureHoldAlpha : alpha;
            fullTint.color = c;
            fullTint.enabled = c.a > 0f;
        }
    }

    private void AllOff()
    {
        damageTimer = 0f;
        scanlineTimer = 0f;
        closureTimer = 0f;
        for (int i = 0; i < (damageEdges != null ? damageEdges.Length : 0); i++)
        {
            if (damageEdges[i] != null)
            {
                damageEdges[i].enabled = false;
            }
        }
        if (scanline != null)
        {
            scanline.enabled = false;
        }
        if (degradationTint != null)
        {
            degradationTint.enabled = false;
        }
        if (fullTint != null)
        {
            fullTint.enabled = false;
        }
    }

    public void ResetRun()
    {
        AllOff();
    }
}
