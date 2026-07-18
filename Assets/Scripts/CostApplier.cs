using UnityEngine;

/// <summary>Applies chosen costs to the current run.</summary>
public class CostApplier : MonoBehaviour, IResettable
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMotor playerMotor;
    [SerializeField] private RelicLight relicLight;

    private RunState runState = new RunState();
    private ChoiceGate[] gates;

    public RunState RunState => runState;

    private void Start()
    {
        // Gates live inside inactive stage chunks; subscribe once for the scene's lifetime.
        gates = FindObjectsByType<ChoiceGate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].CostChosen += HandleCostChosen;
        }
    }

    private void OnDestroy()
    {
        if (gates == null)
        {
            return;
        }
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null)
            {
                gates[i].CostChosen -= HandleCostChosen;
            }
        }
    }

    private void HandleCostChosen(CostData cost)
    {
        // Null offer = the risk altar: everything is kept, no cost to pay.
        if (cost == null)
        {
            return;
        }
        Apply(cost);
    }

    public void Apply(CostData cost)
    {
        if (cost == null)
        {
            Debug.LogWarning("Cannot apply a null cost.", this);
            return;
        }

        switch (cost.CostType)
        {
            case CostType.Blood:
                playerHealth.ApplyMaxHealthModifier(Mathf.RoundToInt(cost.Value));
                // Blood preserves all audio identity layers and represents increased
                // immediate danger, so it raises the Danger stem rather than removing Body.
                GameAudio.Instance?.ChooseRisk();
                break;
            case CostType.Light:
                relicLight.ChangeLight(cost.Value);
                GameAudio.Instance?.ChooseSacrifice(SacrificeType.Light);
                break;
            case CostType.Sight:
                runState.Modifiers.AddSightModifier(cost.Value);
                GameAudio.Instance?.ChooseSacrifice(SacrificeType.Senses);
                break;
            case CostType.Body:
                ApplyBodyCost(cost.Value);
                GameAudio.Instance?.ChooseSacrifice(SacrificeType.Body);
                break;
        }

        runState.RecordChoice(cost);
        Debug.Log($"Applied {cost.CostType} cost ({cost.Value}).", this);
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
