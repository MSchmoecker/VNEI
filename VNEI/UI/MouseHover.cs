using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    // TODO rename class
    public class MouseHover : MonoBehaviour, IPointerClickHandler {
        public Image image;
        public Item item;
        public bool isActive;

        private void Awake() {
            GetComponent<UITooltip>().m_tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip");
            GetComponent<UITooltip>().m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
        }

        public void SetItem(Item item) {
            this.item = item;

            string topic = item.localizedName.Length > 0 ? item.localizedName : item.internalName;
            string tooltip = item.GetTooltip();
            tooltip = tooltip.Length > 0 ? tooltip : item.description;
            GetComponent<UITooltip>().Set(topic, tooltip);
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (SearchUI.Instance.IsCheating()) {
                if (item.itemType == ItemType.Item) {
                    ItemDrop itemDrop = item.gameObject.GetComponent<ItemDrop>();
                    if ((bool)itemDrop) {
                        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                        int stackSize = isShiftKeyDown ? itemDrop.m_itemData.m_shared.m_maxStackSize : 1;
                        Player.m_localPlayer.PickupPrefab(item.gameObject, stackSize);
                    } else {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"item '{item.internalName}' has no ItemDrop");
                    }
                } else {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "only item spawning is possible");
                }
            } else {
                RecipeUI.Instance.SetItem(item);
                BaseUI.Instance.ShowRecipe();
            }
        }
    }
}
