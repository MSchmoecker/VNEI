using UnityEngine.EventSystems;

namespace VNEI.UI {
    public class FavoriteTypeToggle : TypeToggle, IPointerDownHandler {
        private void Awake() {
            baseUI = GetComponentInParent<BaseUI>();
            baseUI.favoriteToggle = this;
            image.sprite = Plugin.Instance.starSprite;
            UpdateToggle();
        }

        public void OnPointerDown(PointerEventData eventData) {
            IsOn = !IsOn;
            baseUI.typeToggleChange?.Invoke();
        }
    }
}
