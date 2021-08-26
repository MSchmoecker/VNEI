using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class MouseHover : MonoBehaviour, IPointerClickHandler {
        public Image image;
        public Item item;

        private void Awake() {
            GetComponent<UITooltip>().m_tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip");
            GetComponent<UITooltip>().m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
        }

        public void SetItem(Item item) {
            this.item = item;
            GetComponent<UITooltip>().Set(item.localizedName, item.GetTooltip());
        }

        public void OnPointerClick(PointerEventData eventData) {
            RecipeUI.Instance.SetItem(item);
            BaseUI.Instance.ShowRecipe();
        }
    }
}
