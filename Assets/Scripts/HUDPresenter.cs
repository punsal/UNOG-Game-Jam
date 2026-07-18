using UnityEngine;
using UnityEngine.UI;

public class HUDPresenter : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RelicLight relicLight;
    [SerializeField] private StageDirector stageDirector;
    [SerializeField] private Text healthText;
    [SerializeField] private Text stageText;
    [SerializeField] private Text lightText;

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
