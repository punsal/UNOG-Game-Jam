using UnityEngine;

/// <summary>
/// Marks each section start with a quantized scanline sweep in a per-section
/// accent color (cyan family early, purple-shifted late — yellow stays
/// reserved for the relic). No scene load, no input interruption; death and
/// restart interrupt safely through ScreenFXOverlay.ResetRun.
/// </summary>
public class SectionTransitionVFX : MonoBehaviour
{
    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private ScreenFXOverlay overlay;
    [SerializeField] private Color[] sectionAccents =
    {
        new Color(0.3f, 0.9f, 1f),    // cyan
        new Color(0.3f, 0.78f, 0.95f),
        new Color(0.35f, 0.65f, 0.95f),
        new Color(0.45f, 0.5f, 0.95f),
        new Color(0.55f, 0.4f, 0.92f), // purple-shifted
        new Color(0.61f, 0.35f, 0.9f),
    };

    private StageDirector stageDirector;

    private void Start()
    {
        stageDirector = FindFirstObjectByType<StageDirector>(FindObjectsInactive.Include);
        if (stageDirector != null)
        {
            stageDirector.StageChanged += HandleStageChanged;
        }
        else
        {
            Debug.LogWarning("SectionTransitionVFX: no StageDirector found; transitions disabled.", this);
        }
    }

    private void OnDestroy()
    {
        if (stageDirector != null)
        {
            stageDirector.StageChanged -= HandleStageChanged;
        }
    }

    public void HandleStageChanged(int index)
    {
        if (overlay == null)
        {
            return;
        }
        Color accent = sectionAccents.Length > 0
            ? sectionAccents[Mathf.Clamp(index, 0, sectionAccents.Length - 1)]
            : (settings != null ? settings.Cyan : LastLightVFXPalette.Cyan);
        overlay.PlayScanline(accent);
    }
}
