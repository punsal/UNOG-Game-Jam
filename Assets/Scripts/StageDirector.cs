using System;
using UnityEngine;

/// <summary>Moves stages through the play area in sequence.</summary>
public class StageDirector : MonoBehaviour, IResettable
{
    [SerializeField] private Transform[] stages;
    [SerializeField] private Transform player;
    [SerializeField] private EndingResolver endingResolver;
    [SerializeField] private RelicLight relicLight;
    [SerializeField] private float scrollSpeed = 4f;
    [SerializeField] private float spawnY = 20f;

    public event Action<int> StageChanged;

    public int CurrentStageNumber => currentIndex + 1;

    private Vector3[] basePositions;
    private int currentIndex = -1;
    private bool scrolling;
    private float stageStartTime;
    private ChoiceGate activeGate;
    private Collider2D activeGateAltarCollider;
    private bool holdLogged;

    private void Awake()
    {
        basePositions = new Vector3[stages.Length];
        for (int i = 0; i < stages.Length; i++)
        {
            basePositions[i] = stages[i].position;
            stages[i].gameObject.SetActive(false);
        }
    }

    public void StartStage(int index)
    {
        if (index < 0 || index >= stages.Length)
        {
            Debug.LogError($"Cannot start stage index {index}; configured count is {stages.Length}.", this);
            return;
        }

        currentIndex = index;
        var stage = stages[index];
        var basePos = basePositions[index];
        stage.position = new Vector3(basePos.x, spawnY, basePos.z);
        stage.gameObject.SetActive(true);
        scrolling = true;
        stageStartTime = Time.time;
        activeGate = stage.GetComponentInChildren<ChoiceGate>(true);
        activeGateAltarCollider = activeGate != null ? activeGate.AltarA.GetComponent<Collider2D>() : null;
        holdLogged = false;
        Debug.Log($"Stage {index + 1}/{stages.Length} started.", this);
        StageChanged?.Invoke(index);
    }

    private void Update()
    {
        if (!scrolling)
        {
            return;
        }

        Tick(Time.deltaTime);
    }

    private void Tick(float deltaTime)
    {
        var stage = stages[currentIndex];
        float move = scrollSpeed * deltaTime;

        // An unchosen gate stops the scroll at the player's level: the run cannot
        // continue until a sacrifice is committed. This also prevents a hitched
        // frame from carrying the stage past the gate.
        if (activeGate != null && !activeGate.HasChosen)
        {
            // Transform-based, not Collider2D.bounds: bounds are only valid after a
            // physics step, which cannot be assumed at this read point.
            float altarCenterY = activeGateAltarCollider.transform.TransformPoint(activeGateAltarCollider.offset).y;
            float distanceToHold = altarCenterY - player.position.y;
            if (distanceToHold < move)
            {
                move = Mathf.Max(0f, distanceToHold);
                if (!holdLogged)
                {
                    holdLogged = true;
                    Debug.Log($"Stage {currentIndex + 1} held at the choice gate; waiting for a sacrifice.", this);
                }
            }
        }

        // A hitched frame can scroll the stage past hazards in one step (observed
        // on-device in the G3 pass); flag the movement actually applied so skipped
        // content is explainable. The gate hold above already clamps gate skips.
        if (deltaTime > 0.25f && move > 0f)
        {
            Debug.LogWarning($"Frame delta {deltaTime:0.###}s scrolled stage {currentIndex + 1} by {move:0.##} units in one step; hazards may have been skipped.", this);
        }

        stage.position += Vector3.down * move;

        var endMarker = stage.Find("EndMarker");
        if (endMarker.position.y <= player.position.y)
        {
            CompleteCurrentStage();
        }
    }

    public void CompleteCurrentStage()
    {
        if (activeGate != null && !activeGate.HasChosen)
        {
            Debug.LogWarning($"Stage {currentIndex + 1} completed without a committed choice; check the gate/EndMarker ordering in this stage.", this);
        }

        Debug.Log($"Stage {currentIndex + 1}/{stages.Length} completed in {Time.time - stageStartTime:0.#}s.", this);
        scrolling = false;
        stages[currentIndex].gameObject.SetActive(false);

        if (currentIndex >= stages.Length - 1)
        {
            endingResolver.Resolve(relicLight.CurrentLight);
        }
        else
        {
            StartStage(currentIndex + 1);
        }
    }

    public void ResetRun()
    {
        scrolling = false;
        currentIndex = -1;
        activeGate = null;
        activeGateAltarCollider = null;

        for (int i = 0; i < stages.Length; i++)
        {
            stages[i].gameObject.SetActive(false);
            stages[i].position = basePositions[i];
        }
    }
}
