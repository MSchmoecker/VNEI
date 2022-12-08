using System.Linq;
using UnityEngine.EventSystems;
using VNEI.Logic;

namespace VNEI.UI {
    public class FavoriteTypeToggle : TypeToggle, IPointerDownHandler {
        protected override void Awake() {
            base.Awake();
            baseUI.favoriteToggle = this;
            image.sprite = Plugin.Instance.starSprite;

            Item.OnAnyFavoriteChanged += UpdateItemCount;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Item.OnAnyFavoriteChanged -= UpdateItemCount;
        }

        protected override void UpdateItemCount() {
            int activeItems = Indexing.GetActiveItems().Count(i => i.Value.isFavorite);
            tooltip.m_text = Localization.instance.Localize("$vnei_items_in_category", activeItems.ToString());
        }

        public void OnPointerDown(PointerEventData eventData) {
            IsOn = !IsOn;
            baseUI.typeToggleChange?.Invoke();
        }
    }
}
