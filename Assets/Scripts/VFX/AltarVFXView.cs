using UnityEngine;

/// <summary>
/// Altar presence and interaction effects. Pay altars: stable yellow signal,
/// inward motes — guaranteed safety at a cost. Risk altars: cyan structure with
/// pink instability, fast upward motes — preserved resources, future danger.
/// Shows a broken pixel link while the player is near; reacts to the gate's
/// choice without ever deciding anything.
/// </summary>
public class AltarVFXView : MonoBehaviour, IResettable
{
    public enum AltarKind { Pay, Risk }
    private enum State { Idle, Chosen, Rejected }

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private AltarKind kind = AltarKind.Pay;
    [SerializeField] private AltarTrigger altarTrigger;
    [SerializeField] private SpriteRenderer[] dimTargets;
    [SerializeField] private FixedSpritePool motePool;
    [SerializeField] private SpriteRenderer absorbRenderer;
    [SerializeField] private Sprite[] absorbFrames;
    [SerializeField] private SpriteRenderer[] linkDashes;
    [SerializeField] private Transform vfxAnchor;

    [Header("Motes")]
    [SerializeField] private float payMoteInterval = 0.5f;
    [SerializeField] private float riskMoteInterval = 0.45f;
    [SerializeField] private float moteLifetime = 0.6f;

    [Header("Near link")]
    [SerializeField] private float nearDistanceX = 1.8f;
    [SerializeField] private float nearDistanceY = 3.5f;

    [Header("Chosen")]
    [SerializeField] private float absorbDuration = 0.25f;
    [SerializeField] [Range(0f, 1f)] private float chosenDim = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float rejectedDim = 0.35f;

    private State state;
    private float moteTimer;
    private float instabilityTimer;
    private float absorbTimer;
    private Color[] originalColors;
    private bool originalsCaptured;
    private Transform player;
    private bool playerSearched;
    private ChoiceGate gate;

    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;
    private Color Yellow => settings != null ? settings.Yellow : LastLightVFXPalette.Yellow;
    private Color Cyan => settings != null ? settings.Cyan : LastLightVFXPalette.Cyan;
    private Color Pink => settings != null ? settings.Pink : LastLightVFXPalette.Pink;

