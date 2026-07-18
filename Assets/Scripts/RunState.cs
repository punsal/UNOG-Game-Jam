using System.Collections.Generic;

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
