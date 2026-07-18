using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
/// <summary>Reports when the player enters an altar and exposes its offer.</summary>
public class AltarTrigger : MonoBehaviour
{
    [SerializeField] private CostData offer;
    [SerializeField] private DecisionCard decisionCard;

    public event Action<AltarTrigger> PlayerEntered;

    public CostData Offer => offer;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void SetOffer(CostData newOffer)
    {
        offer = newOffer;

        if (decisionCard != null)
        {
            decisionCard.SetOffer(newOffer);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEntered?.Invoke(this);
        }
    }
}
