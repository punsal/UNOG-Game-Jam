using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObstacleBase : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private HazardType hazardType = HazardType.Spike;
    // Telegraph fires this many world units above the camera's visible top edge.
    [SerializeField] private float telegraphLeadDistance = 2f;
    // Fallback trigger line if no camera exists (tune per level in that case).
    [SerializeField] private float telegraphY = 12f;

    private Camera gameCamera;
    private bool telegraphed;

    // Concrete obstacle scripts may override to force their type in code;
    // otherwise the serialized Inspector value (default Spike) is used.
    protected virtual HazardType TelegraphHazardType => hazardType;

    // CameraFitter width-locks ortho size per aspect, so the visible top edge
    // must be derived from the camera rather than a fixed world Y.
    private float TelegraphTriggerY => gameCamera != null
        ? gameCamera.transform.position.y + gameCamera.orthographicSize + telegraphLeadDistance
        : telegraphY;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    // Stages are deactivated and repositioned between runs; re-arm the telegraph then.
    private void OnEnable()
    {
        telegraphed = false;
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (telegraphed || transform.position.y > TelegraphTriggerY)
        {
            return;
        }

        telegraphed = true;
        GameAudio.Instance?.PlayHazardTelegraph(TelegraphHazardType);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.ApplyDamage(damage, transform.position);
        }
    }
}
