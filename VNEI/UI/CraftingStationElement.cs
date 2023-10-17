using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class CraftingStationElement : MonoBehaviour {
        public Image icon;
        public Button button;
        public UITooltip tooltip;

        [SerializeField]
        private Item station;

        public Item Station {
            get => station;
            set {
                station?.onKnownChanged.RemoveListener(this);
                station = value;
                station?.onKnownChanged.AddListener(this, UpdateIconAndTooltip);
            }
        }

        public void UpdateIconAndTooltip() {
            if (Station != null) {
                if (Station.IsKnown) {
                    icon.color = Color.white;
                    icon.sprite = Station.GetIcon();
                    tooltip.Set(Station.preLocalizeName, "");
                } else {
                    icon.color = DisplayItem.unknownColor;
                    icon.sprite = Plugin.Instance.noIconSprite;
                    tooltip.Set("$vnei_unknown_item", "");
                }
            } else {
                icon.sprite = null;
                tooltip.Set("", "");
            }
        }

        private void OnDestroy() {
            Station?.onKnownChanged.RemoveListener(this);
        }
    }
}
