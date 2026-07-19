using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Recording aid: five quick taps in the top-left corner flip god mode.</summary>
public class GodModeToggle : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BenefitToast toast;
    [SerializeField] private int tapsRequired = 5;
    [SerializeField] private float windowSeconds = 2f;

    private int taps;
    private float windowStart;

    private void Update()
    {
        if (!TapThisFrame(out Vector2 pos))
        {
            return;
        }

        // Only taps in the top-left corner count (the HP label area).
        if (pos.x > Screen.width * 0.25f || pos.y < Screen.height * 0.85f)
        {
            taps = 0;
            return;
        }

        if (taps == 0 || Time.unscaledTime - windowStart > windowSeconds)
        {
            taps = 0;
            windowStart = Time.unscaledTime;
        }

        taps++;
        if (taps >= tapsRequired)
        {
            taps = 0;
            bool enabled = !playerHealth.GodMode;
            playerHealth.SetGodMode(enabled);
            if (toast != null)
            {
                toast.ShowMessage(enabled ? "God mode on" : "God mode off");
            }
        }
    }

    private bool TapThisFrame(out Vector2 pos)
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            pos = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pos = Mouse.current.position.ReadValue();
            return true;
        }
        pos = default;
        return false;
    }
}
