using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Applies each accepted sacrifice's relief to the next stage.</summary>
public class BenefitApplier : MonoBehaviour, IResettable
{
    [SerializeField] private StageDirector stageDirector;
    // Relief tuning. Body slows blades; Blood narrows spikes, widening the
    // safe corridor; Light removes the next stage's first hazard outright;
    // Sight slows the next stage's scroll, buying reading time.
    [SerializeField] private float bladeSlowMultiplier = 0.6f;
    [SerializeField] private float spikeNarrowScale = 0.7f;
    [SerializeField] private float scrollSlowMultiplier = 0.9f;

    private ChoiceGate[] gates;
    private bool pendingHazardRemoval;  // Light
    private bool pendingBladeSlow;      // Body
    private bool pendingCorridorWiden;  // Blood
    private bool pendingScrollSlow;     // Sight
    // One-shot relief: undone when the next stage starts or the run resets.
    private readonly List<Action> undoActions = new List<Action>();

    private void OnEnable()
    {
        stageDirector.StageChanged += HandleStageChanged;
    }

    private void OnDisable()
    {
        stageDirector.StageChanged -= HandleStageChanged;
    }

    private void Start()
    {
        // Gates live inside inactive stage chunks; subscribe once for the scene's lifetime.
        gates = FindObjectsByType<ChoiceGate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var gate in gates)
        {
            gate.CostChosen += HandleCostChosen;
        }
    }

    private void OnDestroy()
    {
        if (gates == null)
        {
            return;
        }
        foreach (var gate in gates)
        {
            if (gate != null)
            {
                gate.CostChosen -= HandleCostChosen;
            }
        }
    }

    private void HandleCostChosen(CostData cost)
    {
        // Null offer = the risk altar: nothing was sacrificed, nothing is owed.
        if (cost == null)
        {
            return;
        }

        switch (cost.CostType)
        {
            case CostType.Light:
                pendingHazardRemoval = true;
                break;
            case CostType.Body:
                pendingBladeSlow = true;
                break;
            case CostType.Blood:
                pendingCorridorWiden = true;
                break;
            case CostType.Sight:
                pendingScrollSlow = true;
                break;
            default:
                return;
        }
        Debug.Log($"Benefit pended for the next stage: {cost.CostType}.", this);
    }

    private void HandleStageChanged(int index)
    {
        // Relief lasts one stage; restore before applying anything new.
        foreach (var undo in undoActions)
        {
            undo();
        }
        undoActions.Clear();

        var stage = stageDirector.CurrentStage;
        if (stage == null)
        {
            return;
        }

        if (pendingHazardRemoval)
        {
            pendingHazardRemoval = false;
            var target = FirstHazardRoot(stage);
            if (target != null)
            {
                target.gameObject.SetActive(false);
                undoActions.Add(() => target.gameObject.SetActive(true));
                Debug.Log($"Benefit applied (Light): removed hazard '{target.name}' from stage {index + 1}.", this);
            }
            else
            {
                Debug.Log($"Benefit wasted (Light): stage {index + 1} has no hazard to remove.", this);
            }
        }

        if (pendingBladeSlow)
        {
            pendingBladeSlow = false;
            var blades = stage.GetComponentsInChildren<MovingBlade>(true);
            foreach (var blade in blades)
            {
                blade.SetSpeedMultiplier(bladeSlowMultiplier);
                var captured = blade;
                undoActions.Add(() => captured.SetSpeedMultiplier(1f));
            }
            Debug.Log(blades.Length > 0
                ? $"Benefit applied (Body): slowed {blades.Length} blade(s) to x{bladeSlowMultiplier:0.##} in stage {index + 1}."
                : $"Benefit wasted (Body): stage {index + 1} has no blades to slow.", this);
        }

        if (pendingScrollSlow)
        {
            pendingScrollSlow = false;
            stageDirector.ApplyScrollSpeedMultiplier(scrollSlowMultiplier);
            Debug.Log($"Benefit applied (Sight): stage {index + 1} scrolls at x{scrollSlowMultiplier:0.##}.", this);
        }

        if (pendingCorridorWiden)
        {
            pendingCorridorWiden = false;
            int narrowed = 0;
            foreach (var root in HazardRoots(stage))
            {
                if (root.GetComponentInChildren<MovingBlade>(true) != null)
                {
                    continue;
                }
                var captured = root;
                var originalScale = captured.localScale;
                captured.localScale = new Vector3(originalScale.x * spikeNarrowScale, originalScale.y, originalScale.z);
                undoActions.Add(() => captured.localScale = originalScale);
                narrowed++;
            }
            Debug.Log(narrowed > 0
                ? $"Benefit applied (Blood): narrowed {narrowed} spike(s) to x{spikeNarrowScale:0.##} width in stage {index + 1}."
                : $"Benefit wasted (Blood): stage {index + 1} has no spikes to narrow.", this);
        }
    }

    private static List<Transform> HazardRoots(Transform stage)
    {
        var roots = new List<Transform>();
        foreach (Transform child in stage)
        {
            if (child.GetComponentInChildren<ObstacleBase>(true) != null)
            {
                roots.Add(child);
            }
        }
        return roots;
    }

    // The hazard the player meets first: stages scroll down, so the lowest
    // local Y arrives at the player earliest (see the stage scroll convention).
    private static Transform FirstHazardRoot(Transform stage)
    {
        Transform first = null;
        foreach (var root in HazardRoots(stage))
        {
            if (first == null || root.localPosition.y < first.localPosition.y)
            {
                first = root;
            }
        }
        return first;
    }

    public void ResetRun()
    {
        foreach (var undo in undoActions)
        {
            undo();
        }
        undoActions.Clear();
        pendingHazardRemoval = false;
        pendingBladeSlow = false;
        pendingCorridorWiden = false;
        pendingScrollSlow = false;
    }
}
