using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hard-edged vision loss: navy edge panels with dithered inner borders close
/// in as sight is sacrificed. Quantized severity levels, asymmetrical insets,
/// slight stepped instability at the severe level. Never blocks input and
/// always preserves a readable center area.
/// </summary>
public class PixelVignetteView : MonoBehaviour, IResettable
{
    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;
    [SerializeField] private RectTransform topPanel;
    [SerializeField] private RectTransform bottomPanel;

    [Header("Quantized insets per severity level (reference 360x640)")]
    [SerializeField] private float[] sideInsets = { 0f, 20f, 40f, 64f };
    [SerializeField] private float[] vertInsets = { 0f, 28f, 56f, 88f };
    [Tooltip("Extra pixels on the right/top so the boundary reads asymmetrical.")]
    [SerializeField] private float asymmetry = 8f;

    [Header("Severe-state instability")]
    [SerializeField] private float jitterInterval = 0.2f;
    [SerializeField] private float jitterPixels = 4f;

    private int level;
    private float jitterTimer;
    private bool jitterFlip;

    private void OnEnable()
    {
        ApplyLevel();
    }

    /// <summary>Drive from the modifier stack: 1 = full sight, 0.65 = capped minimum.</summary>
    public void SetSightMultiplier(float multiplier)
    {
        float severity = Mathf.InverseLerp(1f, 0.65f, multiplier);
        SetSeverity(severity);
    }

    /// <summary>Severity 0..1, quantized into discrete visual levels.</summary>
    public void SetSeverity(float severity)
    {
        int newLevel = severity <= 0.01f ? 0
                     : severity < 0.4f ? 1
                     : severity < 0.8f ? 2
                     : 3;
        if (newLevel != level)
        {
            level = newLevel;
            ApplyLevel();
        }
    }

    public int Level => level;

    private void Update()
    {
        if (level < 3)
        {
            return;
        }

        jitterTimer += Time.deltaTime;
        if (jitterTimer >= jitterInterval)
        {
            jitterTimer = 0f;
            jitterFlip = !jitterFlip;
            ApplyLevel();
        }
    }

    private void ApplyLevel()
    {
        float side = sideInsets[Mathf.Clamp(level, 0, sideInsets.Length - 1)];
        float vert = vertInsets[Mathf.Clamp(level, 0, vertInsets.Length - 1)];
        float jitter = level >= 3 && jitterFlip ? jitterPixels : 0f;

        SetPanel(leftPanel, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(side + jitter, 0f), true);
        SetPanel(rightPanel, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(side + asymmetry, 0f), false);
        SetPanel(topPanel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, vert + asymmetry * 0.5f), false);
        SetPanel(bottomPanel, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, vert - jitter), true);
    }

    private void SetPanel(RectTransform panel, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, bool positive)
    {
        if (panel == null)
        {
            return;
        }

        bool visible = level > 0 && (size.x > 0f || size.y > 0f);
        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.enabled = visible;
        }
        // Dither strip child follows its parent panel.
        if (panel.childCount > 0)
        {
            var strip = panel.GetChild(0).GetComponent<Image>();
            if (strip != null)
            {
                strip.enabled = visible;
            }
        }
        if (!visible)
        {
            return;
        }

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
        if (size.x > 0f)
        {
            panel.sizeDelta = new Vector2(Mathf.Max(0f, size.x), 0f);
            panel.pivot = new Vector2(positive ? 0f : 1f, 0.5f);
        }
        else
        {
            panel.sizeDelta = new Vector2(0f, Mathf.Max(0f, size.y));
            panel.pivot = new Vector2(0.5f, positive ? 0f : 1f);
        }
        panel.anchoredPosition = Vector2.zero;
    }

    public void ResetRun()
    {
        level = 0;
        jitterFlip = false;
        jitterTimer = 0f;
        ApplyLevel();
    }
}
