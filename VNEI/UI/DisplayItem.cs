using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class DisplayItem : MonoBehaviour, IPointerClickHandler {
        public Image image;
        public UITooltip uiTooltip;
        public Text countText;
        public Item item;

        private void Awake() {
            uiTooltip.m_tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip");
            uiTooltip.m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
            Styling.ApplyText(countText, GUIManager.Instance.AveriaSerif, Color.white);
        }

        public void SetItem(Item target, int quality) {
            item = target;
            image.sprite = item.GetIcon();
            uiTooltip.Set(item.GetPrimaryName(), item.GetTooltip(quality));
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (SearchUI.Instance.IsCheating() && eventData.button == PointerEventData.InputButton.Right) {
                if (item.itemType == ItemType.Item && (bool)item.gameObject.GetComponent<ItemDrop>()) {
                    ItemDrop itemDrop = item.gameObject.GetComponent<ItemDrop>();
                    bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    int stackSize = isShiftKeyDown ? itemDrop.m_itemData.m_shared.m_maxStackSize : 1;
                    Player.m_localPlayer.PickupPrefab(item.gameObject, stackSize);
                } else {
                    Transform playerTransform = Player.m_localPlayer.transform;
                    Instantiate(item.gameObject, playerTransform.position + playerTransform.forward * 2f, Quaternion.identity);
                }
            } else {
                RecipeUI.Instance.SetItem(item);
                BaseUI.Instance.ShowRecipe();
            }
        }

        public void SetCount(string count) {
            countText.text = count;
            countText.gameObject.SetActive(true);
        }
    }
}
