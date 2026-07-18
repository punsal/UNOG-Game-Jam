using UnityEngine;

public class CostApplier : MonoBehaviour, IResettable
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMotor playerMotor;
    [SerializeField] private RelicLight relicLight;

    private RunState runState = new RunState();

    public RunState RunState => runState;

    public void Apply(CostData cost)
    {
        switch (cost.CostType)
        {
            case CostType.Blood:
                playerHealth.ApplyMaxHealthModifier(Mathf.RoundToInt(cost.Value));
                break;
            case CostType.Light:
                relicLight.ChangeLight(cost.Value);
                break;
            case CostType.Sight:
                runState.Modifiers.AddSightModifier(cost.Value);
                break;
            case CostType.Body:
                ApplyBodyCost(cost.Value);
                break;
        }

        runState.RecordChoice(cost);
    }

    private void ApplyBodyCost(float value)
    {
        if (value < 0f)
        {
            runState.Modifiers.AddSpeedModifier(value);
            playerMotor.ApplyMovementModifier(runState.Modifiers.SpeedMultiplier);
        }
        else if (value > 0f)
        {
            runState.Modifiers.AddBodyScaleModifier(value);
            float scale = runState.Modifiers.BodyScaleMultiplier;
            playerMotor.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public bool ValidateCaps()
    {
        return runState.Modifiers.SpeedMultiplier >= 0.7f
            && runState.Modifiers.BodyScaleMultiplier <= 1.25f
            && runState.Modifiers.SightMultiplier >= 0.65f
            && playerHealth.MaxHealth >= 1;
    }

    public void ResetRun()
    {
        runState = new RunState();
        playerMotor.transform.localScale = Vector3.one;
    }
}
