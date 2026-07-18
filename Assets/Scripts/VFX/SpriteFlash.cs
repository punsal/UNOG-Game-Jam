using UnityEngine;

/// <summary>
/// Reusable stepped color flash for SpriteRenderers. Uses renderer.color only —
/// never materials. Do not target renderers whose color is driven by another
/// VFX component (e.g. RelicCore under LightCoreView).
/// </summary>
public class SpriteFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] targets;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float stepDuration = 0.05f;
    [SerializeField] private int steps = 2;

    private Color[] originals;
    private int remainingSteps;
    private float stepTimer;
    private bool flashing;

    private void Awake()
    {
        EnsureBuffer();
    }

    private void EnsureBuffer()
    {
        if (originals == null || originals.Length != (targets != null ? targets.Length : 0))
        {
            originals = new Color[targets != null ? targets.Length : 0];
        }
    }

    public void Flash()
    {
        Flash(flashColor, steps);
    }

    public void Flash(Color color, int flashSteps)
    {
        if (targets == null || targets.Length == 0)
        {
            return;
        }

        EnsureBuffer();
        if (!flashing)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                originals[i] = targets[i] != null ? targets[i].color : Color.white;
            }
        }

        flashing = true;
        remainingSteps = Mathf.Max(1, flashSteps);
        stepTimer = stepDuration;
        SetTargets(color);
    }

    private void Update()
    {
        if (!flashing)
        {
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f)
        {
            return;
        }

        remainingSteps--;
        if (remainingSteps <= 0)
        {
            Restore();
        }
        else
        {
            stepTimer = stepDuration;
            // Alternate flash color and original for a hard two-tone strobe.
            SetTargets((remainingSteps & 1) == 1 ? flashColor : originals[0]);
        }
    }

    private void SetTargets(Color color)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].color = color;
            }
        }
    }

    private void Restore()
    {
        if (!flashing)
        {
            return;
        }

        flashing = false;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].color = originals[i];
            }
        }
    }

    private void OnDisable()
    {
        Restore();
    }
}
