using TMPro;
using UnityEngine;

/// <summary>Briefly shows the relief message when an accepted benefit lands.</summary>
public class BenefitToast : MonoBehaviour, IResettable
{
    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private BenefitApplier benefitApplier;
    [SerializeField] private float showSeconds = 3f;

    private float hideAt = -1f;

    private void Awake()
    {
        toastText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        benefitApplier.BenefitApplied += Show;
    }

    private void OnDisable()
    {
        benefitApplier.BenefitApplied -= Show;
    }

    private void Update()
    {
        if (hideAt >= 0f && Time.time >= hideAt)
        {
            Hide();
        }
    }

    /// <summary>General-purpose brief HUD message (also used by dev toggles).</summary>
    public void ShowMessage(string message)
    {
        Show(message);
    }

    private void Show(string message)
    {
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        hideAt = Time.time + showSeconds;
    }

    private void Hide()
    {
        hideAt = -1f;
        toastText.gameObject.SetActive(false);
    }

    public void ResetRun()
    {
        Hide();
    }
}
