using UnityEngine;

/// <summary>
/// Palette constants used as fallbacks when no settings asset is assigned.
/// Values mirror the VFX art direction (see Docs/Last_Light_Technical_Art_Baseline.pdf).
/// </summary>
public static class LastLightVFXPalette
{
    public static readonly Color Navy = new Color(0.039f, 0.055f, 0.102f);
    public static readonly Color Yellow = new Color(1f, 0.91f, 0.33f);
    public static readonly Color White = Color.white;
    public static readonly Color Pink = new Color(1f, 0.25f, 0.44f);
    public static readonly Color Red = new Color(0.9f, 0.15f, 0.2f);
    public static readonly Color Cyan = new Color(0.3f, 0.9f, 1f);
    public static readonly Color Purple = new Color(0.61f, 0.35f, 0.9f);
    public static readonly Color Ember = new Color(1f, 0.55f, 0.18f);
}

[CreateAssetMenu(fileName = "LastLightVFXSettings", menuName = "Last Light/VFX Settings")]
public class LastLightVFXSettings : ScriptableObject
{
    [Header("Pixel grid")]
    [SerializeField] private float pixelsPerUnit = 16f;

    [Header("Relic light state thresholds (%)")]
    [SerializeField] private float lowThreshold = 50f;
    [SerializeField] private float criticalThreshold = 20f;

    [Header("Accessibility")]
    [Tooltip("Global motion scale. Set below 1 (e.g. 0.5) for reduced motion: camera impulses, fragment travel and wave movement shrink; telegraph colors/shapes are unaffected.")]
    [SerializeField] [Range(0.25f, 1f)] private float motionScale = 1f;

    [Header("Palette")]
    [SerializeField] private Color navy = new Color(0.039f, 0.055f, 0.102f);
    [SerializeField] private Color yellow = new Color(1f, 0.91f, 0.33f);
    [SerializeField] private Color pink = new Color(1f, 0.25f, 0.44f);
    [SerializeField] private Color red = new Color(0.9f, 0.15f, 0.2f);
    [SerializeField] private Color cyan = new Color(0.3f, 0.9f, 1f);
    [SerializeField] private Color purple = new Color(0.61f, 0.35f, 0.9f);
    [SerializeField] private Color ember = new Color(1f, 0.55f, 0.18f);

    public float PixelsPerUnit => pixelsPerUnit;
    public float LowThreshold => lowThreshold;
    public float CriticalThreshold => criticalThreshold;
    public float MotionScale => motionScale;
    public Color Navy => navy;
    public Color Yellow => yellow;
    public Color Pink => pink;
    public Color Red => red;
    public Color Cyan => cyan;
    public Color Purple => purple;
    public Color Ember => ember;

    /// <summary>Runtime hook for a future settings menu.</summary>
    public void SetMotionScale(float scale)
    {
        motionScale = Mathf.Clamp(scale, 0.25f, 1f);
    }
}
