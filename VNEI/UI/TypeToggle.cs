using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class TypeToggle : MonoBehaviour, IPointerDownHandler {
        public static event Action OnChange; // TODO remove static?

        public Image image;
        public Image background;
        public bool isOn = true;
        public bool toggleItemType;
        public ItemType itemType;

        public bool toggleFavorite;
        public bool isFavorite;

        private BaseUI baseUI;

        private void Awake() {
            baseUI = GetComponentInParent<BaseUI>();
            baseUI.typeToggles.Add(this);
            UpdateToggle();
        }

        private void UpdateToggle() {
            image.color = isOn ? Color.white : new Color(0, 0, 0, 0.5f);
        }

        private void OnDestroy() {
            baseUI.typeToggles.Remove(this);
        }

        public void SetOn(bool on) {
            isOn = on;
            UpdateToggle();
        }

        public bool IsDisabled(ItemType type, bool favorite) {
            return !isOn && toggleItemType && type == itemType || isOn && toggleFavorite && favorite != isFavorite;
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Right) {
                bool turnOthersOn = isOn && baseUI.typeToggles.TrueForAll(i => !i.isOn || i == this);

                isOn = true;
                UpdateToggle();

                foreach (TypeToggle toggle in baseUI.typeToggles) {
                    if (toggle == this) {
                        continue;
                    }

                    if (toggle.toggleItemType != toggleItemType && toggle.toggleFavorite != toggleFavorite) {
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
