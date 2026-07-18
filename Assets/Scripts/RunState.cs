using System.Collections.Generic;

/// <summary>Stores choices and modifiers for the current run.</summary>
public class RunState
{
    public ModifierStack Modifiers { get; } = new ModifierStack();
    public int StageIndex { get; private set; }
    public IReadOnlyList<CostData> ChosenCosts => chosenCosts;

    private readonly List<CostData> chosenCosts = new List<CostData>();

    public void AdvanceStage()
    {
        StageIndex++;
    }

    public void RecordChoice(CostData cost)
    {
        chosenCosts.Add(cost);
    }
}
