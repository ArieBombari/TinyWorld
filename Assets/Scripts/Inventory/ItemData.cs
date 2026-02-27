using UnityEngine;

namespace Inventory
{
    public enum ItemCategory
    {
        Tool, Material, Quest, Collectible, Consumable
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Game/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemName = "New Item";
        public string itemID = "item_new";
        [TextArea(2, 4)] public string description = "";

        [Header("Visuals")]
        public Sprite icon;
        public Color highlightColor = new Color(0.4f, 0.85f, 0.8f, 0.85f);

        [Header("Classification")]
        public ItemCategory category = ItemCategory.Tool;
        public bool stackable = false;
        public int maxStack = 1;
    }
}
