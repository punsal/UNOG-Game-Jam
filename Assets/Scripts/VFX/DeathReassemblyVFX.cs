using UnityEngine;

/// <summary>
/// Death and fast-restart presentation. Restart in this project is synchronous
/// (PlayerHealth.Died -> RunController.Restart in the same frame), so this is a
/// non-blocking overlay: fragments burst outward at the death position while
/// the run resets, then converge onto the spawned player and finish with one
/// yellow confirmation spark. Never delays or decides the restart; the player
/// silhouette is not hidden because control returns instantly.
/// </summary>
public class DeathReassemblyVFX : MonoBehaviour, IResettable
{
    private enum Phase { None, Out, In, Spark }

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private FixedSpritePool fragmentPool;
    [SerializeField] private SpriteRenderer emberRenderer;
    [SerializeField] private SpriteRenderer sparkRenderer;

    [Header("Timing (total stays under ~1s)")]
    [SerializeField] private float outDuration = 0.35f;
    [SerializeField] private float inDuration = 0.3f;
    [SerializeField] private float sparkDuration = 0.15f;
    [SerializeField] private float fragmentSpeed = 2.2f;

    private Phase phase = Phase.None;
    private float phaseTimer;
    private Vector3 deathPos;
    private readonly Vector3[] outPositions = new Vector3[10];

    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;
    private float MotionScale => settings != null ? settings.MotionScale : 1f;

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
        if (playerHealth != null)
        {
            playerHealth.Died += HandleDied;
        }
        fragmentPool.Initialize();
        if (emberRenderer != null)
        {
            emberRenderer.enabled = false;
        }
        if (sparkRenderer != null)
        {
            sparkRenderer.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandleDied;
        }
        ClearAll();
    }

    public void HandleDied()
    {
        deathPos = transform.position;

        fragmentPool.DespawnAll();
        for (int i = 0; i < fragmentPool.Capacity; i++)
        {
            // Restrained radial burst, quantized directions.
            float angle = (i / (float)fragmentPool.Capacity) * Mathf.PI * 2f;
            Vector3 vel = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * fragmentSpeed * MotionScale;
            fragmentPool.Spawn(PixelPositionUtility.Snap(deathPos, Ppu), vel, float.MaxValue, null, Color.white);
        }

        if (emberRenderer != null)
        {
            emberRenderer.transform.position = PixelPositionUtility.Snap(deathPos + Vector3.up * 0.5f, Ppu);
            emberRenderer.color = settings != null ? settings.Ember : LastLightVFXPalette.Ember;
            emberRenderer.enabled = true;
        }

        phase = Phase.Out;
        phaseTimer = outDuration;
    }

    // RunController restarts after a short death delay (audio sting). A running
    // death timeline survives the reset: the In phase converges on the respawned
    // player. Resets without an active timeline (retry button) clear everything.
    public void ResetRun()
    {
        if (phase == Phase.None)
        {
            ClearAll();
        }
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {
        if (phase == Phase.None)
        {
            return;
        }

        phaseTimer -= dt;
        fragmentPool.Step(dt, Ppu);

        switch (phase)
        {
            case Phase.Out:
                // Ember rises slightly, holds, then extinguishes at phase end.
                if (emberRenderer != null && emberRenderer.enabled)
                {
                    Vector3 pos = emberRenderer.transform.position;
                    pos.y += 0.6f * dt;
                    emberRenderer.transform.position = PixelPositionUtility.Snap(pos, Ppu);
                    if (phaseTimer < outDuration * 0.25f)
                    {
                        emberRenderer.color = (settings != null ? settings.Ember : LastLightVFXPalette.Ember) * 0.4f;
                    }
                }
                if (phaseTimer <= 0f)
                {
                    if (emberRenderer != null)
                    {
                        emberRenderer.enabled = false;
                    }
                    // Record scatter positions, then converge onto the respawned player.
                    for (int i = 0; i < fragmentPool.Capacity; i++)
                    {
                        if (fragmentPool.IsActive(i))
                        {
                            outPositions[Mathf.Min(i, outPositions.Length - 1)] = fragmentPool.Renderer(i).transform.position;
                            fragmentPool.SetVelocity(i, Vector3.zero);
                        }
                    }
                    phase = Phase.In;
                    phaseTimer = inDuration;
                }
                break;

            case Phase.In:
                Vector3 target = PixelPositionUtility.Snap(transform.position, Ppu);
                float progress = 1f - Mathf.Max(0f, phaseTimer) / inDuration;
                float stepped = Mathf.Floor(progress * 6f) / 6f;
                for (int i = 0; i < fragmentPool.Capacity; i++)
                {
                    if (fragmentPool.IsActive(i))
                    {
                        Vector3 from = outPositions[Mathf.Min(i, outPositions.Length - 1)];
                        fragmentPool.Renderer(i).transform.position =
                            PixelPositionUtility.Snap(Vector3.Lerp(from, target, stepped), Ppu);
                    }
                }
                if (phaseTimer <= 0f)
                {
                    fragmentPool.DespawnAll();
                    if (sparkRenderer != null)
                    {
                        sparkRenderer.transform.position = target;
                        sparkRenderer.color = settings != null ? settings.Yellow : LastLightVFXPalette.Yellow;
                        sparkRenderer.enabled = true;
                    }
                    phase = Phase.Spark;
                    phaseTimer = sparkDuration;
                }
                break;

            case Phase.Spark:
                if (phaseTimer <= 0f)
                {
                    if (sparkRenderer != null)
                    {
                        sparkRenderer.enabled = false;
                    }
                    phase = Phase.None;
                }
                break;
        }
    }

    private void ClearAll()
    {
        phase = Phase.None;
        fragmentPool.DespawnAll();
        if (emberRenderer != null)
        {
            emberRenderer.enabled = false;
        }
        if (sparkRenderer != null)
        {
            sparkRenderer.enabled = false;
        }
    }
}
