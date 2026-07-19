using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the run ending and handles retry requests.</summary>
public class OutcomeScreen : MonoBehaviour, IResettable
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private Button retryButton;
    [SerializeField] private TextMeshProUGUI retryButtonText;
    // Beat for the final light transfer to play at the shrine before the
    // opaque panel covers it.
    [SerializeField] private float arrivalDelay = 2.4f;

    private Coroutine showRoutine;
    [SerializeField] private EndingResolver endingResolver;
    [SerializeField] private RunController runController;

    private void Awake()
    {
        Hide();
        retryButton.onClick.AddListener(HandleRetry);
    }

    private void OnEnable()
    {
        endingResolver.RunEnded += Show;
    }

    private void OnDisable()
    {
        endingResolver.RunEnded -= Show;
    }

    private void OnDestroy()
    {
        retryButton.onClick.RemoveListener(HandleRetry);
    }

    public void Show(EndingType ending)
    {
        Debug.Log($"Showing {ending} outcome screen after {arrivalDelay:0.#}s arrival beat.", this);
        endingText.text = ending == EndingType.Dark ? "Consumed by Darkness" : "The Light Endures";
        if (retryButtonText != null)
        {
            retryButtonText.text = ending == EndingType.Dark ? "Villain" : "Hero";
        }
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
        }
        showRoutine = StartCoroutine(ShowAfterArrival());
    }

    private IEnumerator ShowAfterArrival()
    {
        yield return new WaitForSecondsRealtime(arrivalDelay);
        showRoutine = null;
        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }
        panelRoot.SetActive(false);
    }

    private void HandleRetry()
    {
        Hide();
        GameAudio.Instance?.RestartRun();
        runController.Restart();
    }

    public void ResetRun()
    {
        Hide();
    }
}