    private void Start()
    {
        if (altarTrigger == null)
        {
            altarTrigger = GetComponentInChildren<AltarTrigger>();
        }
        motePool.Initialize();
        SetLink(false);
        if (absorbRenderer != null)
        {
            absorbRenderer.enabled = false;
        }

        // Find the gate that owns this altar (one-time scene search).
        var gates = FindObjectsByType<ChoiceGate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i].AltarA == altarTrigger || gates[i].AltarB == altarTrigger)
            {
                gate = gates[i];
                gate.AltarChosen += HandleAltarChosen;
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (gate != null)
        {
            gate.AltarChosen -= HandleAltarChosen;
        }
    }

    private void HandleAltarChosen(AltarTrigger chosen)
    {
        if (chosen == altarTrigger)
        {
            state = State.Chosen;
            // The chosen reaction follows the actual offer, not the idle styling:
            // a paid cost absorbs; only a true no-cost (risk) choice collapses.
            bool paidCost = altarTrigger != null && altarTrigger.Offer != null;
            if (paidCost && absorbRenderer != null && absorbFrames != null && absorbFrames.Length > 0)
            {
                absorbRenderer.sprite = absorbFrames[0];
                absorbRenderer.color = Yellow;
                absorbRenderer.enabled = true;
                absorbTimer = absorbDuration;
            }
            else if (!paidCost)
            {
                // Cyan stability collapses; instability wins for two hard ticks.
                motePool.DespawnAll();
                TintTargets(Pink);
            }
            Dim(chosenDim);
        }
        else
        {
            state = State.Rejected;
            motePool.DespawnAll();
            Dim(rejectedDim);
        }
        SetLink(false);
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {
        if (absorbTimer > 0f && absorbRenderer != null)
        {
            absorbTimer -= dt;
            if (absorbTimer <= 0f)
            {
                absorbRenderer.enabled = false;
                if (kind == AltarKind.Risk)
                {
                    RestoreTargets();
                }
            }
            else if (absorbFrames.Length > 1)
            {
                float progress = 1f - absorbTimer / absorbDuration;
                absorbRenderer.sprite = absorbFrames[Mathf.Min(absorbFrames.Length - 1, (int)(progress * absorbFrames.Length))];
            }
        }

        motePool.Step(dt, Ppu);
        for (int i = 0; i < motePool.Capacity; i++)
        {
            if (motePool.IsActive(i))
            {
                Color c = kind == AltarKind.Pay ? Yellow : Cyan;
                motePool.Renderer(i).color = FixedSpritePool.SteppedFade(c, motePool.NormalizedAge(i));
            }
        }

        if (state != State.Idle)
        {
            return;
        }

        // Skip idle work while culled.
        bool visible = dimTargets != null && dimTargets.Length > 0 && dimTargets[0] != null && dimTargets[0].isVisible;
        if (Application.isPlaying && !visible)
        {
            return;
        }

        Vector3 basePos = vfxAnchor != null ? vfxAnchor.position : transform.position;

        moteTimer -= dt;
        if (moteTimer <= 0f)
        {
            if (kind == AltarKind.Pay)
            {
                moteTimer = payMoteInterval;
                // Slow inward pull: motes fall from above toward the altar base.
                Vector3 pos = basePos + new Vector3((Random.value - 0.5f) * 1.2f, 0.9f + Random.value * 0.3f, 0f);
                Vector3 vel = (basePos - pos).normalized * 1.1f;
                motePool.Spawn(PixelPositionUtility.Snap(pos, Ppu), vel, moteLifetime, null, Yellow);
            }
            else
            {
                // Jittered cadence: less stable than the pay altar.
                moteTimer = riskMoteInterval * (0.6f + Random.value * 0.9f);
                Vector3 pos = basePos + new Vector3((Random.value - 0.5f) * 1f, 0.1f, 0f);
                motePool.Spawn(PixelPositionUtility.Snap(pos, Ppu), Vector3.up * 2.2f, moteLifetime * 0.7f, null, Cyan);
            }
        }

        // Risk altar: pink instability blink underneath, one hard tick.
        // Only a true no-cost altar signals danger; a risk-styled altar
        // carrying a real offer must not blink a warning at the player.
        if (kind == AltarKind.Risk && altarTrigger != null && altarTrigger.Offer == null)
        {
            instabilityTimer -= dt;
            if (instabilityTimer <= -0.08f)
            {
                RestoreTargets();
                instabilityTimer = 1.1f + Random.value * 0.4f;
            }
            else if (instabilityTimer <= 0f)
            {
                TintTargets(Pink);
            }
        }

        // Player-near commitment link: thin broken pixel line.
        if (!playerSearched)
        {
            var playerGo = GameObject.FindWithTag("Player");
            player = playerGo != null ? playerGo.transform : null;
            playerSearched = true;
        }
        if (player != null && linkDashes != null && linkDashes.Length > 0)
        {
            Vector3 toAltar = basePos - player.position;
            bool near = Mathf.Abs(toAltar.x) <= nearDistanceX && Mathf.Abs(toAltar.y) <= nearDistanceY;
            SetLink(near);
            if (near)
            {
                for (int i = 0; i < linkDashes.Length; i++)
                {
                    float t = (i + 1f) / (linkDashes.Length + 1f);
                    Vector3 pos = Vector3.Lerp(player.position, basePos, t);
                    linkDashes[i].transform.position = PixelPositionUtility.Snap(pos, Ppu);
                    // Deeper commitment = stronger link.
                    float proximity = 1f - Mathf.Clamp01(Mathf.Abs(toAltar.x) / nearDistanceX);
                    linkDashes[i].color = FixedSpritePool.SteppedFade(Cyan, 1f - proximity);
                }
            }
        }
    }

    private void SetLink(bool on)
    {
        if (linkDashes == null)
        {
            return;
        }
        for (int i = 0; i < linkDashes.Length; i++)
        {
            if (linkDashes[i] != null && linkDashes[i].enabled != on)
            {
                linkDashes[i].enabled = on;
            }
        }
    }

    private void CaptureOriginals()
    {
        if (originalsCaptured || dimTargets == null)
        {
            return;
        }
        originalColors = new Color[dimTargets.Length];
        for (int i = 0; i < dimTargets.Length; i++)
        {
            originalColors[i] = dimTargets[i] != null ? dimTargets[i].color : Color.white;
        }
        originalsCaptured = true;
    }

    private void TintTargets(Color color)
    {
        CaptureOriginals();
        for (int i = 0; i < dimTargets.Length; i++)
        {
            if (dimTargets[i] != null)
            {
                dimTargets[i].color = color;
            }
        }
    }

    private void Dim(float factor)
    {
        CaptureOriginals();
        for (int i = 0; i < dimTargets.Length; i++)
        {
            if (dimTargets[i] != null)
            {
                Color c = originalColors[i];
                dimTargets[i].color = new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
            }
        }
    }

    private void RestoreTargets()
    {
        if (!originalsCaptured)
        {
            return;
        }
        for (int i = 0; i < dimTargets.Length; i++)
        {
            if (dimTargets[i] != null)
            {
                dimTargets[i].color = originalColors[i];
            }
        }
    }

    public void ResetRun()
    {
        state = State.Idle;
        motePool.DespawnAll();
        SetLink(false);
        RestoreTargets();
        if (absorbRenderer != null)
        {
            absorbRenderer.enabled = false;
        }
        absorbTimer = 0f;
        moteTimer = 0f;
        instabilityTimer = 1f;
    }
}
