using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IResettable
{
    public event Action<int> HealthChanged;
    public event Action Died;
    // Presentation-only: direction the hit came from (normalized source->player), Vector2.zero when unknown.
    public event Action<Vector2> Damaged;

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 0.9f;

    private int baselineMaxHealth;
    private int currentHealth;
    private float invulnerabilityTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    private void Awake()
    {
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
        if (amount <= 0 || IsInvulnerable || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnerabilityTimer = invulnerabilityDuration;
        GameAudio.Instance?.PlayDamage();
        HealthChanged?.Invoke(currentHealth);

        Vector2 hitDirection = (Vector2)transform.position - sourceWorldPosition;
        Damaged?.Invoke(hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : Vector2.zero);

        if (currentHealth <= 0)
        {
            Died?.Invoke();
        }
    }

    public void ApplyMaxHealthModifier(int delta)
    {
        maxHealth = Mathf.Max(1, maxHealth + delta);
        currentHealth = Mathf.Clamp(currentHealth + delta, 0, maxHealth);
        HealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Died?.Invoke();
        }
    }

    public void ResetRun()
    {
        maxHealth = baselineMaxHealth;
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
        HealthChanged?.Invoke(currentHealth);
    }
}
