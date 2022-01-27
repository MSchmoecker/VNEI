namespace VNEI.Logic {
    public class ItemTypeHelper {
        public static ItemType GetItemType(ItemDrop.ItemData itemData) {
            switch (itemData.m_shared.m_itemType) {
                case ItemDrop.ItemData.ItemType.Consumable:
                    return ItemType.Food;
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Shoulder:
                    return ItemType.Armor;
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.Bow:
                case ItemDrop.ItemData.ItemType.Shield:
                    return ItemType.Weapon;
                default:
                    return ItemType.Undefined;
            }
        }
    }
}
