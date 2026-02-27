// ============================================================================
// InventoryDiagnostic.cs — Checks all wiring on Play and logs results
// Attach to: InventoryManager GameObject, then DELETE after fixing issues
// ============================================================================

using UnityEngine;

namespace Inventory
{
    public class InventoryDiagnostic : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== INVENTORY DIAGNOSTIC START ===");

            // Check InventoryManager
            var mgr = GetComponent<InventoryManager>();
            if (mgr == null)
            {
                Debug.LogError("[DIAG] No InventoryManager on this GameObject!");
                return;
            }
            Debug.Log("[DIAG] InventoryManager found ✓");

            // Use reflection to check serialized fields
            var mgrType = mgr.GetType();
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            CheckField(mgr, mgrType, "inventoryData", flags);
            CheckField(mgr, mgrType, "inventoryUI", flags);
            CheckField(mgr, mgrType, "cameraController", flags);
            CheckField(mgr, mgrType, "openInventoryAction", flags);
            CheckField(mgr, mgrType, "closeInventoryAction", flags);
            CheckField(mgr, mgrType, "navigateInventoryAction", flags);
            CheckField(mgr, mgrType, "selectItemAction", flags);
            CheckField(mgr, mgrType, "playerObject", flags);

            // Check FeatherUI
            Debug.Log("--- Feather UI ---");
            if (FeatherUI.Instance == null)
            {
                Debug.LogError("[DIAG] FeatherUI.Instance is NULL! Either no FeatherUI in scene, or it was destroyed by duplicate singleton.");

                // Find all FeatherUI components
                var allFeatherUIs = FindObjectsOfType<FeatherUI>(true);
                Debug.Log($"[DIAG] Found {allFeatherUIs.Length} FeatherUI components in scene (including inactive)");
                foreach (var fui in allFeatherUIs)
                    Debug.Log($"[DIAG]   - On: {fui.gameObject.name}, Active: {fui.gameObject.activeSelf}");
            }
            else
            {
                Debug.Log($"[DIAG] FeatherUI.Instance found ✓ on '{FeatherUI.Instance.gameObject.name}'");
                Debug.Log($"[DIAG] FeatherUI GameObject active: {FeatherUI.Instance.gameObject.activeSelf}");

                var fuiType = FeatherUI.Instance.GetType();
                var spriteField = fuiType.GetField("iconSprite", flags);
                if (spriteField != null)
                {
                    var sprite = spriteField.GetValue(FeatherUI.Instance) as Sprite;
                    if (sprite == null)
                        Debug.LogError("[DIAG] FeatherUI.iconSprite is NULL — no feather sprite assigned!");
                    else
                        Debug.Log($"[DIAG] FeatherUI.iconSprite = '{sprite.name}' ✓");
                }

                // Check RectTransform
                var rt = FeatherUI.Instance.GetComponent<RectTransform>();
                if (rt != null)
                    Debug.Log($"[DIAG] FeatherUI RectTransform: pos=({rt.anchoredPosition.x}, {rt.anchoredPosition.y}), size=({rt.sizeDelta.x}, {rt.sizeDelta.y})");

                // Check Canvas
                var canvas = FeatherUI.Instance.GetComponentInParent<Canvas>();
                if (canvas != null)
                    Debug.Log($"[DIAG] Parent Canvas: '{canvas.gameObject.name}', renderMode={canvas.renderMode}");
                else
                    Debug.LogError("[DIAG] FeatherUI has no parent Canvas!");
            }

            // Check FeatherManager
            Debug.Log("--- Feather Manager ---");
            if (FeatherManager.Instance == null)
                Debug.LogError("[DIAG] FeatherManager.Instance is NULL!");
            else
                Debug.Log($"[DIAG] FeatherManager.Instance found ✓, collected={FeatherManager.Instance.GetFeathersCollected()}, current={FeatherManager.Instance.GetCurrentFeathers()}");

            // Check InventoryUI reference
            Debug.Log("--- InventoryUI ---");
            var allInvUI = FindObjectsOfType<InventoryUI>(true);
            Debug.Log($"[DIAG] Found {allInvUI.Length} InventoryUI components in scene");
            foreach (var ui in allInvUI)
                Debug.Log($"[DIAG]   - On: '{ui.gameObject.name}', Active: {ui.gameObject.activeSelf}, Destroyed: {ui == null}");

            Debug.Log("=== INVENTORY DIAGNOSTIC END ===");
        }

        private void CheckField(object obj, System.Type type, string fieldName, System.Reflection.BindingFlags flags)
        {
            var field = type.GetField(fieldName, flags);
            if (field == null)
            {
                Debug.LogWarning($"[DIAG] Field '{fieldName}' not found on {type.Name}");
                return;
            }

            var value = field.GetValue(obj);
            if (value == null || value.Equals(null))
                Debug.LogError($"[DIAG] {type.Name}.{fieldName} = NULL ← ASSIGN THIS!");
            else
                Debug.Log($"[DIAG] {type.Name}.{fieldName} = {value} ✓");
        }
    }
}
