using System.Linq;
using UnityEngine;

public class RunController : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private PlayerHealth playerHealth;
    private IResettable[] resettables;

private void Awake()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        resettables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).OfType<IResettable>().ToArray();
    }

    private void OnEnable()
    {
        playerHealth.Died += HandleDeath;
    }

    private void OnDisable()
    {
        playerHealth.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        Restart();
    }

    public void Restart()
    {
        foreach (var resettable in resettables)
        {
            resettable.ResetRun();
        }
    }
}
