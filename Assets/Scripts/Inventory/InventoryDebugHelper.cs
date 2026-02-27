using UnityEngine;

namespace Inventory
{
    public class InventoryDebugHelper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;

        [Header("Test Items (drag ItemData assets here)")]
        [SerializeField] private ItemData[] testItems;

        private void Update()
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                for (int i = 0; i < Mathf.Min(testItems.Length, 9); i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i) && testItems[i] != null)
                    {
                        bool added = inventoryManager.GiveItem(testItems[i]);
                        Debug.Log(added
                            ? $"[Debug] Added {testItems[i].itemName} (icon: {(testItems[i].icon != null ? testItems[i].icon.name : "NULL — assign icon on the asset!")})"
                            : $"[Debug] Could not add {testItems[i].itemName}");
                    }
                }
            }
        }
    }
}
