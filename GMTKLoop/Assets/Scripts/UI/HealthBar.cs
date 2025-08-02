using UnityEngine;

public class HealthBar : MonoBehaviour
{
    HealthSystem healthSystem;
    
    public void Setup(HealthSystem healthSystem)
    {
        this.healthSystem = healthSystem;
        healthSystem.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar()
    {
        if (healthSystem == null)
        {
            Debug.LogError("HealthSystem is not set up for HealthBar.");
            return;
        }
        transform.GetChild(0).localScale = new Vector3(healthSystem.GetHealthPercentage(), 1, 1);
    }
}
