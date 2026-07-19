using UnityEngine;

public enum CostType { Blood, Light, Sight, Body }

[CreateAssetMenu(fileName = "CostData", menuName = "Last Light/Cost Data")]
/// <summary>Defines one altar cost and its display content.</summary>
public class CostData : ScriptableObject
{
    [SerializeField] private Sprite icon;
    [SerializeField] private string offerName;
    [SerializeField] [TextArea] private string benefitCopy;
    [SerializeField] [TextArea] private string costCopy;
    [SerializeField] private CostType costType;

    // Blood/Light/Sight: negative = reduction, matching the resource's own scale.
    // Body only: negative = speed penalty (fraction), positive = size penalty (fraction) -
    // mutually exclusive per offer per the Content & Tuning rule (never both in one offer).
    [SerializeField] private float value;

    public Sprite Icon => icon;
    public string OfferName => offerName;
    public string BenefitCopy => benefitCopy;
    public string CostCopy => costCopy;
    public CostType CostType => costType;
    public float Value => value;
}
