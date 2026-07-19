using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Converts pointer dragging into horizontal input.</summary>
public class InputReader : MonoBehaviour
{
    public event Action<float> OnDrag;

    // One comfortable half-screen swipe should cross most of the corridor.
    [SerializeField] private float dragSensitivity = 2.2f;

    private bool isDragging;
    private Vector2 lastPointerPosition;
    private bool inputEnabled = true;

    public void SetInputEnabled(bool enabled)
    {
        if (inputEnabled != enabled)
        {
            Debug.Log($"Player input {(enabled ? "enabled" : "disabled")}.", this);
        }
        inputEnabled = enabled;
        isDragging = false;
    }

    private void Update()
    {
        if (!inputEnabled)
        {
            return;
        }

        if (!TryGetPointer(out Vector2 currentPosition, out bool pressedThisFrame, out bool isPressed))
        {
            isDragging = false;
            return;
        }

        if (!isPressed)
        {
            isDragging = false;
            return;
        }

        if (pressedThisFrame || !isDragging)
        {
            isDragging = true;
            lastPointerPosition = currentPosition;
            return;
        }

        float deltaPixels = currentPosition.x - lastPointerPosition.x;
        lastPointerPosition = currentPosition;

        if (deltaPixels == 0f)
        {
            return;
        }

        float normalizedX = deltaPixels / Screen.width * dragSensitivity;
        OnDrag?.Invoke(normalizedX);
    }

    // Touch takes priority so a held mouse button on desktop/editor still works as a fallback.
    private bool TryGetPointer(out Vector2 position, out bool pressedThisFrame, out bool isPressed)
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed || touch.press.wasReleasedThisFrame)
            {
                position = touch.position.ReadValue();
                pressedThisFrame = touch.press.wasPressedThisFrame;
                isPressed = touch.press.isPressed;
                return true;
            }
        }

        if (Mouse.current != null)
        {
            position = Mouse.current.position.ReadValue();
            pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            isPressed = Mouse.current.leftButton.isPressed;
            return true;
        }

        position = default;
        pressedThisFrame = false;
        isPressed = false;
        return false;
    }
}
