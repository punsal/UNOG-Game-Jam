using System.Collections;
using System.Linq;
using UnityEngine;

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
    }
}
