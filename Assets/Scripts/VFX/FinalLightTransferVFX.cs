using UnityEngine;

/// <summary>
/// Final light delivery in three visual tiers derived from the committed ending
/// and the exact remaining light (the game's own resolver stays authoritative:
/// Dark ending = bad; Light ending splits good/medium on remaining light).
/// Good: full controlled transfer and the game's first broad yellow response.
/// Medium: unstable flow, some fragments fail. Bad: one pixel reaches, breaks,
/// holds, extinguishes; the screen closes in navy. Plays under the outcome UI.
/// </summary>
public class FinalLightTransferVFX : MonoBehaviour, IResettable
{
    private enum Tier { None, Good, Medium, Bad }

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private RelicLight relicLight;
    [SerializeField] private Transform coreTransform;
    [SerializeField] private FixedSpritePool motePool;
    [SerializeField] private Sprite moteSprite;
    [SerializeField] private Sprite finalPixelSprite;
    [Tooltip("The dying recipient. When empty, the transfer aims a few units above the player.")]
    [SerializeField] private Transform recipient;

    [Header("Tiers")]
    [SerializeField] private float goodLightThreshold = 50f;
    [SerializeField] private float transferDuration = 0.6f;
    [SerializeField] private float riseDuration = 0.4f;
    [SerializeField] private int movementSteps = 10;
    [SerializeField] private float recipientOffsetY = 4f;

    private EndingResolver endingResolver;
    private Tier tier = Tier.None;
    private float elapsed;
    private bool rising;
    private bool closureFired;
    private readonly Vector3[] starts = new Vector3[8];
    private readonly Vector3[] targets = new Vector3[8];
    private readonly float[] delays = new float[8];
    private readonly bool[] fails = new bool[8];
    private int moteCount;

    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;
    private Color Yellow => settings != null ? settings.Yellow : LastLightVFXPalette.Yellow;
    private Color Ember => settings != null ? settings.Ember : LastLightVFXPalette.Ember;

    private void Start()
    {
        if (relicLight == null)
        {
            relicLight = GetComponentInParent<RelicLight>();
        }
        endingResolver = FindFirstObjectByType<EndingResolver>(FindObjectsInactive.Include);
        if (endingResolver != null)
        {
            endingResolver.RunEnded += HandleRunEnded;
        }
        motePool.Initialize();
    }

    private void OnDestroy()
    {
        if (endingResolver != null)
        {
            endingResolver.RunEnded -= HandleRunEnded;
        }
    }

    public void HandleRunEnded(EndingType ending)
    {
        float light = relicLight != null ? relicLight.CurrentLight : 0f;
        Tier newTier = ending == EndingType.Dark ? Tier.Bad
                     : light >= goodLightThreshold ? Tier.Good
                     : Tier.Medium;
        Play(newTier, light);
    }

