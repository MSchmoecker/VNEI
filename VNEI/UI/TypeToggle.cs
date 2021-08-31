using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VNEI.UI {
    public class TypeToggle : MonoBehaviour, IPointerDownHandler {
        public Image image;
        public Image background;
        public static event Action OnChange;
        public bool isOn = true;
        private static List<TypeToggle> typeToggles = new List<TypeToggle>();

        private void Awake() {
            typeToggles.Add(this);
            GetComponent<UITooltip>().m_tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip");
            GetComponent<UITooltip>().m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
            background.sprite = GUIManager.Instance.GetSprite("checkbox");
            background.pixelsPerUnitMultiplier = 2.5f;
            UpdateToggle();
        }

        private void UpdateToggle() {
            image.color = isOn ? Color.white : new Color(0, 0, 0, 0.5f);
        }

        private void OnDestroy() {
            typeToggles.Remove(this);
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Right) {
                isOn = true;
                UpdateToggle();

                foreach (TypeToggle toggle in typeToggles) {
                    if (toggle == this) {
                        continue;
                    }

                    toggle.isOn = false;
                    toggle.UpdateToggle();
                }
            } else {
                isOn = !isOn;
                UpdateToggle();
            }

            OnChange?.Invoke();
        }
    }
}
