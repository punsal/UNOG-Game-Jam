using UnityEngine;

public class MovingBlade : MonoBehaviour, IResettable
{
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float phase = 0f;

    private Vector3 startPosition;
    private float elapsed;

    private void Awake()
    {
        startPosition = transform.position;
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
        var position = startPosition;
        position.x += amplitude * Mathf.Sin(speed * time + phase);
        transform.position = position;
    }

    public void ResetRun()
    {
        elapsed = 0f;
        ApplyOscillation(0f);
    }
}
