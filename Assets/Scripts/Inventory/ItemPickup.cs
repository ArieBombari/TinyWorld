using UnityEngine;
using UnityEngine.InputSystem;
using Inventory;

/// <summary>
/// Place on a world object to make it a collectible item.
/// Player walks near it and presses Interact (E) to pick it up.
/// Shows a prompt when in range.
///
/// Setup:
/// 1. Create a GameObject with a 3D model (boots, lantern, etc.)
/// 2. Add a SphereCollider (Is Trigger = true, radius ~2 for pickup range)
/// 3. Add this script, assign the ItemData ScriptableObject
/// 4. Optionally assign a RotateObject script for idle spinning
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int count = 1;

    [Header("Pickup Settings")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private bool showPrompt = true;
    [SerializeField] private string promptText = "Press E to pick up";

    [Header("Visual Feedback")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float spinSpeed = 60f;
    [SerializeField] private bool enableBob = true;
    [SerializeField] private bool enableSpin = true;

    [Header("Pickup Effect")]
    [SerializeField] private float flyToUIDuration = 0.5f;
    [SerializeField] private AnimationCurve flyToCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool playerInRange = false;
    private PlayerInputActions playerInput;
    private InventoryManager inventoryManager;
    private Vector3 startPosition;
    private bool pickedUp = false;

    void Start()
    {
        startPosition = transform.position;
        inventoryManager = FindObjectOfType<InventoryManager>();

        playerInput = new PlayerInputActions();
        playerInput.Player.Enable();
    }

    void Update()
    {
        if (pickedUp) return;

        // Don't process if dialogue is running
        var dialogueRunner = Yarn.Unity.DialogueRunner.FindObjectOfType<Yarn.Unity.DialogueRunner>();
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning) return;


        // Idle animation
        if (enableBob)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        if (enableSpin)
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        // Check for pickup input
        if (playerInRange && !pickedUp && playerInput.Player.Interact.WasPressedThisFrame())
{
        // Don't pick up if inventory is open
        if (inventoryManager != null && inventoryManager.IsOpen) return;
    
        TryPickup();
}
    }

    void TryPickup()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("[ItemPickup] No InventoryManager found!");
            return;
        }

        bool success = inventoryManager.GiveItem(itemData, count);

        if (success)
        {
            pickedUp = true;

            Debug.Log($"[ItemPickup] Picked up: {itemData.itemName}");

            // Notify ItemEffectManager to refresh
            if (ItemEffectManager.Instance != null)
                ItemEffectManager.Instance.ForceRefresh();

            // if (AudioManager.Instance != null)
            //     AudioManager.Instance.PlayPickup();  

            if (destroyOnPickup)
            {
                // Quick scale-down animation then destroy
                StartCoroutine(PickupAnimation());
            }
        }
        else
        {
            Debug.Log("[ItemPickup] Inventory full!");
        }
    }

    System.Collections.IEnumerator PickupAnimation()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.3f;

        // Quick pop up then shrink
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Pop up at start, then shrink
            float scale;
            if (t < 0.2f)
                scale = 1f + (t / 0.2f) * 0.3f; // grow to 1.3x
            else
                scale = 1.3f * (1f - ((t - 0.2f) / 0.8f)); // shrink to 0

            transform.localScale = originalScale * Mathf.Max(scale, 0f);
            transform.position += Vector3.up * 3f * Time.deltaTime; // float upward

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (pickedUp) return;
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (showPrompt)
        {
            // Show pickup prompt via your existing prompt system
            Debug.Log($"[ItemPickup] {promptText}: {itemData.itemName}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
    }

    void OnDestroy()
    {
        if (playerInput != null)
            playerInput.Player.Disable();
    }

    void OnDrawGizmosSelected()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, sphere.radius);
        }

        // Show item name
        if (itemData != null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, itemData.itemName);
#endif
        }
    }
}
