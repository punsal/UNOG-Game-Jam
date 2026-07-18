using UnityEngine;

public class ModifierStack
{
    private const float MinSpeedMultiplier = 0.7f;
    private const float MaxBodyScaleMultiplier = 1.25f;
    private const float MinSightMultiplier = 0.65f;

    public float SpeedMultiplier { get; private set; } = 1f;
    public float BodyScaleMultiplier { get; private set; } = 1f;
    public float SightMultiplier { get; private set; } = 1f;

    public void AddSpeedModifier(float delta)
    {
        SpeedMultiplier = Mathf.Max(MinSpeedMultiplier, SpeedMultiplier + delta);
    }

    public void AddBodyScaleModifier(float delta)
    {
        BodyScaleMultiplier = Mathf.Min(MaxBodyScaleMultiplier, BodyScaleMultiplier + delta);
    }

    public void AddSightModifier(float delta)
    {
        SightMultiplier = Mathf.Max(MinSightMultiplier, SightMultiplier + delta);
    }
}
