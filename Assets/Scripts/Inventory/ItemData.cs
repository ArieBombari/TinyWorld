using UnityEngine;

namespace Inventory
{
    public enum ItemCategory
    {
        Tool, Material, Quest, Collectible, Consumable
    }

    public enum ItemAbility
    {
        None,
        Boots,          // Resist river current, swim upstream
        FishingRod,     // Fishing (later)
        Lantern,        // Light in dark caves (active — must be selected)
        Wings,          // Glide 1.2x faster
        Hat,            // Resist cold, prevent feather freezing
        Map             // Open world map (active — must be selected)
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

        [Header("Ability")]
        public ItemAbility ability = ItemAbility.None;
    }
}
