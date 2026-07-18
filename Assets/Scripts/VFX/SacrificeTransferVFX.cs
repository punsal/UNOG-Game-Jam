using System;
using UnityEngine;

/// <summary>
/// Forced-extraction visual when a cost altar is chosen: pixels are pulled from
/// the sacrificed source (body, relic, eyes) along a direct, inevitable path to
/// the altar. No spirals, no celebration. Fires TransferCompleted so persistent
/// views apply the permanent mark. Reads decisions; never makes them.
/// </summary>
public class SacrificeTransferVFX : MonoBehaviour, IResettable
{
    public event Action<CostData> TransferCompleted;

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform coreTransform;
    [SerializeField] private FixedSpritePool particlePool;
    [SerializeField] private Sprite pixelSmall;
    [SerializeField] private Sprite pixelMedium;
    [SerializeField] private Sprite shard;
    [SerializeField] private RiskAcceptanceVFX riskAcceptance;

    [Header("Timing")]
    [SerializeField] private float transferDuration = 0.45f;
    [SerializeField] private float perParticleStagger = 0.03f;
    [SerializeField] private int movementSteps = 8;
    [Tooltip("Very short presentation slow-down on commitment. 0 disables it.")]
    [SerializeField] private float hitStopDuration = 0.08f;
    [SerializeField] [Range(0.1f, 1f)] private float hitStopScale = 0.35f;

    [Header("Severity normalization (|cost value| that counts as full severity)")]
    [SerializeField] private float bloodFullSeverity = 2f;
    [SerializeField] private float lightFullSeverity = 40f;
    [SerializeField] private float sightFullSeverity = 0.35f;
    [SerializeField] private float bodyFullSeverity = 0.3f;

    private readonly Vector3[] starts = new Vector3[12];
    private readonly Vector3[] targets = new Vector3[12];
    private readonly float[] delays = new float[12];
    private readonly int[] slots = new int[12];
    private int particleCount;
    private float elapsed;
    private bool transferring;
    private CostData activeCost;
    private float hitStopTimer;
    private bool hitStopApplied;
    private ChoiceGate[] gates;

    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;

