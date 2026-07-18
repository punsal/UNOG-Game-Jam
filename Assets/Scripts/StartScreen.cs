using UnityEngine;
using UnityEngine.UI;

/// <summary>Controls the initial start prompt and drag hint.</summary>
public class StartScreen : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject dragHint;
    [SerializeField] private Button startButton;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private RunController runController;

    private bool hasMoved;

    private void Awake()
    {
        inputReader.SetInputEnabled(false);
        startPanel.SetActive(true);
        dragHint.SetActive(false);
        startButton.onClick.AddListener(HandleTapStart);
    }

    private void OnEnable()
    {
        inputReader.OnDrag += HandleFirstDrag;
    }

    private void OnDisable()
    {
        inputReader.OnDrag -= HandleFirstDrag;
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(HandleTapStart);
    }

    public void HandleTapStart()
    {
        Debug.Log("Start requested.", this);
        startPanel.SetActive(false);
        dragHint.SetActive(true);
        hasMoved = false;
        inputReader.SetInputEnabled(true);
        GameAudio.Instance?.PlayLastLightPing();
        runController.Restart();
    }

    private void HandleFirstDrag(float delta)
    {
        if (hasMoved)
        {
            return;
        }

        hasMoved = true;
        dragHint.SetActive(false);
    }
}
