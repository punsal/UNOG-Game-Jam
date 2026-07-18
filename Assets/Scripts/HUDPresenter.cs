using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Keeps HUD values synchronized with the current run.</summary>
public class HUDPresenter : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RelicLight relicLight;
    [SerializeField] private StageDirector stageDirector;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI lightText;

    private void OnEnable()
    {
        playerHealth.HealthChanged += HandleHealthChanged;
        relicLight.LightChanged += HandleLightChanged;
        stageDirector.StageChanged += HandleStageChanged;

        HandleHealthChanged(playerHealth.CurrentHealth);
        HandleLightChanged(relicLight.CurrentLight);
    }

    private void OnDisable()
    {
        playerHealth.HealthChanged -= HandleHealthChanged;
        relicLight.LightChanged -= HandleLightChanged;
        stageDirector.StageChanged -= HandleStageChanged;
    }

    private void HandleHealthChanged(int current)
    {
        healthText.text = "HP " + current + "/" + playerHealth.MaxHealth;
    }

    private void HandleLightChanged(float current)
    {
        lightText.text = "Light " + Mathf.RoundToInt(current) + "%";
    }

    private void HandleStageChanged(int index)
    {
        stageText.text = "Stage " + (index + 1) + "/6";
    }
}
