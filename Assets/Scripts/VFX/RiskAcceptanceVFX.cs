using UnityEngine;

/// <summary>
/// Feedback for refusing sacrifice: the world becomes more dangerous. Pink/red
/// speed lines enter from the top of the view and a threat wave sweeps down.
/// No reward colors, no positive burst; the lower input third stays clear.
/// </summary>
public class RiskAcceptanceVFX : MonoBehaviour, IResettable
{
    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private FixedSpritePool linePool;
    [SerializeField] private SpriteRenderer waveRenderer;
    [SerializeField] private Sprite lineShort;
    [SerializeField] private Sprite lineLong;

    [Header("Timing (total effect stays under ~0.75s)")]
    [SerializeField] private float lineSpeed = 9f;
    [SerializeField] private float lineLifetime = 0.4f;
    [SerializeField] private int lineCount = 6;
    [SerializeField] private float waveDuration = 0.35f;
    [SerializeField] private int waveSteps = 6;

    private Camera cam;
    private float waveTimer;
    private float waveStartY;
    private float waveEndY;

    private float Ppu => settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;
    private float MotionScale => settings != null ? settings.MotionScale : 1f;

    public void Play(AltarTrigger altar)
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                return;
            }
        }

        linePool.Initialize();
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;
        float topY = camPos.y + halfHeight + 0.5f;

        Color pink = settings != null ? settings.Pink : LastLightVFXPalette.Pink;
        Color red = settings != null ? settings.Red : LastLightVFXPalette.Red;

        for (int i = 0; i < lineCount; i++)
        {
            float x = camPos.x + ((i + 0.5f) / lineCount - 0.5f) * halfWidth * 1.7f;
            Vector3 pos = new Vector3(x, topY + (i % 2) * 0.75f, 0f);
            Vector3 velocity = Vector3.down * lineSpeed * MotionScale * (0.85f + Random.value * 0.3f);
            Sprite sprite = i % 2 == 0 ? lineLong : lineShort;
            int slot = linePool.Spawn(PixelPositionUtility.Snap(pos, Ppu), velocity,
                lineLifetime * (0.85f + Random.value * 0.3f), sprite, i % 2 == 0 ? red : pink);
            if (slot >= 0)
            {
                linePool.Renderer(slot).transform.localScale = new Vector3(1f, 2f, 1f);
            }
        }

        if (waveRenderer != null)
        {
            waveStartY = topY;
            // The wave stops above the lower input third of the screen.
            waveEndY = camPos.y - halfHeight * 0.2f;
            waveTimer = waveDuration;
            waveRenderer.color = pink;
            waveRenderer.enabled = true;
            waveRenderer.transform.position = PixelPositionUtility.Snap(new Vector3(camPos.x, waveStartY, 0f), Ppu);
        }
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Public so editor validation can drive time without Play Mode.
    public void Tick(float dt)
    {
        linePool.Step(dt, Ppu);

        // Keep the lower input area clear: expire lines below the wave floor.
        for (int i = 0; i < linePool.Capacity; i++)
        {
            if (linePool.IsActive(i) && cam != null
                && linePool.Renderer(i).transform.position.y < cam.transform.position.y - cam.orthographicSize * 0.25f)
            {
                linePool.Despawn(i);
            }
        }

        if (waveTimer > 0f && waveRenderer != null)
        {
            waveTimer -= dt;
            if (waveTimer <= 0f)
            {
                waveRenderer.enabled = false;
            }
            else
            {
                float progress = 1f - waveTimer / waveDuration;
                float stepped = Mathf.Floor(progress * waveSteps) / waveSteps;
                float y = Mathf.Lerp(waveStartY, waveEndY, stepped);
                Vector3 pos = waveRenderer.transform.position;
                pos.y = y;
                waveRenderer.transform.position = PixelPositionUtility.Snap(pos, Ppu);
            }
        }
    }

    public void ResetRun()
    {
        linePool.DespawnAll();
        waveTimer = 0f;
        if (waveRenderer != null)
        {
            waveRenderer.enabled = false;
        }
    }
}
