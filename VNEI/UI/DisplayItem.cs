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

        private void UpdateIconAndTooltip() {
            if (item != null) {
                if (item.IsKnown) {
                    image.color = Color.white;
                    image.sprite = item.GetIcon();
                    uiTooltip.Set(item.GetPrimaryName(), item.GetTooltip(quality));
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
                if (item.gameObject) {
                    if (item.gameObject.TryGetComponent(out ItemDrop itemDrop)) {
                        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                        int stackSize = isShiftKeyDown ? itemDrop.m_itemData.m_shared.m_maxStackSize : 1;
                        Player.m_localPlayer.PickupPrefab(item.gameObject, stackSize);
                    } else if (item.gameObject.TryGetComponent(out Piece piece)) {
                        foreach (Piece.Requirement resource in piece.m_resources) {
                            GameObject dropPrefab = ObjectDB.instance.GetItemPrefab(resource.m_resItem.name);
                            Player.m_localPlayer.PickupPrefab(dropPrefab, resource.m_amount);
                        }
                    }
                }
            } else if ((int)Plugin.removeRecentHotkey.Value == (int)eventData.button) {
                baseUI.RemoveItemFromLastViewedQueue(item);
            } else {
                baseUI.recipeUi.SetItem(item);
                baseUI.ShowRecipe();
            }
        }

        public void SetCount(string count) {
            countText.text = count;
            countText.gameObject.SetActive(true);
        }
    }
}
