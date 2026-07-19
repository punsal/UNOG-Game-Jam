using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Confirms altar offers: shows icon, benefit and cost, then accepts or declines.</summary>
public class ChoicePopup : MonoBehaviour, IResettable
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI benefitText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private Transform player;
    [SerializeField] private InputReader inputReader;

    private ChoiceGate[] gates;
    private ChoiceGate activeGate;

    private void Awake()
    {
        Hide();
        acceptButton.onClick.AddListener(HandleAccept);
        declineButton.onClick.AddListener(HandleDecline);
    }

    private void Start()
    {
        // Gates live inside inactive stage chunks; subscribe once for the scene's lifetime.
        gates = FindObjectsByType<ChoiceGate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var gate in gates)
        {
            gate.AltarEntered += HandleAltarEntered;
        }
    }

    private void OnDestroy()
    {
        acceptButton.onClick.RemoveListener(HandleAccept);
        declineButton.onClick.RemoveListener(HandleDecline);
        if (gates == null)
        {
            return;
        }
        foreach (var gate in gates)
        {
            if (gate != null)
            {
                gate.AltarEntered -= HandleAltarEntered;
            }
        }
    }

    private void HandleAltarEntered(ChoiceGate gate, AltarTrigger altar)
    {
        var offer = altar.Offer;
        if (offer == null)
        {
            // Risk altar: there is nothing to weigh, entering accepts it.
            gate.Accept();
            return;
        }

        activeGate = gate;
        iconImage.sprite = offer.Icon;
        titleText.text = offer.OfferName;
        benefitText.text = offer.BenefitCopy;
        costText.text = offer.CostCopy;
        Debug.Log($"Choice popup shown for '{offer.OfferName}'.", this);
        panelRoot.SetActive(true);
        if (inputReader != null)
        {
            inputReader.SetInputEnabled(false);
        }
    }

    private void HandleAccept()
    {
        if (activeGate == null)
        {
            return;
        }

        var gate = activeGate;
        activeGate = null;
        Hide();
        RestoreInput();
        gate.Accept();
    }

    private void HandleDecline()
    {
        if (activeGate == null)
        {
            return;
        }

        var gate = activeGate;
        activeGate = null;
        Hide();
        RestoreInput();
        // Recentre the traveller so the altar trigger resets and can be re-entered.
        player.position = new Vector3(0f, player.position.y, player.position.z);
        gate.Decline();
    }

    private void RestoreInput()
    {
        if (inputReader != null)
        {
            inputReader.SetInputEnabled(true);
        }
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    public void ResetRun()
    {
        Hide();
        activeGate = null;
    }
}
