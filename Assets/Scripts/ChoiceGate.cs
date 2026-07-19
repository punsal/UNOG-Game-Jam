using System;
using UnityEngine;

/// <summary>Turns altar entries into pending offers and commits one accepted choice.</summary>
public class ChoiceGate : MonoBehaviour, IResettable
{
    [SerializeField] private AltarTrigger altarA;
    [SerializeField] private AltarTrigger altarB;

    public event Action<CostData> CostChosen;
    // Presentation-only: which altar was committed to (fires alongside CostChosen).
    public event Action<AltarTrigger> AltarChosen;
    // An altar was entered and awaits confirmation (the popup subscribes).
    public event Action<ChoiceGate, AltarTrigger> AltarEntered;

    public AltarTrigger AltarA => altarA;
    public AltarTrigger AltarB => altarB;
    public bool HasChosen => hasChosen;

    private bool hasChosen;
    private AltarTrigger pendingAltar;

    private void OnEnable()
    {
        if (altarA != null) altarA.PlayerEntered += HandleEntered;
        if (altarB != null) altarB.PlayerEntered += HandleEntered;
    }

    private void OnDisable()
    {
        if (altarA != null) altarA.PlayerEntered -= HandleEntered;
        if (altarB != null) altarB.PlayerEntered -= HandleEntered;
    }

    private void HandleEntered(AltarTrigger altar)
    {
        if (hasChosen)
        {
            Debug.Log($"Ignored entry at '{altar.name}': this gate already committed a choice.", this);
            return;
        }

        if (pendingAltar != null)
        {
            return;
        }

        pendingAltar = altar;
        Debug.Log($"Offer opened at '{altar.name}': {DescribeOffer(altar.Offer)}.", this);
        GameAudio.Instance?.PlayAltarApproach();
        AltarEntered?.Invoke(this, altar);
    }

    /// <summary>Commits the pending offer as this gate's one choice.</summary>
    public void Accept()
    {
        if (hasChosen || pendingAltar == null)
        {
            return;
        }

        var altar = pendingAltar;
        pendingAltar = null;
        hasChosen = true;
        AltarTrigger rejected = altar == altarA ? altarB : altarA;
        Debug.Log($"Choice made at '{altar.name}': {DescribeOffer(altar.Offer)}. Rejected '{rejected.name}': {DescribeOffer(rejected.Offer)}.", this);
        SetAltarsInteractable(false);
        CostChosen?.Invoke(altar.Offer);
        AltarChosen?.Invoke(altar);
    }

    /// <summary>Dismisses the pending offer; both altars stay armed.</summary>
    public void Decline()
    {
        if (pendingAltar == null)
        {
            return;
        }

        Debug.Log($"Offer declined at '{pendingAltar.name}'.", this);
        pendingAltar = null;
    }

    private static string DescribeOffer(CostData offer)
    {
        return offer != null ? $"{offer.OfferName} ({offer.CostType} {offer.Value})" : "Risk (no cost)";
    }

    private void SetAltarsInteractable(bool interactable)
    {
        altarA.GetComponent<Collider2D>().enabled = interactable;
        altarB.GetComponent<Collider2D>().enabled = interactable;
    }

    public void ResetRun()
    {
        if (hasChosen)
        {
            Debug.Log("Choice gate re-armed for the new run.", this);
        }
        hasChosen = false;
        pendingAltar = null;
        SetAltarsInteractable(true);
    }
}
