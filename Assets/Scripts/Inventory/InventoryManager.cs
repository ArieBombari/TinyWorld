// ============================================================================
// InventoryManager.cs — Uses Unity Input System (not legacy Input.GetKeyDown)
// ============================================================================
// References InputAction assets directly — works with keyboard + controller.
// Add OpenInventory, CloseInventory, NavigateInventory, SelectItem actions
// to your PlayerInputActions asset, then drag them into the fields below.
// ============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryData inventoryData;
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private InventoryCameraController cameraController;

        [Header("Input Actions (drag from PlayerInputActions asset)")]
        [SerializeField] private InputActionReference openInventoryAction;
        [SerializeField] private InputActionReference closeInventoryAction;
        [SerializeField] private InputActionReference navigateInventoryAction;
        [SerializeField] private InputActionReference selectItemAction;

        [Header("Player Movement Blocking")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private MonoBehaviour playerMovementOverride;

        [Header("Navigation")]
        [Tooltip("Cooldown between navigate ticks when holding a direction (seconds)")]
        [SerializeField] private float navigateRepeatDelay = 0.2f;

        [Header("State")]
        [SerializeField] private bool isOpen = false;

        public event System.Action OnInventoryOpened;
        public event System.Action OnInventoryClosed;
        public event System.Action<ItemData> OnItemSelected;
        public bool IsOpen => isOpen;

        private MonoBehaviour playerMovementScript;
        private float navigateCooldown = 0f;

        private void Awake()
        {
            if (inventoryData != null)
                inventoryData.EnsureSlots();

            if (playerMovementOverride != null)
                playerMovementScript = playerMovementOverride;
            else if (playerObject != null)
                playerMovementScript = FindMovementScript(playerObject);

            if (playerMovementScript != null)
                Debug.Log($"[InventoryManager] Movement script: {playerMovementScript.GetType().Name}");
        }

        private void OnEnable()
        {
            if (openInventoryAction != null)
            {
                openInventoryAction.action.Enable();
                openInventoryAction.action.performed += OnOpenInventory;
            }
            if (closeInventoryAction != null)
            {
                closeInventoryAction.action.Enable();
                closeInventoryAction.action.performed += OnCloseInventory;
            }
            if (navigateInventoryAction != null)
            {
                navigateInventoryAction.action.Enable();
            }
            if (selectItemAction != null)
            {
                selectItemAction.action.Enable();
                selectItemAction.action.performed += OnSelectItem;
            }
        }

        private void OnDisable()
        {
            if (openInventoryAction != null)
                openInventoryAction.action.performed -= OnOpenInventory;
            if (closeInventoryAction != null)
                closeInventoryAction.action.performed -= OnCloseInventory;
            if (selectItemAction != null)
                selectItemAction.action.performed -= OnSelectItem;
        }

        private void Update()
        {
            if (!isOpen) return;

            // Handle navigation (axis-based, with repeat delay)
            if (navigateInventoryAction != null)
            {
                float axis = navigateInventoryAction.action.ReadValue<float>();

                if (navigateCooldown > 0f)
                {
                    navigateCooldown -= Time.unscaledDeltaTime;
                }
                else if (Mathf.Abs(axis) > 0.5f)
                {
                    int direction = axis > 0 ? 1 : -1;
                    inventoryData.MoveActiveSlot(direction);
                    navigateCooldown = navigateRepeatDelay;
                }
            }

            // Scroll wheel (legacy — always works as a bonus)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.05f) inventoryData.MoveActiveSlot(-1);
            if (scroll < -0.05f) inventoryData.MoveActiveSlot(1);
        }

        // ----- Input Action Callbacks -----
        private void OnOpenInventory(InputAction.CallbackContext ctx)
        {
            if (!isOpen) OpenInventory();
            else CloseInventory();
        }

        private void OnCloseInventory(InputAction.CallbackContext ctx)
        {
            if (isOpen) CloseInventory();
        }

        private void OnSelectItem(InputAction.CallbackContext ctx)
        {
            if (!isOpen) return;
            SelectCurrentItem();
        }

        // ----- Open / Close -----
        public void OpenInventory()
        {
            if (isOpen) return;
            isOpen = true;
            navigateCooldown = 0f;
            SetPlayerMovement(false);
            inventoryUI?.Show();
            cameraController?.ZoomIn();
            OnInventoryOpened?.Invoke();
        }

        public void CloseInventory()
        {
            if (!isOpen) return;
            isOpen = false;
            SetPlayerMovement(true);
            inventoryUI?.Hide();
            cameraController?.ZoomOut();
            OnInventoryClosed?.Invoke();
        }

        private void SetPlayerMovement(bool enabled)
        {
            if (playerMovementScript != null)
                playerMovementScript.enabled = enabled;
        }

        private void SelectCurrentItem()
        {
            var slot = inventoryData.GetActiveSlot();
            if (slot == null || slot.IsEmpty) return;
            OnItemSelected?.Invoke(slot.item);
            Debug.Log($"[Inventory] Selected: {slot.item.itemName}");
        }

        // ----- Public API -----
        public bool GiveItem(ItemData item, int count = 1)
        {
            if (inventoryData == null || item == null) return false;
            bool success = inventoryData.AddItem(item, count);
            if (success) inventoryUI?.Refresh();
            return success;
        }

        public bool TakeItem(ItemData item, int count = 1)
        {
            if (inventoryData == null || item == null) return false;
            bool success = inventoryData.RemoveItem(item, count);
            if (success) inventoryUI?.Refresh();
            return success;
        }

        private MonoBehaviour FindMovementScript(GameObject player)
        {
            string[] names = {
                "DefaultPlayerMovement", "PlayerMovement", "PlayerController",
                "CharacterMovement", "MovementController"
            };
            foreach (var name in names)
                foreach (var mb in player.GetComponents<MonoBehaviour>())
                    if (mb.GetType().Name == name) return mb;
            foreach (var name in names)
                foreach (var mb in player.GetComponentsInChildren<MonoBehaviour>())
                    if (mb.GetType().Name == name) return mb;
            return null;
        }
    }
}
