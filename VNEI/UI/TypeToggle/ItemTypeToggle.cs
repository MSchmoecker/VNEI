using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using VNEI.Logic;

namespace VNEI.UI {
    public class ItemTypeToggle : TypeToggle, IPointerDownHandler {
        public ItemType itemType;
        public int order;

        protected override void Awake() {
            base.Awake();
            baseUI.typeToggles.Add(this);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            baseUI.typeToggles.Remove(this);
        }

        public bool IsDisabled(ItemType type) {
            return !IsOn && type == itemType;
        }

        protected override void UpdateItemCount() {
            int activeItems = Indexing.GetActiveItems().Count(i => i.Value.itemType == itemType);
            tooltip.m_text = Localization.instance.Localize("$vnei_items_in_category", activeItems.ToString());
            IsEnabled = activeItems > 0;
            baseUI.searchUi.UpdateFilters();
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
