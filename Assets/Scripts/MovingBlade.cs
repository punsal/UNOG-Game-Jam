using UnityEngine;

/// <summary>Moves a blade horizontally with a sine wave.</summary>
public class MovingBlade : MonoBehaviour, IResettable
{
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float phase = 0f;

    private Vector3 startingLocalPosition;
    private float elapsed;

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

    private void ApplyOscillation(float time)
    {
        var position = startingLocalPosition;
        position.x += amplitude * Mathf.Sin(speed * time + phase);
        transform.localPosition = position;
    }

    public void ResetRun()
    {
        elapsed = 0f;
        ApplyOscillation(0f);
    }
}
