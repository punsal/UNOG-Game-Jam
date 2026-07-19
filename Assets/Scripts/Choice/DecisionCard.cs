using TMPro;
using UnityEngine;

/// <summary>Displays the offer assigned to an altar.</summary>
public class DecisionCard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    
    public void SetOffer(CostData offer)
    {
        if (offer == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        iconRenderer.sprite = offer.Icon;
    }
}