    private void Play(Tier newTier, float light)
    {
        tier = newTier;
        elapsed = 0f;
        rising = false;
        closureFired = false;
        motePool.DespawnAll();

        Vector3 origin = coreTransform != null ? coreTransform.position : transform.position;
        Vector3 target = recipient != null ? recipient.position : origin + Vector3.up * recipientOffsetY;

        moteCount = tier == Tier.Good ? 8 : tier == Tier.Medium ? 6 : 1;
        for (int i = 0; i < moteCount; i++)
        {
            Vector3 jitter = new Vector3((Random.value - 0.5f) * 0.4f, (Random.value - 0.5f) * 0.3f, 0f);
            starts[i] = PixelPositionUtility.Snap(origin + jitter, Ppu);
            targets[i] = PixelPositionUtility.Snap(target + jitter * 0.5f, Ppu);
            delays[i] = i * 0.05f;
            // Medium: some fragments fail to arrive. Bad: the single pixel breaks late.
            fails[i] = tier == Tier.Medium ? (i == 2 || i == 4) : tier == Tier.Bad;
            Sprite sprite = tier == Tier.Bad && finalPixelSprite != null ? finalPixelSprite : moteSprite;
            motePool.Spawn(starts[i], Vector3.zero, float.MaxValue, sprite, Yellow);
        }

        if (tier == Tier.Good && ScreenFXOverlay.Instance != null)
        {
            // First broad yellow response of the game: restrained, brief.
            ScreenFXOverlay.Instance.SetFullTint(Yellow, 0.1f);
        }
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {
        if (tier == Tier.None)
        {
            return;
        }

        elapsed += dt;

        if (!rising)
        {
            bool anyActive = false;
            for (int i = 0; i < moteCount; i++)
            {
                if (!motePool.IsActive(i))
                {
                    continue;
                }

                float t = (elapsed - delays[i]) / transferDuration;
                if (t < 0f)
                {
                    anyActive = true;
                    continue;
                }

                // Failure point: medium fragments die mid-path, the bad pixel breaks at 70%.
                float failPoint = tier == Tier.Bad ? 0.7f : 0.55f;
                if (fails[i] && t >= failPoint)
                {
                    var r = motePool.Renderer(i);
                    if (tier == Tier.Bad && !closureFired)
                    {
                        // The last pixel holds one beat, then everything darkens.
                        r.color = Ember;
                        if (t >= failPoint + 0.25f)
                        {
                            motePool.Despawn(i);
                            closureFired = true;
                            if (ScreenFXOverlay.Instance != null)
                            {
                                ScreenFXOverlay.Instance.PlayDarkClosure(0.4f, 0.55f);
                            }
                        }
                        else
                        {
                            anyActive = true;
                        }
                    }
                    else
                    {
                        r.color = FixedSpritePool.SteppedFade(Ember, Mathf.Clamp01((t - failPoint) * 3f));
                        if (t >= failPoint + 0.3f)
                        {
                            motePool.Despawn(i);
                        }
                        else
                        {
                            anyActive = true;
                        }
                    }
                    continue;
                }

                if (t >= 1f)
                {
                    if (tier == Tier.Good)
                    {
                        // Arrived: hold at the recipient, then rise.
                        motePool.Renderer(i).transform.position = targets[i];
                        anyActive = true;
                    }
                    else
                    {
                        motePool.Despawn(i);
                    }
                    continue;
                }

                anyActive = true;
                float eased = t * t * (3f - 2f * t);
                float stepped = Mathf.Floor(eased * movementSteps) / movementSteps;
                // Medium instability: stepped lateral wobble on the way.
                Vector3 pos = Vector3.Lerp(starts[i], targets[i], stepped);
                if (tier == Tier.Medium && (Mathf.FloorToInt(t * 8f) & 1) == 1)
                {
                    pos.x += (i % 2 == 0 ? 1f : -1f) / Ppu;
                }
                motePool.Renderer(i).transform.position = PixelPositionUtility.Snap(pos, Ppu);
            }

            if (elapsed >= transferDuration + moteCount * 0.05f + 0.3f)
            {
                if (tier == Tier.Good)
                {
                    rising = true;
                    elapsed = 0f;
                    for (int i = 0; i < moteCount; i++)
                    {
                        if (motePool.IsActive(i))
                        {
                            motePool.SetVelocity(i, Vector3.up * 1.2f);
                        }
                    }
                    if (ScreenFXOverlay.Instance != null)
                    {
                        ScreenFXOverlay.Instance.SetFullTint(Yellow, 0f); // clear the broad response
                    }
                }
                else if (!anyActive)
                {
                    tier = Tier.None;
                }
            }
        }
        else
        {
            // Good tier closure: particles rise and step out.
            motePool.Step(dt, Ppu);
            for (int i = 0; i < motePool.Capacity; i++)
            {
                if (motePool.IsActive(i))
                {
                    motePool.Renderer(i).color = FixedSpritePool.SteppedFade(Yellow, elapsed / riseDuration);
                }
            }
            if (elapsed >= riseDuration)
            {
                motePool.DespawnAll();
                tier = Tier.None;
            }
        }
    }

    public void ResetRun()
    {
        motePool.DespawnAll();
        tier = Tier.None;
        rising = false;
        if (ScreenFXOverlay.Instance != null)
        {
            ScreenFXOverlay.Instance.SetFullTint(Color.black, 0f);
        }
    }
}
