using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace VNEI.UI {
    public class TypeToggle : MonoBehaviour {
        public Image image;
        private Toggle toggle;

        private void Awake() {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(UpdateToggle);
            GetComponent<UITooltip>().m_tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip");
            GetComponent<UITooltip>().m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
        }

        private void UpdateToggle(bool isOn) {
            image.color = isOn ? Color.white : new Color(0, 0, 0, 0.25f);
        }
    }
}
