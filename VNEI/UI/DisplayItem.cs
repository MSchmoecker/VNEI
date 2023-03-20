using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class DisplayItem : MonoBehaviour, IPointerClickHandler {
        private BaseUI baseUI;

        public Image image;
        public Image background;
        public Image favorite;
        public UITooltip uiTooltip;
        public Text countText;
        public Text qualityText;
        private Item item;
        private int quality;

        public static Color unknownColor = new Color(0.5f, 0.5f, 0.5f, .5f);

        private void Awake() {
            uiTooltip.m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
            background.sprite = GUIManager.Instance.GetSprite("item_background");
            Styling.ApplyText(countText, GUIManager.Instance.AveriaSerif, Color.white, 12);
            qualityText.font = GUIManager.Instance.AveriaSerifBold;
            favorite.color = GUIManager.Instance.ValheimOrange;
        }

        private void OnEnable() {
            Plugin.showModTooltip.SettingChanged += UpdateIconAndTooltip;
        }

        private void OnDestroy() {
            Plugin.showModTooltip.SettingChanged -= UpdateIconAndTooltip;
        }

        public void Update() {
            uiTooltip.m_showTimer = 1;
        }

        public void Init(BaseUI baseUI) {
            this.baseUI = baseUI;
        }

        public void SetItem(Item target, int quality) {
            if (item != null) {
                item.onFavoriteChanged.RemoveListener(this);
                item.onKnownChanged.RemoveListener(this);
            }

            item = target;
            this.quality = quality;
            image.gameObject.SetActive(item != null);
            UpdateFavorite();
            UpdateIconAndTooltip();

            if (item != null) {
                item.onFavoriteChanged.AddListener(this, UpdateFavorite);
                item.onKnownChanged.AddListener(this, UpdateIconAndTooltip);
            }
        }

        private void UpdateIconAndTooltip(object sender, EventArgs e) => UpdateIconAndTooltip();

        private void UpdateIconAndTooltip() {
            if (item != null) {
                if (item.IsKnown) {
                    image.color = Color.white;
                    image.sprite = item.GetIcon();
                    uiTooltip.Set(item.preLocalizeName, item.GetTooltip(quality));
                    qualityText.text = item.maxQuality > 1 || quality > 1 ? $"{quality}" : "";
                } else {
                    image.color = unknownColor;
                    image.sprite = Plugin.Instance.noIconSprite;
                    uiTooltip.Set("$vnei_unknown_item", "");
                    qualityText.text = "";
                }
            } else {
                image.sprite = null;
                uiTooltip.Set("", "");
                qualityText.text = "";
            }
        }

        private void UpdateFavorite() {
            favorite.gameObject.SetActive(item != null && item.isFavorite);
        }

        public static bool IsPlayerCheating() {
            return Player.m_localPlayer && SynchronizationManager.Instance.PlayerIsAdmin && Terminal.m_cheat;
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (item == null) {
                return;
            }

            if (!item.IsKnown) {
                return;
            }

            if (baseUI.TryGetComponent(out SelectUI selectUI)) {
                selectUI.SelectItem(item);
                return;
            }

            if (IsPlayerCheating() && (int)Plugin.itemCheatHotkey.Value == (int)eventData.button) {
                if (item.prefab) {
                    CheatItem();
                }
            } else if ((int)Plugin.removeRecentHotkey.Value == (int)eventData.button) {
                baseUI.RemoveItemFromLastViewedQueue(item);
            } else {
                baseUI.ShowRecipe(item, true);
            }
        }

        private void CheatItem() {
            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool isControlKeyDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (item.prefab.TryGetComponent(out ItemDrop itemDrop)) {
                Recipe recipe = ObjectDB.instance.GetRecipe(itemDrop.m_itemData);

                if (isControlKeyDown && recipe) {
                    foreach (Piece.Requirement resource in recipe.m_resources) {
                        SpawnItem(resource.m_resItem, resource.GetAmount(1), isShiftKeyDown);
                    }
                } else {
                    SpawnItem(item.prefab.GetComponent<ItemDrop>(), 1, isShiftKeyDown);
                }
            } else if (item.prefab.TryGetComponent(out Piece piece)) {
                foreach (Piece.Requirement resource in piece.m_resources) {
                    SpawnItem(resource.m_resItem, resource.GetAmount(1), isShiftKeyDown);
                }
            }
        }

        private static void SpawnItem(ItemDrop item, int amount, bool fullStack) {
            if (!item) {
                return;
            }

            int stackSize = fullStack ? item.m_itemData.m_shared.m_maxStackSize : amount;

            if (stackSize > 0) {
                Player.m_localPlayer.PickupPrefab(item.gameObject, stackSize);
            }
        }

        public void SetCount(string count) {
            countText.text = count;
            countText.gameObject.SetActive(true);
        }
    }
}
