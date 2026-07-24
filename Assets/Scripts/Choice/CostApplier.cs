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
        gates = FindObjectsByType<ChoiceGate>(FindObjectsInactive.Include);
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
            Debug.Log("Risk accepted: no cost paid, all resources kept.", this);
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

        // The altar takes, but steadies: every paid sacrifice restores 1 HP,
        // so chip damage is recoverable at gates while in-stage mistakes
        // stay lethal. Risk altars pay nothing and heal nothing.
        playerHealth.Heal(1);

        runState.RecordChoice(cost);
        Debug.Log($"Applied {cost.CostType} cost ({cost.Value}). Run state: health {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}, light {relicLight.CurrentLight:0.#}, speed x{runState.Modifiers.SpeedMultiplier:0.##}, sight x{runState.Modifiers.SightMultiplier:0.##}, scale x{runState.Modifiers.BodyScaleMultiplier:0.##}, choices {runState.ChosenCosts.Count}.", this);

        if (!ValidateCaps())
        {
            Debug.LogWarning("A modifier cap invariant is violated after applying this cost.", this);
        }
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
