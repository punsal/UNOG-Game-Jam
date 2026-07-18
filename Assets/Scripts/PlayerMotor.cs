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
        float limit = MovementHalfWidth();
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x + worldDelta, -limit, limit);
        transform.position = position;
    }

    // Keep the player inside the visible corridor: screen half-width minus the
    // wall (0.5) and the player's own half-extent (~1), whichever is tighter.
    private float MovementHalfWidth()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return halfWidth;
        }

        return Mathf.Min(halfWidth, cam.orthographicSize * cam.aspect - 1.5f);
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
