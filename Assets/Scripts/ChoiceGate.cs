using System;
using UnityEngine;

public class ChoiceGate : MonoBehaviour, IResettable
{
    [SerializeField] private AltarTrigger altarA;
    [SerializeField] private AltarTrigger altarB;

    public event Action<CostData> CostChosen;

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
        CostChosen?.Invoke(altar.Offer);
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
