using UnityEngine.EventSystems;
using VNEI.Logic;

namespace VNEI.UI {
    public class ItemTypeToggle : TypeToggle, IPointerDownHandler {
        public ItemType itemType;

        private void Awake() {
            baseUI = GetComponentInParent<BaseUI>();
            baseUI.typeToggles.Add(this);
            UpdateToggle();
        }

        private void OnDestroy() {
            baseUI.typeToggles.Remove(this);
        }

        public bool IsDisabled(ItemType type) {
            return !IsOn && type == itemType;
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Right) {
                bool turnOthersOn = IsOn && baseUI.typeToggles.TrueForAll(i => !i.IsOn || i == this);
                IsOn = true;

                foreach (ItemTypeToggle toggle in baseUI.typeToggles) {
                    if (toggle == this) {
                        continue;
                    }

                    toggle.IsOn = turnOthersOn;
                }
            } else {
                IsOn = !IsOn;
            }

            baseUI.typeToggleChange?.Invoke();
        }
    }
}
