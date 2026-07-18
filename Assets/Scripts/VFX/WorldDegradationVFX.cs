using UnityEngine;

/// <summary>
/// The world dims as the run costs mount: a quantized navy tint deepens with
/// section progress and committed choices. Event-driven (no polling), subtle by
/// budget (max alpha in ScreenFXOverlay stays hazard-readable), resets per run.
/// </summary>
public class WorldDegradationVFX : MonoBehaviour, IResettable
{
    [SerializeField] private ScreenFXOverlay overlay;
    [Tooltip("Degradation points: one per two sections plus one per committed choice; level = points, clamped to overlay levels.")]
    [SerializeField] private int pointsPerChoice = 1;

    private StageDirector stageDirector;
    private ChoiceGate[] gates;
    private int stageIndex;
    private int choicePoints;

    private void Start()
    {
        stageDirector = FindFirstObjectByType<StageDirector>(FindObjectsInactive.Include);
        if (stageDirector != null)
        {
            stageDirector.StageChanged += HandleStageChanged;
        }

        gates = FindObjectsByType<ChoiceGate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].AltarChosen += HandleAltarChosen;
        }
        Apply();
    }

    private void OnDestroy()
    {
        if (stageDirector != null)
        {
            stageDirector.StageChanged -= HandleStageChanged;
        }
        if (gates != null)
        {
            for (int i = 0; i < gates.Length; i++)
            {
                if (gates[i] != null)
                {
                    gates[i].AltarChosen -= HandleAltarChosen;
                }
            }
        }
    }

    public void HandleStageChanged(int index)
    {
        stageIndex = index;
        Apply();
    }

    private void HandleAltarChosen(AltarTrigger altar)
    {
        // Every commitment degrades the world: paid costs and accepted risks alike.
        choicePoints += pointsPerChoice;
        Apply();
    }

    private void Apply()
    {
        if (overlay == null)
        {
            return;
        }
        int level = Mathf.Clamp(stageIndex / 2 + choicePoints / 2, 0, 3);
        overlay.SetDegradationLevel(level);
    }

    public int CurrentLevel => Mathf.Clamp(stageIndex / 2 + choicePoints / 2, 0, 3);

    public void ResetRun()
    {
        stageIndex = 0;
        choicePoints = 0;
        Apply();
    }
}
