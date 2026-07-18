using System;
using UnityEngine;

/// <summary>
/// Fixed pool over prefab-authored SpriteRenderer children. No Instantiate or
/// Destroy at gameplay time; owners loop over active items for styling.
/// Renderers must start disabled in the prefab.
/// </summary>
[Serializable]
public class FixedSpritePool
{
    [SerializeField] private SpriteRenderer[] renderers;

    private float[] ages;
    private float[] lifetimes;
    private Vector3[] velocities;
    private bool initialized;

    public int Capacity => renderers != null ? renderers.Length : 0;

    public void Initialize()
    {
        if (initialized || renderers == null)
        {
            return;
        }

        ages = new float[renderers.Length];
        lifetimes = new float[renderers.Length];
        velocities = new Vector3[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }
        initialized = true;
    }

    public int Spawn(Vector3 position, Vector3 velocity, float lifetime, Sprite sprite, Color color)
    {
        if (!initialized)
        {
            Initialize();
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null || r.enabled)
            {
                continue;
            }

            r.transform.position = position;
            r.transform.localScale = Vector3.one;
            if (sprite != null)
            {
                r.sprite = sprite;
            }
            r.color = color;
            r.enabled = true;
            ages[i] = 0f;
            lifetimes[i] = lifetime;
            velocities[i] = velocity;
            return i;
        }

        return -1;
    }

    /// <summary>Advances ages and positions; disables expired items. Positions are pixel-snapped.</summary>
    public void Step(float deltaTime, float pixelsPerUnit)
    {
        if (!initialized)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null || !r.enabled)
            {
                continue;
            }

            ages[i] += deltaTime;
            if (ages[i] >= lifetimes[i])
            {
                r.enabled = false;
                continue;
            }

            if (velocities[i] != Vector3.zero)
            {
                Vector3 target = r.transform.position + velocities[i] * deltaTime;
                r.transform.position = PixelPositionUtility.Snap(target, pixelsPerUnit);
            }
        }
    }

    public bool IsActive(int index) => initialized && renderers[index] != null && renderers[index].enabled;
    public SpriteRenderer Renderer(int index) => renderers[index];
    public float Age(int index) => ages[index];
    public float Lifetime(int index) => lifetimes[index];
    public void SetVelocity(int index, Vector3 velocity) => velocities[index] = velocity;
    public Vector3 Velocity(int index) => velocities[index];

    /// <summary>Normalized age 0..1 for stepped fading.</summary>
    public float NormalizedAge(int index)
    {
        float life = lifetimes[index];
        return life > 0f ? Mathf.Clamp01(ages[index] / life) : 1f;
    }

    public void Despawn(int index)
    {
        if (initialized && renderers[index] != null)
        {
            renderers[index].enabled = false;
        }
    }

    public void DespawnAll()
    {
        if (!initialized)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }
    }

    /// <summary>Four-level stepped brightness used across effects: bright, medium, dim, off.</summary>
    public static Color SteppedFade(Color baseColor, float normalizedAge)
    {
        if (normalizedAge < 0.4f) return baseColor;
        if (normalizedAge < 0.7f) return baseColor * 0.75f;
        if (normalizedAge < 0.9f) return baseColor * 0.45f;
        return baseColor * 0.2f;
    }
}
