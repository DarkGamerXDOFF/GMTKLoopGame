using UnityEngine;
using System;

public class HealthSystem 
{
    private int health;
    private int maxHealth;

    public Action OnHealthChanged;

    public Action OnUnitDied;

    public HealthSystem(int maxHealth)
    {
        this.maxHealth = maxHealth;
        health = maxHealth;
    }

    public int GetHealth()
    {
        return health;
    }

    public float GetHealthPercentage()
    {
        return (float)health / maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            OnUnitDied?.Invoke();
        }
        OnHealthChanged?.Invoke();
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        OnHealthChanged?.Invoke();
    }

    //public bool IsDead() => health <= 0;
}
