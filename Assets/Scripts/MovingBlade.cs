using UnityEngine;

/// <summary>Moves a blade horizontally with a sine wave.</summary>
public class MovingBlade : MonoBehaviour, IResettable
{
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float phase = 0f;

    private Vector3 startingLocalPosition;
    private float elapsed;
    private float speedMultiplier = 1f;

    private void Awake()
    {
        startingLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        ApplyOscillation(elapsed);
    }

    public void Configure(float amplitude, float speed, float phase)
    {
        this.amplitude = amplitude;
        this.speed = speed;
        this.phase = phase;
    }

    // Benefit hook (Weary Legs): only safe to change at stage start or reset,
    // otherwise the sine phase jumps.
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    private void ApplyOscillation(float time)
    {
        var position = startingLocalPosition;
        position.x += amplitude * Mathf.Sin(speed * speedMultiplier * time + phase);
        transform.localPosition = position;
    }

    public void ResetRun()
    {
        elapsed = 0f;
        speedMultiplier = 1f;
        ApplyOscillation(0f);
    }
}
