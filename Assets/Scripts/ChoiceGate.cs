using System;
using UnityEngine;

/// <summary>Accepts one altar choice and disables the remaining choice.</summary>
public class ChoiceGate : MonoBehaviour, IResettable
{
    [SerializeField] private AltarTrigger altarA;
    [SerializeField] private AltarTrigger altarB;

    public event Action<CostData> CostChosen;
    // Presentation-only: which altar was committed to (fires alongside CostChosen).
    public event Action<AltarTrigger> AltarChosen;

    public AltarTrigger AltarA => altarA;
    public AltarTrigger AltarB => altarB;
    public bool HasChosen => hasChosen;

    private bool hasChosen;

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

        hasChosen = true;
        AltarTrigger rejected = altar == altarA ? altarB : altarA;
        Debug.Log($"Choice made at '{altar.name}': {DescribeOffer(altar.Offer)}. Rejected '{rejected.name}': {DescribeOffer(rejected.Offer)}.", this);
        SetAltarsInteractable(false);
        GameAudio.Instance?.PlayAltarApproach();
        CostChosen?.Invoke(altar.Offer);
        AltarChosen?.Invoke(altar);
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
        SetAltarsInteractable(true);
    }
}
