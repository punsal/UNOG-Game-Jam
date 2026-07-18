using UnityEngine;

/// <summary>
/// Hazard warning presentation for the two real hazard archetypes:
/// Spike — pink crack that brightens in three hard ticks as the hazard scrolls
/// toward the player, small dust burst when it becomes imminent, fast dim after.
/// Blade — cyan motion edge on the leading side with a one-step compression
/// pulse on direction reversal. Reads positions only; never touches damage,
/// colliders or hazard timing. Warning start mirrors ObstacleBase's
/// camera-relative telegraph trigger so sight and sound fire together.
/// </summary>
public class HazardTelegraphVFX : MonoBehaviour, IResettable
{
    public enum HazardMode { Spike, Blade }

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private HazardMode mode = HazardMode.Spike;

    [Header("Spike")]
    [SerializeField] private SpriteRenderer crackRenderer;
    [SerializeField] private FixedSpritePool dustPool;
    [Tooltip("Extra world units above the camera's visible top edge where the warning begins (matches ObstacleBase telegraphLeadDistance).")]
    [SerializeField] private float warningLeadDistance = 2f;
    [Tooltip("Fallback warning Y when no camera is available (matches ObstacleBase telegraphY).")]
    [SerializeField] private float warningY = 12f;
    [SerializeField] private float imminentY = 5f;
    [SerializeField] private float recoverOffset = 1.5f;

    [Header("Blade")]
    [SerializeField] private SpriteRenderer edgeLeft;
    [SerializeField] private SpriteRenderer edgeRight;
    [SerializeField] private Transform movingTransform;
    [SerializeField] private float compressDuration = 0.08f;

    private Transform player;
    private Camera gameCamera;
    private bool playerSearched;
    private bool cameraFallbackWarned;
    private float lastX;
    private float lastDirection;
    private float compressTimer;
    private bool dustFired;
    private int warnLevel = -1;

    // CameraFitter width-locks ortho size per aspect, so the warning start must
    // track the camera's visible top edge; mirrors ObstacleBase.TelegraphTriggerY.
    private float WarningStartY => gameCamera != null
        ? gameCamera.transform.position.y + gameCamera.orthographicSize + warningLeadDistance
        : warningY;

    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;
    private Color Pink => settings != null ? settings.Pink : LastLightVFXPalette.Pink;
    private Color Cyan => settings != null ? settings.Cyan : LastLightVFXPalette.Cyan;

    private void OnEnable()
    {
        // Stages re-activate on each run/section: re-arm.
        dustFired = false;
        warnLevel = -1;
        compressTimer = 0f;
        if (gameCamera == null)
        {
            // Only an orthographic camera yields a meaningful top edge; otherwise
            // WarningStartY falls back to the fixed warningY.
            Camera cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                gameCamera = cam;
            }
            else if (!cameraFallbackWarned)
            {
                cameraFallbackWarned = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("HazardTelegraphVFX: no orthographic main camera; using fixed warningY fallback.", this);
#endif
            }
        }
        if (crackRenderer != null)
        {
            crackRenderer.enabled = false;
        }
        if (edgeLeft != null)
        {
            edgeLeft.enabled = false;
        }
        if (edgeRight != null)
        {
            edgeRight.enabled = false;
        }
        if (movingTransform != null)
        {
            lastX = movingTransform.position.x;
        }
        dustPool.Initialize();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {
        if (!playerSearched)
        {
            var playerGo = GameObject.FindWithTag("Player");
            player = playerGo != null ? playerGo.transform : null;
            playerSearched = true;
        }

        if (mode == HazardMode.Spike)
        {
            TickSpike(dt);
        }
        else
        {
            TickBlade(dt);
        }
    }

    private void TickSpike(float dt)
    {
        dustPool.Step(dt, Ppu);
        for (int i = 0; i < dustPool.Capacity; i++)
        {
            if (dustPool.IsActive(i))
            {
                dustPool.Renderer(i).color = FixedSpritePool.SteppedFade(Pink, dustPool.NormalizedAge(i));
            }
        }

        if (crackRenderer == null)
        {
            return;
        }

        float y = transform.position.y;
        float playerY = player != null ? player.position.y : 0f;
        float warnStartY = WarningStartY;

        if (y > warnStartY)
        {
            SetWarnLevel(-1);
            return;
        }

        if (y < playerY - recoverOffset)
        {
            // Recover: dim rapidly once the danger has passed.
            SetWarnLevel(-1);
            return;
        }

        // Brightness climbs in three hard ticks between warning range and imminence.
        float progress = Mathf.InverseLerp(warnStartY, imminentY, y);
        int level = Mathf.Clamp((int)(progress * 3f), 0, 2);
        SetWarnLevel(level);

        // Small vertical debris burst the moment the hazard becomes imminent.
        if (!dustFired && y <= imminentY)
        {
            dustFired = true;
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = transform.position + new Vector3((i - 1) * 0.25f, 0f, 0f);
                Vector3 vel = new Vector3((i - 1) * 0.4f, 1.6f, 0f);
                dustPool.Spawn(PixelPositionUtility.Snap(pos, Ppu), vel, 0.25f, null, Pink);
            }
        }
    }

    private void SetWarnLevel(int level)
    {
        if (level == warnLevel || crackRenderer == null)
        {
            return;
        }
        warnLevel = level;
        if (level < 0)
        {
            crackRenderer.enabled = false;
            return;
        }
        crackRenderer.enabled = true;
        float brightness = level == 0 ? 0.45f : level == 1 ? 0.7f : 1f;
        Color c = Pink;
        crackRenderer.color = new Color(c.r * brightness, c.g * brightness, c.b * brightness, 1f);
    }

    private void TickBlade(float dt)
    {
        if (movingTransform == null || (edgeLeft == null && edgeRight == null))
        {
            return;
        }

        float x = movingTransform.position.x;
        float direction = Mathf.Abs(x - lastX) > 0.0005f ? Mathf.Sign(x - lastX) : lastDirection;

        // One-step compression pulse on reversal.
        if (lastDirection != 0f && direction != 0f && direction != lastDirection)
        {
            compressTimer = compressDuration;
        }
        lastX = x;
        lastDirection = direction;

        bool moving = direction != 0f;
        bool movingRight = direction > 0f;
        if (edgeLeft != null)
        {
            edgeLeft.enabled = moving && !movingRight;
        }
        if (edgeRight != null)
        {
            edgeRight.enabled = moving && movingRight;
        }

        var active = movingRight ? edgeRight : edgeLeft;
        if (active != null && active.enabled)
        {
            active.color = Cyan;
            float squash = compressTimer > 0f ? 0.6f : 1f;
            active.transform.localScale = new Vector3(squash * (movingRight ? 1f : -1f), 1f, 1f);
        }
        if (compressTimer > 0f)
        {
            compressTimer -= dt;
        }
    }

    public void ResetRun()
    {
        dustFired = false;
        warnLevel = -1;
        compressTimer = 0f;
        lastDirection = 0f;
        dustPool.DespawnAll();
        if (crackRenderer != null)
        {
            crackRenderer.enabled = false;
        }
        if (edgeLeft != null)
        {
            edgeLeft.enabled = false;
        }
        if (edgeRight != null)
        {
            edgeRight.enabled = false;
        }
    }
}
