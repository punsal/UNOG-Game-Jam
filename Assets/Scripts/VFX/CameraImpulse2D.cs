using UnityEngine;

/// <summary>
/// Short positional camera impulses with zero drift. Applies an additive,
/// pixel-quantized offset to the main camera and always removes exactly what it
/// applied. Lives on the traveller prefab (not the camera) so no scene edits are
/// required; it caches Camera.main once.
/// </summary>
public class CameraImpulse2D : MonoBehaviour
{
    public static CameraImpulse2D Instance { get; private set; }

    [SerializeField] private LastLightVFXSettings settings;
    [SerializeField] private float defaultMagnitude = 0.125f;
    [SerializeField] private float defaultDuration = 0.06f;
    [SerializeField] private float maxDuration = 0.08f;

    private Transform cameraTransform;
    private Vector3 appliedOffset;
    private Vector2 direction;
    private float magnitude;
    private float timer;
    private float duration;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        RemoveOffset();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Play(Vector2 impulseDirection)
    {
        Play(impulseDirection, defaultMagnitude, defaultDuration);
    }

    public void Play(Vector2 impulseDirection, float impulseMagnitude, float impulseDuration)
    {
        if (cameraTransform == null)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }
            cameraTransform = cam.transform;
        }

        float motionScale = settings != null ? settings.MotionScale : 1f;
        direction = impulseDirection.sqrMagnitude > 0.0001f ? impulseDirection.normalized : Vector2.down;
        magnitude = impulseMagnitude * motionScale;
        duration = Mathf.Min(impulseDuration, maxDuration);
        timer = duration;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            return;
        }

        RemoveOffset();

        if (timer <= 0f)
        {
            return;
        }

        timer -= Time.deltaTime;

        // Two-step decay: full offset for the first half, one pixel for the rest.
        float ppu = settings != null ? settings.PixelsPerUnit : PixelPositionUtility.DefaultPixelsPerUnit;
        float strength = timer > duration * 0.5f ? magnitude : 1f / ppu;
        Vector3 offset = new Vector3(direction.x * strength, direction.y * strength, 0f);
        offset.x = Mathf.Round(offset.x * ppu) / ppu;
        offset.y = Mathf.Round(offset.y * ppu) / ppu;

        cameraTransform.position += offset;
        appliedOffset = offset;
    }

    private void RemoveOffset()
    {
        if (cameraTransform != null && appliedOffset != Vector3.zero)
        {
            cameraTransform.position -= appliedOffset;
            appliedOffset = Vector3.zero;
        }
    }
}
