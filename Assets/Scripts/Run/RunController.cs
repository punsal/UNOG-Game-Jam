using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>Coordinates player death and run resets.</summary>
public class RunController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private StageDirector stageDirector;
    [SerializeField] private DeathScreen deathScreen;
    // Beat between the death sting/VFX and the death screen appearing.
    [SerializeField] private float deathScreenDelay = 0.45f;

    private PlayerHealth playerHealth;
    private InputReader inputReader;
    private IResettable[] resettables;
    private Coroutine deathScreenRoutine;

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
        if (deathScreenRoutine != null)
        {
            return;
        }

        Debug.Log($"Player died on stage {stageDirector.CurrentStageNumber}; showing death screen.", this);
        GameAudio.Instance?.PlayDeath();
        if (inputReader != null)
        {
            inputReader.SetInputEnabled(false);
        }
        // Freeze the run where it died so stages cannot complete behind the screen.
        stageDirector.StopScrolling();
        deathScreenRoutine = StartCoroutine(ShowDeathScreen());
    }

    private IEnumerator ShowDeathScreen()
    {
        yield return new WaitForSecondsRealtime(deathScreenDelay);
        deathScreenRoutine = null;
        deathScreen.Show();
    }

    public void Restart()
    {
        // An external restart (e.g. retry button) supersedes a pending death screen.
        if (deathScreenRoutine != null)
        {
            StopCoroutine(deathScreenRoutine);
            deathScreenRoutine = null;
        }

        foreach (var resettable in resettables)
        {
            resettable.ResetRun();
        }

        stageDirector.StartStage(0);
        if (inputReader != null)
        {
            inputReader.SetInputEnabled(true);
        }
        GameAudio.Instance?.StartRun();
        Debug.Log($"Run restarted. Reset {resettables.Length} objects.", this);
    }
}
