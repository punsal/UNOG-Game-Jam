using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObstacleBase : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
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
            health.ApplyDamage(damage);
        }
    }
}
