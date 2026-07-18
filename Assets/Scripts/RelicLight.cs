using System;
using UnityEngine;

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
        LightChanged?.Invoke(currentLight);
    }

    public void ResetRun()
    {
        currentLight = startLight;
        LightChanged?.Invoke(currentLight);
    }
}
