using UnityEngine;

/// <summary>
/// Directional damage feedback: one-step white body flash, pink fracture burst
/// at the contact side, pooled debris flying opposite the hit, a very short
/// camera impulse, a brief relic-core lag, and a pink screen-edge frame.
/// Consumes PlayerHealth.Damaged; never touches damage math.
/// </summary>
public class DamageVFXView : MonoBehaviour, IResettable
{
    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SpriteFlash bodyFlash;
    [SerializeField] private Transform coreTransform;
    [SerializeField] private SpriteRenderer burstRenderer;
    [SerializeField] private Sprite[] burstFrames;
    [SerializeField] private FixedSpritePool debrisPool;

    [Header("Timing")]
    [SerializeField] private float burstDuration = 0.15f;
    [SerializeField] private float debrisLifetime = 0.22f;
    [SerializeField] private float debrisSpeed = 2.5f;
    [SerializeField] private int debrisCount = 5;
    [SerializeField] private float coreLagDuration = 0.1f;
    [SerializeField] private float coreLagPixels = 2f;
    [SerializeField] private float cameraImpulseMagnitude = 0.125f;
    [SerializeField] private float cameraImpulseDuration = 0.06f;

    private float burstTimer;
    private float coreLagTimer;
    private Vector3 coreOriginalLocalPos;
    private bool coreOriginalCaptured;
    private Vector3 coreLagOffset;
    private bool coreLagApplied;

    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;

    private void Awake()
    {
        CaptureCoreOriginal();
    }

    private void CaptureCoreOriginal()
    {
        if (!coreOriginalCaptured && coreTransform != null)
        {
            coreOriginalLocalPos = coreTransform.localPosition;
            coreOriginalCaptured = true;
        }
    }

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
        if (playerHealth != null)
        {
            playerHealth.Damaged += HandleDamaged;
        }
        debrisPool.Initialize();
        if (burstRenderer != null)
        {
            burstRenderer.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged -= HandleDamaged;
        }
        ClearAll();
    }

    // Public fallback trigger for integrations without a PlayerHealth event.
    public void HandleDamaged(Vector2 hitDirection)
    {
        if (hitDirection == Vector2.zero)
        {
            hitDirection = Vector2.up; // unknown source reads as a frontal hit
        }

        if (bodyFlash != null)
        {
            bodyFlash.Flash();
        }

        // Fracture burst on the contact side (opposite the knockback direction).
        if (burstRenderer != null && burstFrames != null && burstFrames.Length > 0)
        {
            Vector3 contact = transform.position - (Vector3)(hitDirection * 0.4f);
            burstRenderer.transform.position = PixelPositionUtility.Snap(contact, Ppu);
            burstRenderer.sprite = burstFrames[0];
            burstRenderer.color = settings != null ? settings.Pink : LastLightVFXPalette.Pink;
            burstRenderer.enabled = true;
            burstTimer = burstDuration;
        }

        // Debris flies with the hit direction (away from the obstacle).
        Color debrisColor = settings != null ? settings.Pink : LastLightVFXPalette.Pink;
        for (int i = 0; i < debrisCount; i++)
        {
            float spread = (i - (debrisCount - 1) * 0.5f) * 0.35f;
            Vector2 dir = Rotate(hitDirection, spread);
            Vector3 velocity = new Vector3(dir.x, dir.y, 0f) * debrisSpeed * (0.7f + Random.value * 0.6f);
            debrisPool.Spawn(transform.position, velocity, debrisLifetime * (0.8f + Random.value * 0.4f), null, debrisColor);
        }

        if (CameraImpulse2D.Instance != null)
        {
            CameraImpulse2D.Instance.Play(-hitDirection, cameraImpulseMagnitude, cameraImpulseDuration);
        }

        if (ScreenFXOverlay.Instance != null)
        {
            ScreenFXOverlay.Instance.PlayDamageEdge();
        }

        // Relic core lags one beat opposite the knockback, then snaps back.
        if (coreTransform != null)
        {
            CaptureCoreOriginal();
            coreLagOffset = -(Vector3)(hitDirection * (coreLagPixels / Ppu));
            coreTransform.localPosition = coreOriginalLocalPos + coreLagOffset;
            coreLagTimer = coreLagDuration;
            coreLagApplied = true;
        }
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {

        if (burstTimer > 0f)
        {
            burstTimer -= dt;
            if (burstTimer <= 0f)
            {
                burstRenderer.enabled = false;
            }
            else if (burstFrames.Length > 1)
            {
                float progress = 1f - burstTimer / burstDuration;
                int frame = Mathf.Min(burstFrames.Length - 1, (int)(progress * burstFrames.Length));
                burstRenderer.sprite = burstFrames[frame];
            }
        }

        debrisPool.Step(dt, Ppu);
        Color baseColor = settings != null ? settings.Pink : LastLightVFXPalette.Pink;
        for (int i = 0; i < debrisPool.Capacity; i++)
        {
            if (debrisPool.IsActive(i))
            {
                debrisPool.Renderer(i).color = FixedSpritePool.SteppedFade(baseColor, debrisPool.NormalizedAge(i));
            }
        }

        if (coreLagApplied)
        {
            coreLagTimer -= dt;
            if (coreLagTimer <= 0f)
            {
                coreTransform.localPosition = coreOriginalLocalPos;
                coreLagApplied = false;
            }
        }
    }

    private void ClearAll()
    {
        burstTimer = 0f;
        if (burstRenderer != null)
        {
            burstRenderer.enabled = false;
        }
        debrisPool.DespawnAll();
        if (coreLagApplied && coreTransform != null)
        {
            coreTransform.localPosition = coreOriginalLocalPos;
            coreLagApplied = false;
        }
    }

    public void ResetRun()
    {
        ClearAll();
    }
}
