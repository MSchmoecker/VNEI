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
            RecipeUI.Instance.SetItem(item);
            BaseUI.Instance.ShowRecipe();
        }
    }
}
