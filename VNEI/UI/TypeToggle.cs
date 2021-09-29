using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class TypeToggle : MonoBehaviour, IPointerDownHandler {
        public static event Action OnChange;

        public Image image;
        public Image background;
        public bool isOn = true;
        public ItemType itemType;

        public static List<TypeToggle> typeToggles = new List<TypeToggle>();

        private void Awake() {
            typeToggles.Add(this);
            GetComponent<UITooltip>().m_tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip");
            GetComponent<UITooltip>().m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
            background.sprite = GUIManager.Instance.GetSprite("checkbox");
            background.pixelsPerUnitMultiplier = 2.5f;
            background.color = new Color(0.61f, 0.61f, 0.61f, 1f);
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
                bool turnOthersOn = isOn && typeToggles.TrueForAll(i => !i.isOn || i == this);

                isOn = true;
                UpdateToggle();

                foreach (TypeToggle toggle in typeToggles) {
                    if (toggle == this) {
                        continue;
                    }

                    toggle.isOn = turnOthersOn;
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
