using System;
using UnityEngine;

/// <summary>Tracks the relic's remaining light.</summary>
public class RelicLight : MonoBehaviour, IResettable
{
    public event Action<float> LightChanged;

    [SerializeField] private float startLight = 100f;

    private float currentLight;

    public float CurrentLight => currentLight;

    private void Awake()
    {
        currentLight = startLight;
    }

    public void ChangeLight(float delta)
    {
        currentLight = Mathf.Clamp(currentLight + delta, 0f, 100f);
        Debug.Log($"Relic light changed by {delta:0.#}. Remaining: {currentLight:0.#}.", this);
        GameAudio.Instance?.SetLightLevel(currentLight / 100f);
        LightChanged?.Invoke(currentLight);
    }

    public void ResetRun()
    {
        currentLight = startLight;
        LightChanged?.Invoke(currentLight);
    }
}
