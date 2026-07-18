using System;
using UnityEngine;

public class ChoiceGate : MonoBehaviour, IResettable
{
    [SerializeField] private AltarTrigger altarA;
    [SerializeField] private AltarTrigger altarB;

    public event Action<CostData> CostChosen;
    // Presentation-only: which altar was committed to (fires alongside CostChosen).
    public event Action<AltarTrigger> AltarChosen;

    public AltarTrigger AltarA => altarA;
    public AltarTrigger AltarB => altarB;

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
            return;
        }

        hasChosen = true;
        SetAltarsInteractable(false);
        GameAudio.Instance?.PlayAltarApproach();
        CostChosen?.Invoke(altar.Offer);
        AltarChosen?.Invoke(altar);
    }

    private void SetAltarsInteractable(bool interactable)
    {
        altarA.GetComponent<Collider2D>().enabled = interactable;
        altarB.GetComponent<Collider2D>().enabled = interactable;
    }

    public void ResetRun()
    {
        hasChosen = false;
        SetAltarsInteractable(true);
    }
}
