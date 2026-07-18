using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sensory HUD loss presentation: the element flickers purple for two hard
/// ticks, disappears, and leaves a faint scar frame for the rest of the run.
/// HUD objects are never destroyed and layout is preserved (only the Graphic
/// is disabled). NOTE: no gameplay cost currently removes HUD info — this is
/// an integration-ready hook (see VFX docs).
/// </summary>
public class HUDLossVFX : MonoBehaviour, IResettable
{
    public enum HUDElement { Health = 0, Light = 1, Stage = 2 }

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private Graphic[] elementGraphics;   // indexed by HUDElement
    [SerializeField] private Image[] scarFrames;          // indexed by HUDElement
    [SerializeField] private float flickerStep = 0.07f;
    [SerializeField] [Range(0f, 1f)] private float scarAlpha = 0.18f;

    private readonly bool[] lost = new bool[3];
    private int flickerTarget = -1;
    private int flickerTicksLeft;
    private float flickerTimer;
    private Color originalColor;

    public void LoseElement(HUDElement element)
    {
        int index = (int)element;
        if (index < 0 || index >= elementGraphics.Length || lost[index] || elementGraphics[index] == null)
        {
            return;
        }

        lost[index] = true;
        flickerTarget = index;
        flickerTicksLeft = 4; // two on/off pairs
        flickerTimer = flickerStep;
        originalColor = elementGraphics[index].color;
        elementGraphics[index].color = settings != null ? settings.Purple : LastLightVFXPalette.Purple;
    }

    public bool IsLost(HUDElement element)
    {
        return lost[(int)element];
    }

    private void Update()
    {
        if (flickerTarget < 0)
        {
            return;
        }

        flickerTimer -= Time.deltaTime;
        if (flickerTimer > 0f)
        {
            return;
        }

        flickerTimer = flickerStep;
        flickerTicksLeft--;
        var graphic = elementGraphics[flickerTarget];

        if (flickerTicksLeft <= 0)
        {
            // Information disappears; a faint scar remains.
            graphic.color = originalColor;
            graphic.enabled = false;
            var scar = flickerTarget < scarFrames.Length ? scarFrames[flickerTarget] : null;
            if (scar != null)
            {
                Color c = settings != null ? settings.Purple : LastLightVFXPalette.Purple;
                c.a = scarAlpha;
                scar.color = c;
                scar.enabled = true;
            }
            flickerTarget = -1;
        }
        else
        {
            // Hard purple/white alternation, one white corruption tick max.
            graphic.color = (flickerTicksLeft & 1) == 1
                ? Color.white
                : (settings != null ? settings.Purple : LastLightVFXPalette.Purple);
        }
    }

    public void ResetRun()
    {
        flickerTarget = -1;
        for (int i = 0; i < lost.Length; i++)
        {
            lost[i] = false;
            if (i < elementGraphics.Length && elementGraphics[i] != null)
            {
                elementGraphics[i].enabled = true;
            }
            if (i < scarFrames.Length && scarFrames[i] != null)
            {
                scarFrames[i].enabled = false;
            }
        }
    }
}
