using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
        [SerializeField] private Transform textBase;
        [SerializeField] private Text text;
        private Item item;

        public void SetItem(Item item) {
            this.item = item;
            text.text = item.localizedName;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            textBase.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            textBase.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData) {
            RecipeUI.Instance.SetItem(item);
            BaseUI.Instance.ShowRecipe();
        }
    }
}
