using System;
using UnityEngine;

/// <summary>Tracks player health, damage immunity, and death.</summary>
public class PlayerHealth : MonoBehaviour, IResettable
{
    public event Action<int> HealthChanged;
    public event Action Died;
    // Presentation-only: direction the hit came from (normalized source->player), Vector2.zero when unknown.
    public event Action<Vector2> Damaged;

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 0.9f;
    // Recording aid: hazards cannot damage the player; altars work normally.
    // Deliberately sticky across ResetRun so a recording can span restarts.
    [SerializeField] private bool godMode;

    private int baselineMaxHealth;
    private int currentHealth;
    private float invulnerabilityTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;
    public bool GodMode => godMode;

    public void SetGodMode(bool enabled)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        godMode = enabled;
        Debug.Log($"God mode {(enabled ? "enabled" : "disabled")}.", this);
#else
        godMode = false;
#endif
    }

    private void Awake()
    {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        // Release builds can never run god mode, even if the scene shipped
        // with the flag serialized on.
        godMode = false;
#endif
        baselineMaxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    public void ApplyDamage(int amount)
    {
        ApplyDamage(amount, (Vector2)transform.position);
    }

    public void ApplyDamage(int amount, Vector2 sourceWorldPosition)
    {
        if (godMode || amount <= 0 || IsInvulnerable || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"Player took {amount} damage. Health: {currentHealth}/{maxHealth}.", this);
        invulnerabilityTimer = invulnerabilityDuration;
        GameAudio.Instance?.PlayDamage();
        HealthChanged?.Invoke(currentHealth);

        Vector2 hitDirection = (Vector2)transform.position - sourceWorldPosition;
        Damaged?.Invoke(hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : Vector2.zero);

        if (currentHealth <= 0)
        {
            Debug.Log("Player died.", this);
            Died?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0 || currentHealth >= maxHealth)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed {amount}. Health: {currentHealth}/{maxHealth}.", this);
        HealthChanged?.Invoke(currentHealth);
    }

    public void ApplyMaxHealthModifier(int delta)
    {
        maxHealth = Mathf.Max(1, maxHealth + delta);
        // A blood cost reshapes the vessel but cannot kill: a living player's
        // current health clamps into [1, max] rather than taking the delta as
        // damage (hard cap "Minimum 1" in the cost tuning table).
        if (currentHealth > 0)
        {
            currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
        }
        Debug.Log($"Max health modified by {delta}. Health: {currentHealth}/{maxHealth}.", this);
        HealthChanged?.Invoke(currentHealth);
    }

    public void ResetRun()
    {
        maxHealth = baselineMaxHealth;
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
        HealthChanged?.Invoke(currentHealth);
    }
}
