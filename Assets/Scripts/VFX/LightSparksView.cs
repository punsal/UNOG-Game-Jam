using UnityEngine;

/// <summary>
/// Emits pooled pixel sparks from the relic core and maintains a discrete
/// 3-chip trail of previous core positions. Density and energy follow the
/// relic light state; speed-loss sacrifices reduce energy via SetEnergyScale.
/// </summary>
public class LightSparksView : MonoBehaviour, IResettable
{
    private enum LightState { Idle, Low, Critical, Zero }

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private RelicLight relicLight;
    [SerializeField] private Transform coreTransform;
    [SerializeField] private FixedSpritePool sparkPool;
    [SerializeField] private SpriteRenderer[] trailRenderers;
    [SerializeField] private Sprite sparkDot;
    [SerializeField] private Sprite sparkShard;

    [Header("Spark emission per state")]
    [SerializeField] private float idleInterval = 0.18f;
    [SerializeField] private float lowInterval = 0.32f;
    [SerializeField] private float criticalInterval = 0.55f;
    [SerializeField] private float idleLifetime = 0.3f;
    [SerializeField] private float lowLifetime = 0.24f;
    [SerializeField] private float criticalLifetime = 0.18f;
    [SerializeField] private int idleMaxActive = 5;
    [SerializeField] private int lowMaxActive = 3;
    [SerializeField] private int criticalMaxActive = 2;
    [SerializeField] private float driftSpeed = 0.9f;
    [SerializeField] [Range(0f, 1f)] private float criticalSkipChance = 0.35f;

    [Header("Trail")]
    [SerializeField] private float trailSampleDistance = 0.2f;
    [SerializeField] private float trailSampleInterval = 0.09f;
    [SerializeField] private float trailStaleTime = 0.35f;

    private LightState state;
    private float currentLight = 100f;
    private float emitTimer;
    private float energyScale = 1f;

    private readonly Vector3[] trailPositions = new Vector3[8];
    private readonly float[] trailAges = new float[8];
    private int trailHead = -1;
    private Vector3 lastSamplePos;
    private float sampleTimer;
    private Vector3 lastCorePos;

    private void OnEnable()
    {
        if (relicLight == null)
        {
            relicLight = GetComponentInParent<RelicLight>();
        }

        sparkPool.Initialize();
        HideTrail();

        if (relicLight != null)
        {
            relicLight.LightChanged += HandleLightChanged;
            HandleLightChanged(relicLight.CurrentLight);
        }
    }

    private void OnDisable()
    {
        if (relicLight != null)
        {
            relicLight.LightChanged -= HandleLightChanged;
        }
        sparkPool.DespawnAll();
        HideTrail();
    }

    private void HandleLightChanged(float light)
    {
        currentLight = light;
        state = light <= 0f ? LightState.Zero
              : light <= CriticalThreshold ? LightState.Critical
              : light <= LowThreshold ? LightState.Low
              : LightState.Idle;
    }

    private float LowThreshold => settings != null ? settings.LowThreshold : 50f;
    private float CriticalThreshold => settings != null ? settings.CriticalThreshold : 20f;
    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;

    /// <summary>Persistent speed-loss hook: lower values mean weaker, slower sparks.</summary>
    public void SetEnergyScale(float scale)
    {
        energyScale = Mathf.Clamp(scale, 0.3f, 1f);
    }

    private void LateUpdate()
    {
        if (coreTransform == null)
        {
            return;
        }

        Tick(Time.deltaTime, Time.time);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float deltaTime, float time)
    {
        Vector3 corePos = coreTransform.position;

        StepSparks(deltaTime, corePos);
        StepTrail(deltaTime, corePos);

        lastCorePos = corePos;
    }

