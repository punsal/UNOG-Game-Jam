using UnityEngine;

/// <summary>
/// Drives the carried light core visuals (RelicCore + GlowInner) from RelicLight.
/// Pulses in discrete pixel steps; glow base size shrinks permanently as light is lost.
/// </summary>
public class LightCoreView : MonoBehaviour
{
    private enum LightState { Idle, Low, Critical }

    [SerializeField] private RelicLight relicLight;
    [SerializeField] private SpriteRenderer coreRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;

    [Header("State thresholds (light %)")]
    [SerializeField] private float lowThreshold = 50f;
    [SerializeField] private float criticalThreshold = 20f;

    [Header("Pulse step duration per state (seconds)")]
    [SerializeField] private float idleStepDuration = 0.6f;
    [SerializeField] private float lowStepDuration = 0.35f;
    [SerializeField] private float criticalStepDuration = 0.15f;

    [Header("Glow scale (quantized to pulseScaleStep)")]
    [SerializeField] private float glowScaleAtFullLight = 1f;
    [SerializeField] private float glowScaleAtZeroLight = 0.375f;
    [SerializeField] private float pulseScaleStep = 0.125f;

    [Header("Glow colors")]
    [SerializeField] private Color glowIdleColor = new Color(1f, 0.92f, 0.35f);
    [SerializeField] private Color glowLowColor = new Color(1f, 0.76f, 0.25f);
    [SerializeField] private Color glowCriticalColor = new Color(1f, 0.55f, 0.18f);

    [Header("Core colors")]
    [SerializeField] private Color coreIdleColor = Color.white;
    [SerializeField] private Color coreLowColor = new Color(0.95f, 0.88f, 0.75f);
    [SerializeField] private Color coreCriticalColor = new Color(0.85f, 0.65f, 0.45f);

    private LightState state;
    private float baseGlowScale;
    private int lastTick = -1;

    private void OnEnable()
    {
        if (relicLight == null)
        {
            relicLight = GetComponentInParent<RelicLight>();
        }

        if (relicLight != null)
        {
            relicLight.LightChanged += ApplyLight;
            ApplyLight(relicLight.CurrentLight);
        }
        else
        {
            ApplyLight(100f);
        }
    }

    private void OnDisable()
    {
        if (relicLight != null)
        {
            relicLight.LightChanged -= ApplyLight;
        }
    }

    private void Update()
    {
        Tick(Time.time);
    }

    // Public so state can be driven and verified without Play Mode time.
    public void ApplyLight(float light)
    {
        state = light <= criticalThreshold ? LightState.Critical
              : light <= lowThreshold ? LightState.Low
              : LightState.Idle;

        // Persistent consequence: glow footprint shrinks with lost light and stays shrunk.
        float raw = Mathf.Lerp(glowScaleAtZeroLight, glowScaleAtFullLight, Mathf.Clamp01(light / 100f));
        baseGlowScale = Mathf.Max(pulseScaleStep, Mathf.Round(raw / pulseScaleStep) * pulseScaleStep);

        if (glowRenderer != null)
        {
            glowRenderer.color = state == LightState.Critical ? glowCriticalColor
                               : state == LightState.Low ? glowLowColor
                               : glowIdleColor;
        }

        if (coreRenderer != null)
        {
            coreRenderer.color = state == LightState.Critical ? coreCriticalColor
                               : state == LightState.Low ? coreLowColor
                               : coreIdleColor;
        }

        lastTick = -1;
        Tick(Time.time);
    }

    public void Tick(float time)
    {
        float stepDuration = state == LightState.Critical ? criticalStepDuration
                           : state == LightState.Low ? lowStepDuration
                           : idleStepDuration;

        int tick = (int)(time / Mathf.Max(0.01f, stepDuration));
        if (tick == lastTick)
        {
            return;
        }

        lastTick = tick;
        bool expanded = (tick & 1) == 0;

        if (glowRenderer != null)
        {
            float scale = expanded ? baseGlowScale + pulseScaleStep : baseGlowScale;
            glowRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        if (coreRenderer != null && state == LightState.Critical)
        {
            // Hard two-frame flicker while the light is nearly out.
            coreRenderer.color = expanded ? coreIdleColor : coreCriticalColor;
        }
    }
}
