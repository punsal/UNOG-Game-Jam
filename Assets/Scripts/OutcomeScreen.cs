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
        Debug.Log($"Showing {ending} outcome screen.", this);
        endingText.text = ending == EndingType.Dark ? "Consumed by Darkness" : "The Light Endures";
        if (retryButtonText != null)
        {
            retryButtonText.text = ending == EndingType.Dark ? "Villain" : "Hero";
        }
        panelRoot.SetActive(true);
    }

    public void Hide()
    {
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