    private void StepSparks(float deltaTime, Vector3 corePos)
    {
        sparkPool.Step(deltaTime, Ppu);

        // Stepped brightness on live sparks.
        Color tint = state == LightState.Critical
            ? (settings != null ? settings.Ember : LastLightVFXPalette.Ember)
            : Color.white;
        for (int i = 0; i < sparkPool.Capacity; i++)
        {
            if (sparkPool.IsActive(i))
            {
                sparkPool.Renderer(i).color = FixedSpritePool.SteppedFade(tint, sparkPool.NormalizedAge(i));
            }
        }

        if (state == LightState.Zero)
        {
            return;
        }

        emitTimer -= deltaTime;
        if (emitTimer > 0f)
        {
            return;
        }

        float interval = state == LightState.Critical ? criticalInterval
                       : state == LightState.Low ? lowInterval
                       : idleInterval;
        // Irregular cadence at critical: jitter the interval and sometimes skip.
        if (state == LightState.Critical)
        {
            interval *= 0.8f + Random.value * 0.6f;
        }
        emitTimer = interval / Mathf.Max(0.3f, energyScale);

        if (state == LightState.Critical && Random.value < criticalSkipChance)
        {
            return;
        }

        int maxActive = state == LightState.Critical ? criticalMaxActive
                      : state == LightState.Low ? lowMaxActive
                      : idleMaxActive;
        int active = 0;
        for (int i = 0; i < sparkPool.Capacity; i++)
        {
            if (sparkPool.IsActive(i))
            {
                active++;
            }
        }
        if (active >= maxActive)
        {
            return;
        }

        float lifetime = state == LightState.Critical ? criticalLifetime
                       : state == LightState.Low ? lowLifetime
                       : idleLifetime;
        lifetime *= 0.85f + Random.value * 0.3f;

        // Shards read better while moving diagonally; dots otherwise.
        float horizontalSpeed = Mathf.Abs(coreTransform.position.x - lastCorePos.x);
        Sprite sprite = horizontalSpeed > 0.02f && sparkShard != null ? sparkShard : sparkDot;

        float energy = driftSpeed * energyScale * (state == LightState.Idle ? 1f : 0.6f);
        Vector3 velocity = new Vector3(
            (Random.value - 0.5f) * 0.6f * energy,
            (0.5f + Random.value * 0.5f) * energy,
            0f);

        Vector3 spawnPos = corePos;
        spawnPos.x += (Random.value - 0.5f) * 0.125f;
        spawnPos.y += (Random.value - 0.5f) * 0.125f;
        sparkPool.Spawn(PixelPositionUtility.Snap(spawnPos, Ppu), velocity, lifetime, sprite, Color.white);
    }

    private void StepTrail(float deltaTime, Vector3 corePos)
    {
        if (trailRenderers == null || trailRenderers.Length == 0)
        {
            return;
        }

        sampleTimer += deltaTime;
        for (int i = 0; i < trailAges.Length; i++)
        {
            trailAges[i] += deltaTime;
        }

        float moved = Vector3.Distance(corePos, lastSamplePos);
        float sampleDistance = trailSampleDistance / Mathf.Max(0.3f, energyScale);
        if (moved >= sampleDistance && sampleTimer >= trailSampleInterval)
        {
            trailHead = (trailHead + 1) % trailPositions.Length;
            trailPositions[trailHead] = PixelPositionUtility.Snap(lastSamplePos, Ppu);
            trailAges[trailHead] = 0f;
            lastSamplePos = corePos;
            sampleTimer = 0f;
        }
        else if (moved >= sampleDistance)
        {
            lastSamplePos = Vector3.Lerp(lastSamplePos, corePos, 0.5f);
        }

        int visibleChips = state == LightState.Zero ? 0
                         : state == LightState.Critical ? 1
                         : state == LightState.Low ? 2
                         : trailRenderers.Length;

        Color chipColor = state == LightState.Critical
            ? (settings != null ? settings.Ember : LastLightVFXPalette.Ember)
            : (settings != null ? settings.Yellow : LastLightVFXPalette.Yellow);

        for (int i = 0; i < trailRenderers.Length; i++)
        {
            var chip = trailRenderers[i];
            if (chip == null)
            {
                continue;
            }

            int slot = trailHead - i;
            if (slot < 0)
            {
                slot += trailPositions.Length;
            }

            bool show = i < visibleChips && trailHead >= 0 && trailAges[slot] < trailStaleTime;
            if (chip.enabled != show)
            {
                chip.enabled = show;
            }
            if (!show)
            {
                continue;
            }

            chip.transform.position = trailPositions[slot];
            // Quantized shrink and stepped dimming with chip index.
            float scale = i == 0 ? 1f : i == 1 ? 0.75f : 0.5f;
            chip.transform.localScale = new Vector3(scale, scale, 1f);
            chip.color = FixedSpritePool.SteppedFade(chipColor, (i + 1f) / (trailRenderers.Length + 1f));
        }
    }

    private void HideTrail()
    {
        if (trailRenderers == null)
        {
            return;
        }
        for (int i = 0; i < trailRenderers.Length; i++)
        {
            if (trailRenderers[i] != null)
            {
                trailRenderers[i].enabled = false;
            }
        }
        for (int i = 0; i < trailAges.Length; i++)
        {
            trailAges[i] = float.MaxValue;
        }
        trailHead = -1;
    }

    public void ResetRun()
    {
        sparkPool.DespawnAll();
        HideTrail();
        emitTimer = 0f;
        energyScale = 1f;
        if (relicLight != null)
        {
            HandleLightChanged(relicLight.CurrentLight);
        }
    }
}
