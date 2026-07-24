using UnityEngine;

/// <summary>
/// Coordinates the permanent visual consequences of sacrifices. Reads completed
/// transfers and updates overlays/other views; owns no gameplay state and never
/// changes stats. "Every gain flashes briefly. Every loss remains."
/// </summary>
public class PersistentSacrificeView : MonoBehaviour, IResettable
{
    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private SacrificeTransferVFX transferVFX;
    [SerializeField] private LightSparksView sparksView;
    [SerializeField] private SpriteFlash bodyFlash;
    [SerializeField] private SpriteRenderer crackOverlay;
    [SerializeField] private SpriteRenderer missingPixelOverlay;
    [SerializeField] private SpriteRenderer sizeRing;

    [Header("Speed loss")]
    [Tooltip("Spark energy multiplier applied per unit of |Body| speed value (matches ModifierStack scale).")]
    [SerializeField] private float speedLossToEnergy = 1f;

    [Header("Size penalty ring")]
    [SerializeField] private float ringDuration = 0.25f;
    [SerializeField] private int ringSteps = 3;
    [SerializeField] private float ringMaxScale = 2.5f;

    private int bloodCount;
    private float speedLossTotal;
    private float sightLossTotal;
    private float ringTimer;
    private PixelVignetteView vignette;
    private bool vignetteSearched;

    private void OnEnable()
    {
        if (transferVFX == null)
        {
            transferVFX = GetComponentInParent<SacrificeTransferVFX>();
        }
        if (transferVFX != null)
        {
            transferVFX.TransferCompleted += ApplyCost;
        }
        ApplyOverlays();
    }

    private void OnDisable()
    {
        if (transferVFX != null)
        {
            transferVFX.TransferCompleted -= ApplyCost;
        }
    }

    /// <summary>Applies the permanent visual for a completed sacrifice.</summary>
    public void ApplyCost(CostData cost)
    {
        if (cost == null)
        {
            return;
        }

        switch (cost.CostType)
        {
            case CostType.Blood:
                bloodCount++;
                break;
            case CostType.Body:
                if (cost.Value < 0f)
                {
                    speedLossTotal += Mathf.Abs(cost.Value) * speedLossToEnergy;
                }
                else if (cost.Value > 0f)
                {
                    PlaySizeRing();
                }
                break;
            case CostType.Sight:
                sightLossTotal += Mathf.Abs(cost.Value);
                break;
            case CostType.Light:
                // Relic loss stays visible through LightCoreView/LightSparksView.
                break;
        }

        ApplyOverlays();
    }

    /// <summary>
    /// Hook for a future audio-layer sacrifice: brief purple sensory collapse.
    /// Call from the system that removes a sound layer; no such cost exists yet.
    /// </summary>
    public void PlaySensoryCollapse()
    {
        if (bodyFlash != null)
        {
            bodyFlash.Flash(settings != null ? settings.Purple : LastLightVFXPalette.Purple, 2);
        }
    }

    private void ApplyOverlays()
    {
        if (crackOverlay != null)
        {
            crackOverlay.enabled = bloodCount > 0;
            if (bloodCount > 0)
            {
                Color c = settings != null ? settings.Pink : LastLightVFXPalette.Pink;
                crackOverlay.color = bloodCount == 1 ? c * 0.8f : c;
            }
        }

        if (missingPixelOverlay != null)
        {
            missingPixelOverlay.enabled = bloodCount >= 2;
        }

        if (sparksView != null)
        {
            sparksView.SetEnergyScale(1f - Mathf.Min(0.5f, speedLossTotal));
        }

        if (!vignetteSearched)
        {
            vignette = FindAnyObjectByType<PixelVignetteView>(FindObjectsInactive.Include);
            vignetteSearched = true;
        }
        if (vignette != null)
        {
            // Mirror ModifierStack: sight multiplier floors at 0.65.
            vignette.SetSightMultiplier(Mathf.Max(0.65f, 1f - sightLossTotal));
        }
    }

    private void PlaySizeRing()
    {
        if (sizeRing == null)
        {
            return;
        }
        Color c = settings != null ? settings.Pink : LastLightVFXPalette.Pink;
        sizeRing.color = c;
        sizeRing.transform.localScale = Vector3.one;
        sizeRing.enabled = true;
        ringTimer = ringDuration;
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {
        if (ringTimer <= 0f || sizeRing == null)
        {
            return;
        }

        ringTimer -= dt;
        if (ringTimer <= 0f)
        {
            sizeRing.enabled = false;
            return;
        }

        // Uncomfortable quantized expansion: hard scale steps, no easing.
        float progress = 1f - ringTimer / ringDuration;
        float stepped = Mathf.Floor(progress * ringSteps) / (ringSteps - 1f);
        float scale = Mathf.Lerp(1f, ringMaxScale, Mathf.Clamp01(stepped));
        sizeRing.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void ResetRun()
    {
        bloodCount = 0;
        speedLossTotal = 0f;
        sightLossTotal = 0f;
        ringTimer = 0f;
        if (sizeRing != null)
        {
            sizeRing.enabled = false;
        }
        ApplyOverlays();
    }
}