    private void Start()
    {
        // One-time scene lookup; gates live inside inactive stage chunks.
        gates = FindObjectsByType<ChoiceGate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].AltarChosen += HandleAltarChosen;
        }
        particlePool.Initialize();
    }

    private void OnDestroy()
    {
        if (gates == null)
        {
            return;
        }
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null)
            {
                gates[i].AltarChosen -= HandleAltarChosen;
            }
        }
    }

    private void OnDisable()
    {
        RestoreTimeScale();
    }

    private void HandleAltarChosen(AltarTrigger altar)
    {
        if (altar == null)
        {
            return;
        }

        if (altar.Offer == null)
        {
            // No cost paid: the world gets more dangerous instead.
            if (riskAcceptance != null)
            {
                riskAcceptance.Play(altar);
            }
            return;
        }

        Transform anchor = altar.transform.Find("VFXAnchor");
        Play(SourceFor(altar.Offer.CostType), anchor != null ? anchor : altar.transform,
             altar.Offer, SeverityFor(altar.Offer));
    }

    private Transform SourceFor(CostType type)
    {
        switch (type)
        {
            case CostType.Light: return coreTransform != null ? coreTransform : transform;
            default: return bodyTransform != null ? bodyTransform : transform;
        }
    }

    private float SeverityFor(CostData cost)
    {
        float v = Mathf.Abs(cost.Value);
        switch (cost.CostType)
        {
            case CostType.Blood: return Mathf.Clamp01(v / bloodFullSeverity);
            case CostType.Light: return Mathf.Clamp01(v / lightFullSeverity);
            case CostType.Sight: return Mathf.Clamp01(v / sightFullSeverity);
            case CostType.Body: return Mathf.Clamp01(v / bodyFullSeverity);
            default: return 0.5f;
        }
    }

    private Color ColorFor(CostType type)
    {
        bool hasSettings = settings != null;
        switch (type)
        {
            case CostType.Blood: return hasSettings ? settings.Red : LastLightVFXPalette.Red;
            case CostType.Light: return hasSettings ? settings.Yellow : LastLightVFXPalette.Yellow;
            case CostType.Sight: return hasSettings ? settings.Purple : LastLightVFXPalette.Purple;
            default: return hasSettings ? settings.Pink : LastLightVFXPalette.Pink;
        }
    }

    /// <summary>Public trigger: source transform, target transform, cost, normalized severity.</summary>
    public void Play(Transform source, Transform target, CostData cost, float severity)
    {
        if (source == null || target == null)
        {
            return;
        }

        particlePool.DespawnAll();
        particleCount = Mathf.Clamp(4 + Mathf.RoundToInt(severity * 8f), 4, 12);
        elapsed = 0f;
        transferring = true;
        activeCost = cost;

        Color color = cost != null ? ColorFor(cost.CostType) : (settings != null ? settings.Pink : LastLightVFXPalette.Pink);
        Vector3 eyeOffset = cost != null && cost.CostType == CostType.Sight ? new Vector3(0f, 0.5f, 0f) : Vector3.zero;

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 offset = new Vector3((UnityEngine.Random.value - 0.5f) * 0.5f, (UnityEngine.Random.value - 0.5f) * 0.6f, 0f);
            starts[i] = PixelPositionUtility.Snap(source.position + eyeOffset + offset, Ppu);
            targets[i] = PixelPositionUtility.Snap(target.position, Ppu);
            delays[i] = i * perParticleStagger;
            Sprite sprite = severity > 0.6f && i % 3 == 0 && shard != null ? shard
                          : i % 2 == 0 && pixelMedium != null ? pixelMedium
                          : pixelSmall;
            slots[i] = particlePool.Spawn(starts[i], Vector3.zero, float.MaxValue, sprite, color);
        }

        if (hitStopDuration > 0f && Application.isPlaying && Mathf.Approximately(Time.timeScale, 1f))
        {
            Time.timeScale = hitStopScale;
            hitStopTimer = hitStopDuration;
            hitStopApplied = true;
        }
    }

    private void Update()
    {
        if (hitStopApplied)
        {
            hitStopTimer -= Time.unscaledDeltaTime;
            if (hitStopTimer <= 0f)
            {
                RestoreTimeScale();
            }
        }

        if (!transferring)
        {
            return;
        }

        Tick(Time.unscaledDeltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {
        if (!transferring)
        {
            return;
        }

        elapsed += dt;
        bool anyActive = false;

        for (int i = 0; i < particleCount; i++)
        {
            int slot = slots[i];
            if (slot < 0 || !particlePool.IsActive(slot))
            {
                continue;
            }

            float t = (elapsed - delays[i]) / transferDuration;
            if (t < 0f)
            {
                anyActive = true;
                continue;
            }
            if (t >= 1f)
            {
                particlePool.Despawn(slot);
                continue;
            }

            anyActive = true;
            // Slight acceleration toward the altar, quantized into hard steps.
            float eased = t * t * (3f - 2f * t);
            eased = Mathf.Floor(eased * movementSteps) / movementSteps;
            Vector3 pos = Vector3.Lerp(starts[i], targets[i], eased);
            particlePool.Renderer(slot).transform.position = PixelPositionUtility.Snap(pos, Ppu);
        }

        if (!anyActive)
        {
            transferring = false;
            var cost = activeCost;
            activeCost = null;
            TransferCompleted?.Invoke(cost);
        }
    }

    private void RestoreTimeScale()
    {
        if (hitStopApplied)
        {
            Time.timeScale = 1f;
            hitStopApplied = false;
        }
    }

    public void ResetRun()
    {
        particlePool.DespawnAll();
        transferring = false;
        activeCost = null;
        RestoreTimeScale();
    }
}
