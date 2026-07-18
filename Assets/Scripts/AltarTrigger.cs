using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AltarTrigger : MonoBehaviour
{
    [SerializeField] private CostData offer;

    public event Action<AltarTrigger> PlayerEntered;

    public CostData Offer => offer;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void SetOffer(CostData newOffer)
    {
        offer = newOffer;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEntered?.Invoke(this);
        }
    }
}
