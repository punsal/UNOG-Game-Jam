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
    // Target scroll seconds per stage. Retuned 2026-07-19 for the jam's
    // under-3-minute total: 154s scroll plus decision time. Scroll speed is
    // derived from each stage's actual EndMarker distance, so stage length can
    // change without retuning. A missing or zero entry falls back to scrollSpeed.
    [SerializeField] private float[] stageDurations = { 22f, 22f, 26f, 26f, 28f, 30f };

    public event Action<int> StageChanged;

    public int CurrentStageNumber => currentIndex + 1;

    public Transform CurrentStage => currentIndex >= 0 ? stages[currentIndex] : null;

    private Vector3[] basePositions;
    private int currentIndex = -1;
    private bool scrolling;
    private float stageStartTime;
    private ChoiceGate activeGate;
    private Collider2D activeGateAltarCollider;
    private bool holdLogged;
    private float currentScrollSpeed;

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

        float travelDistance = stage.Find("EndMarker").position.y - player.position.y;
        float duration = stageDurations != null && index < stageDurations.Length ? stageDurations[index] : 0f;
        currentScrollSpeed = duration > 0f ? travelDistance / duration : scrollSpeed;
        Debug.Log($"Stage {index + 1}/{stages.Length} started; {travelDistance:0.#} units at speed {currentScrollSpeed:0.##} targeting {(duration > 0f ? duration : travelDistance / scrollSpeed):0.#}s.", this);
        StageChanged?.Invoke(index);
    }

    // One long frame (GC pause, thermal throttle, focus loss) must not scroll the
    // stage past hazards in a single step; observed on-device in the G3 pass.
    private const float MaxTickDelta = 0.1f;

    private void Update()
    {
        if (!scrolling)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        if (deltaTime > MaxTickDelta)
        {
            Debug.LogWarning($"Frame delta {deltaTime:0.###}s clamped to {MaxTickDelta:0.##}s for stage scroll.", this);
        }
        Tick(Mathf.Min(deltaTime, MaxTickDelta));
    }

    private void Tick(float deltaTime)
    {
        var stage = stages[currentIndex];
        float move = currentScrollSpeed * deltaTime;

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

        stage.position += Vector3.down * move;

        var endMarker = stage.Find("EndMarker");
        if (endMarker.position.y <= player.position.y)
        {
            CompleteCurrentStage();
        }
    }

    public void StopScrolling()
    {
        scrolling = false;
    }

    // Benefit hook (Clouded Eyes): naturally one-shot, since StartStage
    // recomputes the speed for every stage.
    public void ApplyScrollSpeedMultiplier(float multiplier)
    {
        currentScrollSpeed *= multiplier;
    }

    public void CompleteCurrentStage()
    {
        if (activeGate != null && !activeGate.HasChosen)
        {
            Debug.LogWarning($"Stage {currentIndex + 1} completed without a committed choice; check the gate/EndMarker ordering in this stage.", this);
        }

        Debug.Log($"Stage {currentIndex + 1}/{stages.Length} completed in {Time.time - stageStartTime:0.#}s.", this);
        scrolling = false;

        if (currentIndex >= stages.Length - 1)
        {
            // The final stage stays visible for the light delivery at the
            // shrine; ResetRun hides it with everything else.
            endingResolver.Resolve(relicLight.CurrentLight);
        }
        else
        {
            stages[currentIndex].gameObject.SetActive(false);
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
