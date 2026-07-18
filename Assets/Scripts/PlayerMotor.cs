using UnityEngine;

[RequireComponent(typeof(InputReader))]
public class PlayerMotor : MonoBehaviour, IResettable
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float halfWidth = 3.5f;

    private InputReader inputReader;
    private Vector3 startPosition;
    private float speedMultiplier = 1f;

    private void Awake()
    {
        inputReader = GetComponent<InputReader>();
        startPosition = transform.position;
    }

    private void OnEnable()
    {
        inputReader.OnDrag += SetInput;
    }

    private void OnDisable()
    {
        inputReader.OnDrag -= SetInput;
    }

    public void SetInput(float normalizedX)
    {
        float worldDelta = normalizedX * moveSpeed * speedMultiplier;
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x + worldDelta, -halfWidth, halfWidth);
        transform.position = position;
    }

    public void ApplyMovementModifier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

public void ResetRun()
    {
        speedMultiplier = 1f;
        transform.position = startPosition;
    }
}
