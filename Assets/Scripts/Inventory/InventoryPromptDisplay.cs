// ============================================================================
// InventoryPromptDisplay.cs — Shows correct button labels for current device
// Attach to: PromptsDisplay GameObject
// ============================================================================
// Detects whether player last used keyboard or gamepad and updates the
// button letter text accordingly. E.g. "A Select" vs "Enter Select".
// ============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace Inventory
{
    public class InventoryPromptDisplay : MonoBehaviour
    {
        [Header("Select Prompt")]
        [SerializeField] private TextMeshProUGUI selectButtonLetter;
        [SerializeField] private TextMeshProUGUI selectLabel;

        [Header("Cancel Prompt")]
        [SerializeField] private TextMeshProUGUI cancelButtonLetter;
        [SerializeField] private TextMeshProUGUI cancelLabel;

        [Header("Keyboard Labels")]
        [SerializeField] private string keyboardSelect = "Enter";
        [SerializeField] private string keyboardCancel = "Esc";

        [Header("Gamepad Labels")]
        [SerializeField] private string gamepadSelect = "A";
        [SerializeField] private string gamepadCancel = "B";

        private bool lastWasGamepad = false;

        private void OnEnable()
        {
            // Check current device and subscribe to changes
            InputSystem.onActionChange += OnActionChange;
            DetectCurrentDevice();
        }

        private void OnDisable()
        {
            InputSystem.onActionChange -= OnActionChange;
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed) return;

            var action = obj as InputAction;
            if (action == null) return;

            var device = action.activeControl?.device;
            if (device == null) return;

            bool isGamepad = device is Gamepad;
            if (isGamepad != lastWasGamepad)
            {
                lastWasGamepad = isGamepad;
                UpdateLabels();
            }
        }

        private void DetectCurrentDevice()
        {
            // Default to keyboard, switch if gamepad is the most recent
            lastWasGamepad = Gamepad.current != null &&
                (Keyboard.current == null || !Keyboard.current.wasUpdatedThisFrame);
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (lastWasGamepad)
            {
                if (selectButtonLetter != null) selectButtonLetter.text = gamepadSelect;
                if (cancelButtonLetter != null) cancelButtonLetter.text = gamepadCancel;
            }
            else
            {
                if (selectButtonLetter != null) selectButtonLetter.text = keyboardSelect;
                if (cancelButtonLetter != null) cancelButtonLetter.text = keyboardCancel;
            }

            // Labels stay the same regardless of device
            if (selectLabel != null) selectLabel.text = "Select";
            if (cancelLabel != null) cancelLabel.text = "Cancel";
        }
    }
}
