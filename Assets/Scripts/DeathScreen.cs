using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the death panel and handles restart requests.</summary>
public class DeathScreen : MonoBehaviour, IResettable
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button restartButton;
    [SerializeField] private RunController runController;

    private void Awake()
    {
        Hide();
        restartButton.onClick.AddListener(HandleRestart);
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(HandleRestart);
    }

    public void Show()
    {
        Debug.Log("Showing death screen.", this);
        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void HandleRestart()
    {
        Debug.Log("Restart requested from death screen.", this);
        Hide();
        GameAudio.Instance?.RestartRun();
        runController.Restart();
    }

    public void ResetRun()
    {
        Hide();
    }
}
