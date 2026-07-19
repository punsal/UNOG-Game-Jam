using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>Coordinates player death and run resets.</summary>
public class RunController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private StageDirector stageDirector;
    // Gap between the death sting and the restart, so the 0.2s music fade
    // and sting are audible before the new stems schedule.
    [SerializeField] private float deathRestartDelay = 0.45f;

    private PlayerHealth playerHealth;
    private InputReader inputReader;
    private IResettable[] resettables;
    private Coroutine deathRestartRoutine;

    private void Awake()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        inputReader = player.GetComponent<InputReader>();
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
        if (deathRestartRoutine != null)
        {
            return;
        }

        Debug.Log($"Player died on stage {stageDirector.CurrentStageNumber}; restarting in {deathRestartDelay:0.##} seconds.", this);
        GameAudio.Instance?.PlayDeath();
        if (inputReader != null)
        {
            inputReader.SetInputEnabled(false);
        }
        deathRestartRoutine = StartCoroutine(DeathRestart());
    }

    private IEnumerator DeathRestart()
    {
        yield return new WaitForSecondsRealtime(deathRestartDelay);
        deathRestartRoutine = null;
        Restart();
        if (inputReader != null)
        {
            inputReader.SetInputEnabled(true);
        }
    }

    public void Restart()
    {
        // An external restart (e.g. retry button) supersedes a pending death restart.
        if (deathRestartRoutine != null)
        {
            StopCoroutine(deathRestartRoutine);
            deathRestartRoutine = null;
        }

        foreach (var resettable in resettables)
        {
            resettable.ResetRun();
        }

        stageDirector.StartStage(0);
        GameAudio.Instance?.StartRun();
        Debug.Log($"Run restarted. Reset {resettables.Length} objects.", this);
    }
}
