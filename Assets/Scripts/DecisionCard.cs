using TMPro;
using UnityEngine;

/// <summary>Displays the offer assigned to an altar.</summary>
public class DecisionCard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TextMeshPro copyText;

    private void Awake()
    {
        // Altars show only their icon in the world; the offer's full benefit
        // and cost text lives in the confirmation popup.
        if (copyText != null)
        {
            copyText.gameObject.SetActive(false);
        }
    }

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
