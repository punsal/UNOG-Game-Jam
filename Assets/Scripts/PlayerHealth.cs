using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public event Action<int> HealthChanged;
    public event Action Died;

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 0.9f;

    private int currentHealth;
    private float invulnerabilityTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    private void Awake()
    {
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
        if (amount <= 0 || IsInvulnerable || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnerabilityTimer = invulnerabilityDuration;
        HealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Died?.Invoke();
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
        HealthChanged?.Invoke(currentHealth);
    }
}
