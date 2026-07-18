using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutcomeScreen : MonoBehaviour, IResettable
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private Button retryButton;
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
        endingText.text = ending == EndingType.Dark ? "Consumed by Darkness" : "The Light Endures";
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
