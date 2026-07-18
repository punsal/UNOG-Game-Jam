using UnityEngine;

public class DecisionCard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TextMesh copyText;

    public void SetOffer(CostData offer)
    {
        if (offer == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        iconRenderer.sprite = offer.Icon;
        copyText.text = offer.CostCopy;
    }
}
