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
        // A hitched frame can scroll the stage past hazards and gates in one step
        // (observed on-device in the G3 pass); flag it so skipped content is explainable.
        if (deltaTime > 0.25f)
        {
            Debug.LogWarning($"Frame delta {deltaTime:0.###}s scrolled stage {currentIndex + 1} by {scrollSpeed * deltaTime:0.##} units in one step; hazards or gates may have been skipped.", this);
        }

        var stage = stages[currentIndex];
        stage.position += Vector3.down * scrollSpeed * deltaTime;

        var endMarker = stage.Find("EndMarker");
        if (endMarker.position.y <= player.position.y)
        {
            CompleteCurrentStage();
        }
    }

    public void CompleteCurrentStage()
    {
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

        for (int i = 0; i < stages.Length; i++)
        {
            stages[i].gameObject.SetActive(false);
            stages[i].position = basePositions[i];
        }
    }
}
